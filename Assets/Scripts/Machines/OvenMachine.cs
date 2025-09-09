using System;
using UnityEngine;

public class OvenMachine : BaseMachine<OvenMachineData>, IHasProgress
{
    private bool isBurnt = false;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnOvenInteractSuccess;
    [SerializeField] private OvenRecipeSo[] ovenRecipeSoArray;
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

        if (HasKitchenObject())
        {
            if (isBurnt) return;

            var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
            var recipe = GetOvenRecipeSoWithState(currentObjectSo);
            if (recipe != null)
            {
                ovenProgress += Time.deltaTime;
                UpdateProgress(recipe);

                SoundManager.Instance?.PlayLoopingSound("oven-bake", machineTopPoint.position);
                // Transition: input -> intermediate
                if (currentObjectSo == recipe.input && ovenProgress >= recipe.interMediateBakeTime)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.intermediate, this);
                }
                // Transition: intermediate -> output
                else if (currentObjectSo == recipe.intermediate && ovenProgress >= recipe.bakeProgressMax)
                {
                    ReplaceWithOutput(recipe.output);
                }
                // Transition: output -> burntOutput
                else if (currentObjectSo == recipe.output && ovenProgress >= recipe.burntBakeTime)
                {
                    ReplaceWithOutput(recipe.burntOutput);
                    isBurnt = true;
                }
                Save();
            }
        }
        else
        {
            ovenProgress = 0f;
            isBurnt = false;
        }
    }

    public override void Interact()
    {
        if (HasKitchenObject())
        {
            HandleOvenPickup();
            Save(); // Save after change
            return;
        }

        // Get currently selected object from inventory
        var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
        if (selectedObjectSo == null) return;

        // Ensure this object can be used in a recipe
        var recipe = GetOvenRecipeSoWithState(selectedObjectSo);
        if (recipe == null || selectedObjectSo != recipe.input) return;

        // Place the item in the oven and remove it from inventory
        var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
        KitchenObject.SpawnKitchenObject(takenObject, this);

        SoundManager.Instance.PlaySound("place-pan", machineTopPoint.position);

        ResetProgress();
        Save();
    }

    // public override void InteractAlternate()
    // {
    //     // No longer needed for oven progress
    //     // Could be used for other alternate interactions if needed
    // }

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
        isBurnt = false;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    private void UpdateProgress(OvenRecipeSo recipe)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = ovenProgress / recipe.bakeProgressMax
        });
    }

    private OvenRecipeSo GetOvenRecipeSoWithState(KitchenObjectSo stateSo)
    {
        foreach (var recipe in ovenRecipeSoArray)
        {
            if (recipe.input == stateSo || recipe.intermediate == stateSo || recipe.output == stateSo || recipe.burntOutput == stateSo)
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
            data.isBurnt = isBurnt;
        }
        else
        {
            data.kitchenObjectId = "";
            data.ovenProgress = 0;
            data.isBurnt = false;
        }

        return data;
    }

    public override void LoadFromSaveData(OvenMachineData data)
    {
        ovenProgress = data.ovenProgress;
        isBurnt = data.isBurnt;

        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

                var recipe = GetOvenRecipeSoWithState(kitchenObjectSo);
                if (recipe != null)
                {
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = ovenProgress / recipe.bakeProgressMax
                    });
                }
            }
        }
    }
}
