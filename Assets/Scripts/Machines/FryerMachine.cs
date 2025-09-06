using System;
using UnityEngine;

public class FryerMachine : BaseMachine<FryerMachineData>, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnFryInteractSuccess;

    [SerializeField] private FrenchFryRecipeSo[] frenchFriesRecipeSoArray;

    private int fryProgress;
    private float fryTimer;

    private bool isLoadNeeded = true;

    protected override string GetSaveKey() => "fryerMachine";

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
            var recipe = GetFriesRecipeSoWithInput(currentObjectSo);
            if (recipe != null && fryProgress < recipe.fryProgressMax)
            {
                fryTimer += Time.deltaTime;
                if (fryTimer >= 1f) // 1 second per progress step, adjust as needed
                {
                    fryProgress++;
                    fryTimer = 0f;
                    UpdateProgress(recipe);
                    SoundManager.Instance?.PlaySound("fill-coffee-cup", machineTopPoint.position);

                    // Check for intermediate stage
                    if (fryProgress == Mathf.RoundToInt(recipe.interMediateFryTime))
                    {
                        // Replace with intermediate kitchen object
                        KitchenObject.DestroyKitchenObject(this);
                        KitchenObject.SpawnKitchenObject(recipe.intermediate, this);
                    }

                    if (fryProgress >= recipe.fryProgressMax)
                    {
                        ReplaceWithOutput(recipe.output);
                    }
                    Save();
                }
            }
        }
        else
        {
            fryTimer = 0f;
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
        fryProgress = 0;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    private void UpdateProgress(FrenchFryRecipeSo recipe)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)fryProgress / recipe.fryProgressMax
        });

        // Notify listeners that a successful fill interaction happened
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
            data.fryingProgress = fryProgress;
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
        fryProgress = data.fryingProgress;

        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

                var recipe = GetFriesRecipeSoWithInput(kitchenObjectSo);
                if (recipe != null)
                {
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = (float)fryProgress / recipe.fryProgressMax
                    });
                }
            }
        }
    }
}
