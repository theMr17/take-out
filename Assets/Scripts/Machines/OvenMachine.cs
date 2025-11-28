using System;
using UnityEngine;

public enum OvenState
{
    Idle, // nothing inside
    Baking, // currently baking
    Done // fully baked
}

public class OvenMachine : BaseMachine<OvenMachineData>, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    [SerializeField] private OvenRecipeSo[] ovenRecipeSoArray;

    [SerializeField] private GameObject openDoorVisual;
    [SerializeField] private GameObject closedDoorVisual;

    private OvenState currentState = OvenState.Idle;
    private DateTime? bakeStartTime;
    private float ovenProgress;
    private bool isLoadNeeded = true;

    protected override string GetSaveKey() => "ovenMachine";

    private void Update()
    {
        if (isLoadNeeded)
        {
            Load();
            isLoadNeeded = false;
        }

        if (!HasKitchenObject())
        {
            SetState(OvenState.Idle);
            return;
        }

        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
        var recipe = GetOvenRecipeSoWithState(currentObjectSo);
        if (recipe == null) return;

        // Ensure start time is set
        if (bakeStartTime == null)
            bakeStartTime = DateTime.UtcNow;

        double elapsed = (DateTime.UtcNow - bakeStartTime.Value).TotalSeconds;
        ovenProgress = Mathf.Min((float)elapsed, recipe.bakeProgressMax);

        UpdateProgress(recipe);

        if (ovenProgress >= recipe.bakeProgressMax && currentObjectSo == recipe.input)
        {
            ReplaceWithOutput(recipe.output);
            SetState(OvenState.Done);
        }
        else
        {
            SetState(OvenState.Baking);
        }

        Save();
    }

    public override void Interact()
    {
        if (HasKitchenObject())
        {
            HandleOvenPickup();
            Save();
            return;
        }

        var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
        var recipe = GetOvenRecipeSoWithState(selectedObjectSo);
        if (recipe == null || selectedObjectSo != recipe.input) return;

        var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
        KitchenObject.SpawnKitchenObject(takenObject, this);
        bakeStartTime = DateTime.UtcNow;
        ResetProgress();
        SetState(OvenState.Baking);
        Save();
    }

    public override void InteractAlternate()
    {
        // Currently not used, can be extended for special oven features
    }

    private void HandleOvenPickup()
    {
        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
        if (!InventoryManager.Instance.TryPickupObject(currentObjectSo)) return;

        KitchenObject.DestroyKitchenObject(this);
        ResetProgress();
        SetState(OvenState.Idle);
    }

    private void ReplaceWithOutput(KitchenObjectSo outputSo)
    {
        KitchenObject.DestroyKitchenObject(this);
        KitchenObject.SpawnKitchenObject(outputSo, this);
    }

    private void ResetProgress()
    {
        ovenProgress = 0f;
        bakeStartTime = null;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    private void UpdateProgress(OvenRecipeSo recipe)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = Mathf.Clamp01(ovenProgress / recipe.bakeProgressMax)
        });
    }

    private void SetState(OvenState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case OvenState.Idle:
                bakeStartTime = null;
                ovenProgress = 0f;
                openDoorVisual.SetActive(true);
                closedDoorVisual.SetActive(false);
                SoundManager.Instance.StopLoopingSound("oven-fan");
                break;

            case OvenState.Baking:
                bakeStartTime ??= DateTime.UtcNow;

                SoundManager.Instance.PlaySound("oven-door-close", transform.position);
                openDoorVisual.SetActive(false);
                closedDoorVisual.SetActive(true);

                SoundManager.Instance.PlayLoopingSound("oven-fan", transform.position, false);
                break;

            case OvenState.Done:
                SoundManager.Instance.PlaySound("oven-door-open", transform.position);
                openDoorVisual.SetActive(true);
                closedDoorVisual.SetActive(false);

                SoundManager.Instance.StopLoopingSound("oven-fan");
                break;
        }
    }

    private OvenRecipeSo GetOvenRecipeSoWithState(KitchenObjectSo stateSo)
    {
        foreach (var recipe in ovenRecipeSoArray)
        {
            if (recipe.input == stateSo)
                return recipe;
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
        GetOvenRecipeSoWithState(inputSo) != null;

    public override OvenMachineData GetSaveData()
    {
        var data = new OvenMachineData();
        if (HasKitchenObject())
        {
            data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
            data.bakeStartTimestamp = bakeStartTime?.ToBinary() ?? 0;
        }
        else
        {
            data.kitchenObjectId = "";
            data.bakeStartTimestamp = 0;
        }
        return data;
    }

    public override void LoadFromSaveData(OvenMachineData data)
    {
        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>("ScriptableObjects/KitchenObjects/" + data.kitchenObjectId);
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

                if (data.bakeStartTimestamp != 0)
                {
                    bakeStartTime = DateTime.FromBinary(data.bakeStartTimestamp);
                }

                var recipe = GetOvenRecipeSoWithState(kitchenObjectSo);
                if (recipe != null && bakeStartTime != null)
                {
                    double elapsed = (DateTime.UtcNow - bakeStartTime.Value).TotalSeconds;
                    ovenProgress = Mathf.Min((float)elapsed, recipe.bakeProgressMax);
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = Mathf.Clamp01(ovenProgress / recipe.bakeProgressMax)
                    });
                }
            }
        }
    }
}
