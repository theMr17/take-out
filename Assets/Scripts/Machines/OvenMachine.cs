using System;
using UnityEngine;

public class OvenMachine : BaseMachine<OvenMachineData>, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    [SerializeField] private OvenRecipeSo[] ovenRecipeSoArray;
    private float ovenProgress;

    protected override string GetSaveKey() => "ovenMachine";

    private void Awake()
    {
        Load();
    }

    private void Update()
    {
        if (!HasKitchenObject())
        {
            ovenProgress = 0f;
            return;
        }

        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
        var recipe = GetOvenRecipeSoWithState(currentObjectSo);
        if (recipe == null) return;

        ovenProgress += Time.deltaTime;
        UpdateProgress(recipe);

        if (currentObjectSo == recipe.input && ovenProgress >= recipe.bakeProgressMax)
        {
            ReplaceWithOutput(recipe.output);
            ovenProgress = 0f;
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
        ResetProgress();
        Save();
    }

    public override void InteractAlternate()
    {
        // No longer needed for oven progress
        // Could be used for other alternate interactions if needed
    }

    private void HandleOvenPickup()
    {
        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
        if (!InventoryManager.Instance.TryPickupObject(currentObjectSo)) return;
        KitchenObject.DestroyKitchenObject(this);
        ResetProgress();
    }

    private void ReplaceWithOutput(KitchenObjectSo outputSo)
    {
        KitchenObject.DestroyKitchenObject(this);
        KitchenObject.SpawnKitchenObject(outputSo, this);
    }

    private void ResetProgress()
    {
        ovenProgress = 0f;
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

    private OvenRecipeSo GetOvenRecipeSoWithState(KitchenObjectSo stateSo)
    {
        foreach (var recipe in ovenRecipeSoArray)
        {
            if (recipe.input == stateSo || recipe.output == stateSo)
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
            data.ovenProgress = Mathf.RoundToInt(ovenProgress);
        }
        else
        {
            data.kitchenObjectId = "";
            data.ovenProgress = 0;
        }
        return data;
    }

    public override void LoadFromSaveData(OvenMachineData data)
    {
        ovenProgress = data.ovenProgress;
        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>("ScriptableObjects/KitchenObjects/" + data.kitchenObjectId);
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);
                var recipe = GetOvenRecipeSoWithState(kitchenObjectSo);
                if (recipe != null)
                {
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = Mathf.Clamp01(ovenProgress / recipe.bakeProgressMax)
                    });
                }
            }
        }
    }
}
