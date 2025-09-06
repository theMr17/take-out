using System;
using UnityEngine;

public class GrillMachine : BaseMachine<FryerMachineData>, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnFryInteractSuccess;

    [SerializeField] private GrillRecipeSo[] pattyRecipeSoArray;

    private float grillProgress;

    private bool isLoadNeeded = true;

    protected override string GetSaveKey() => "grillMachine";

    private void Update()
    {
        if (isLoadNeeded)
        {
            Load();
            isLoadNeeded = false;
        }

        if (HasKitchenObject())
        {
            var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
            var recipe = GetPattyRecipeSoWithInput(currentObjectSo);
            if (recipe != null && grillProgress < recipe.grillProgressMax)
            {
                grillProgress += Time.deltaTime;
                UpdateProgress(recipe);
                SoundManager.Instance?.PlaySound("grill-sizzle", machineTopPoint.position);

                // Check for intermediate stage
                if (grillProgress >= recipe.interMediateGrillTime && currentObjectSo != recipe.intermediate)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.intermediate, this);
                }

                if (grillProgress >= recipe.grillProgressMax)
                {
                    ReplaceWithOutput(recipe.output);
                }
                Save();
            }
        }
        else
        {
            grillProgress = 0f;
        }
    }

    public override void Interact()
    {
        if (HasKitchenObject())
        {
            HandleFriesPickup();
            Save(); // Save after change
            return;
        }

        // Get currently selected object from inventory
        var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
        if (selectedObjectSo == null) return;

        // Ensure this object can be used in a recipe
        if (!HasRecipeWithInput(selectedObjectSo)) return;

        // Place the cup in the machine and remove it from inventory
        var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
        KitchenObject.SpawnKitchenObject(takenObject, this);

        SoundManager.Instance.PlaySound("place-cup", machineTopPoint.position);

        ResetProgress();
        Save();
    }

    // public override void InteractAlternate()
    // {
    //     // No longer needed for frying progress
    //     // Could be used for other alternate interactions if needed
    // }

    private void HandleFriesPickup()
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
        grillProgress = 0f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    private void UpdateProgress(GrillRecipeSo recipe)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = grillProgress / recipe.grillProgressMax
        });
    }

    private GrillRecipeSo GetPattyRecipeSoWithInput(KitchenObjectSo inputSo)
    {
        foreach (var recipe in pattyRecipeSoArray)
        {
            if (recipe.input == inputSo || recipe.intermediate == inputSo)
                return recipe;
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
      GetPattyRecipeSoWithInput(inputSo) != null;

    public override FryerMachineData GetSaveData()
    {
        var data = new FryerMachineData();

        if (HasKitchenObject())
        {
            data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
            data.fryingProgress = Mathf.RoundToInt(grillProgress); // If you want to save as int
        }
        else
        {
            data.kitchenObjectId = "";
            data.fryingProgress = 0;
        }

        return data;
    }

    public override void LoadFromSaveData(FryerMachineData data)
    {
        grillProgress = data.fryingProgress;

        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

                var recipe = GetPattyRecipeSoWithInput(kitchenObjectSo);
                if (recipe != null)
                {
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = grillProgress / recipe.grillProgressMax
                    });
                }
            }
        }
    }
}
