using System;
using UnityEngine;

public enum GrillState
{
    Idle, // nothing inside
    Grilling, // raw item is grilling
    HalfDone, // intermediate stage
    Done, // properly grilled
    Burnt // overcooked
}

public class GrillMachine : BaseMachine<GrillMachineData>, IHasProgress
{
    private GrillState currentState = GrillState.Idle;

    private DateTime? grillStartTime;
    private float grillProgress;
    private bool isLoadNeeded = true;

    private bool isBurnt = false;

    [SerializeField] private GrillRecipeSo[] grillRecipeSoArray;

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnGrillInteractSuccess;

    protected override string GetSaveKey() => "grillMachine";

    private void Update()
    {
        if (isLoadNeeded)
        {
            Load();
            isLoadNeeded = false;
        }

        if (!HasKitchenObject())
        {
            SetState(GrillState.Idle);
            return;
        }

        if (isBurnt) return;

        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
        var recipe = GetGrillRecipeSoWithState(currentObjectSo);
        if (recipe == null) return;

        if (grillStartTime == null)
            grillStartTime = DateTime.UtcNow;

        double elapsed = (DateTime.UtcNow - grillStartTime.Value).TotalSeconds;
        grillProgress = (float)elapsed;

        UpdateProgress(recipe);

        // Handle state transitions
        if (currentObjectSo == recipe.input && grillProgress >= recipe.interMediateGrillTime)
        {
            SetState(GrillState.HalfDone, recipe);
        }
        else if (currentObjectSo == recipe.intermediate && grillProgress >= recipe.grillProgressMax)
        {
            SetState(GrillState.Done, recipe);
        }
        else if (currentObjectSo == recipe.output && grillProgress >= recipe.burntGrillTime)
        {
            SetState(GrillState.Burnt, recipe);
        }
        else
        {
            SetState(GrillState.Grilling);
        }

        Save();
    }

    public override void Interact()
    {
        if (HasKitchenObject())
        {
            HandlePickup();
            Save();
            return;
        }

        var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
        if (selectedObjectSo == null) return;

        var recipe = GetGrillRecipeSoWithState(selectedObjectSo);
        if (recipe == null || selectedObjectSo != recipe.input) return;

        var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
        KitchenObject.SpawnKitchenObject(takenObject, this);

        grillStartTime = DateTime.UtcNow;
        SetState(GrillState.Grilling);
        ResetProgress();
        Save();
    }

    private void HandlePickup()
    {
        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();

        if (!InventoryManager.Instance.TryPickupObject(currentObjectSo)) return;

        KitchenObject.DestroyKitchenObject(this);
        SetState(GrillState.Idle);
    }

    private void SetState(GrillState newState, GrillRecipeSo recipe = null)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case GrillState.Idle:
                grillStartTime = null;
                grillProgress = 0f;
                isBurnt = false;
                SoundManager.Instance.StopLoopingSound("grill-sizzle");
                break;

            case GrillState.Grilling:
                grillStartTime ??= DateTime.UtcNow;
                SoundManager.Instance.PlayLoopingSound("grill-sizzle", machineTopPoint.position, false);
                break;

            case GrillState.HalfDone:
                if (recipe != null)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.intermediate, this);
                }
                break;

            case GrillState.Done:
                if (recipe != null)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.output, this);
                }
                SoundManager.Instance.StopLoopingSound("grill-sizzle");
                break;

            case GrillState.Burnt:
                if (recipe != null)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.burntOutput, this);
                }
                isBurnt = true;
                SoundManager.Instance.StopLoopingSound("grill-sizzle");
                break;
        }
    }

    private void ResetProgress()
    {
        grillProgress = 0f;
        isBurnt = false;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    private void UpdateProgress(GrillRecipeSo recipe)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = Mathf.Clamp01(grillProgress / recipe.grillProgressMax)
        });

        OnGrillInteractSuccess?.Invoke(this, EventArgs.Empty);
    }

    private GrillRecipeSo GetGrillRecipeSoWithState(KitchenObjectSo stateSo)
    {
        foreach (var recipe in grillRecipeSoArray)
        {
            if (recipe.input == stateSo || recipe.intermediate == stateSo || recipe.output == stateSo || recipe.burntOutput == stateSo)
                return recipe;
        }
        return null;
    }

    public override GrillMachineData GetSaveData()
    {
        var data = new GrillMachineData();

        if (HasKitchenObject())
        {
            data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
            data.grillStartTimestamp = grillStartTime?.ToBinary() ?? 0;
            data.isBurnt = isBurnt;
        }
        else
        {
            data.kitchenObjectId = "";
            data.grillStartTimestamp = 0;
            data.isBurnt = false;
        }

        return data;
    }

    public override void LoadFromSaveData(GrillMachineData data)
    {
        grillStartTime = data.grillStartTimestamp != 0 ? DateTime.FromBinary(data.grillStartTimestamp) : (DateTime?)null;
        isBurnt = data.isBurnt;

        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

                var recipe = GetGrillRecipeSoWithState(kitchenObjectSo);
                if (recipe != null)
                {
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = Mathf.Clamp01(grillProgress / recipe.grillProgressMax)
                    });
                }
            }
        }
    }
}
