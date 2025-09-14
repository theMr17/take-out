using System;
using UnityEngine;

public enum FryerState
{
    Idle, // nothing inside
    Frying, // raw item is frying
    HalfDone, // intermediate stage (e.g., par-fried)
    Done // fries ready
}

public class FryerMachine : BaseMachine<FryerMachineData>, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnFryInteractSuccess;

    [SerializeField] private FrenchFryRecipeSo[] frenchFriesRecipeSoArray;

    private FryerState currentState = FryerState.Idle;
    private DateTime? fryStartTime;
    private float fryProgress;

    private bool isLoadNeeded = true;

    protected override string GetSaveKey() => "fryerMachine";

    private void Update()
    {
        if (isLoadNeeded)
        {
            Load();
            isLoadNeeded = false;
        }

        if (!HasKitchenObject())
        {
            SetState(FryerState.Idle);
            return;
        }

        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
        var recipe = GetFriesRecipeSoWithInput(currentObjectSo);
        if (recipe == null) return;

        if (fryStartTime == null)
            fryStartTime = DateTime.UtcNow;

        double elapsed = (DateTime.UtcNow - fryStartTime.Value).TotalSeconds;
        fryProgress = Mathf.Min((float)elapsed, recipe.fryProgressMax);

        UpdateProgress(recipe);

        if (fryProgress >= recipe.fryProgressMax)
        {
            SetState(FryerState.Done);
        }
        else if (fryProgress >= recipe.interMediateFryTime && currentObjectSo != recipe.intermediate)
        {
            SetState(FryerState.HalfDone);
        }
        else
        {
            SetState(FryerState.Frying);
        }

        Save();
    }

    public override void Interact()
    {
        if (HasKitchenObject())
        {
            HandleFriesPickup();
            Save();
            return;
        }

        var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
        if (selectedObjectSo == null || !HasRecipeWithInput(selectedObjectSo)) return;

        var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
        KitchenObject.SpawnKitchenObject(takenObject, this);

        fryStartTime = DateTime.UtcNow;
        SetState(FryerState.Frying);
        ResetProgress();
        Save();
    }

    public override void InteractAlternate()
    {
        return; // No alternate interaction needed for fryer
    }

    private void HandleFriesPickup()
    {
        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();

        if (!InventoryManager.Instance.TryPickupObject(currentObjectSo)) return;

        KitchenObject.DestroyKitchenObject(this);
        SetState(FryerState.Idle);
    }

    private void SetState(FryerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case FryerState.Idle:
                fryStartTime = null;
                fryProgress = 0f;
                SoundManager.Instance.StopLoopingSound("frying");
                break;

            case FryerState.Frying:
                fryStartTime ??= DateTime.UtcNow;
                SoundManager.Instance.PlayLoopingSound("frying", machineTopPoint.position, false);
                break;

            case FryerState.HalfDone:
                var recipe = GetFriesRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSO());
                if (recipe != null)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.intermediate, this);
                }
                break;

            case FryerState.Done:
                var doneRecipe = GetFriesRecipeSoWithInput(GetKitchenObject().GetKitchenObjectSO());
                if (doneRecipe != null)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(doneRecipe.output, this);
                }
                SoundManager.Instance.StopLoopingSound("frying");
                break;
        }
    }

    private void ResetProgress()
    {
        fryProgress = 0f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    private void UpdateProgress(FrenchFryRecipeSo recipe)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryProgress / recipe.fryProgressMax
        });

        OnFryInteractSuccess?.Invoke(this, EventArgs.Empty);
    }

    private FrenchFryRecipeSo GetFriesRecipeSoWithInput(KitchenObjectSo inputSo)
    {
        foreach (var recipe in frenchFriesRecipeSoArray)
        {
            if (recipe.input == inputSo || recipe.intermediate == inputSo)
                return recipe;
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
        GetFriesRecipeSoWithInput(inputSo) != null;

    public override FryerMachineData GetSaveData()
    {
        var data = new FryerMachineData();

        if (HasKitchenObject())
        {
            data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
            data.fryStartTimestamp = fryStartTime?.ToBinary() ?? 0; // store DateTime as long
        }
        else
        {
            data.kitchenObjectId = "";
            data.fryStartTimestamp = 0;
        }

        return data;
    }

    public override void LoadFromSaveData(FryerMachineData data)
    {
        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

                if (data.fryStartTimestamp != 0)
                {
                    fryStartTime = DateTime.FromBinary(data.fryStartTimestamp);
                }
            }
        }
    }
}
