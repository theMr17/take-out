using System;
using UnityEngine;

public class GrillMachine : BaseMachine<GrillMachineData>, IHasProgress
{
    private bool isBurnt = false;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnGrillInteractSuccess;
    [SerializeField] private GrillRecipeSo[] grillRecipeSoArray;
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
            if (isBurnt) return;

            var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
            var recipe = GetGrillRecipeSoWithState(currentObjectSo);
            if (recipe != null)
            {
                grillProgress += Time.deltaTime;
                UpdateProgress(recipe);
                SoundManager.Instance?.PlaySound("grill-sizzle", machineTopPoint.position);

                // Transition: input -> intermediate
                if (currentObjectSo == recipe.input && grillProgress >= recipe.interMediateGrillTime)
                {
                    KitchenObject.DestroyKitchenObject(this);
                    KitchenObject.SpawnKitchenObject(recipe.intermediate, this);
                }
                // Transition: intermediate -> output
                else if (currentObjectSo == recipe.intermediate && grillProgress >= recipe.grillProgressMax)
                {
                    ReplaceWithOutput(recipe.output);
                }
                // Transition: output -> burntOutput
                else if (currentObjectSo == recipe.output && grillProgress >= recipe.burntGrillTime)
                {
                    ReplaceWithOutput(recipe.burntOutput);
                    isBurnt = true;
                }
                Save();
            }
        }
        else
        {
            grillProgress = 0f;
            isBurnt = false;
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
    //     // No longer needed for grilling progress
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
            progressNormalized = grillProgress / recipe.grillProgressMax
        });
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

    private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
        GetGrillRecipeSoWithState(inputSo) != null;

    public override GrillMachineData GetSaveData()
    {
        var data = new GrillMachineData();

        if (HasKitchenObject())
        {
            data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
            data.grillProgress = Mathf.RoundToInt(grillProgress);
            data.isBurnt = isBurnt;
        }
        else
        {
            data.kitchenObjectId = "";
            data.grillProgress = 0;
            data.isBurnt = false;
        }

        return data;
    }

    public override void LoadFromSaveData(GrillMachineData data)
    {
        grillProgress = data.grillProgress;
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
                        progressNormalized = grillProgress / recipe.grillProgressMax
                    });
                }
            }
        }
    }
}
