using System;
using UnityEngine;

public class FryerMachine : BaseMachine<FryerMachineData>, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnFryInteractSuccess;

    [SerializeField] private FrenchFryRecipeSo[] frenchFriesRecipeSoArray;

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

        if (HasKitchenObject())
        {
            var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
            var recipe = GetFriesRecipeSoWithInput(currentObjectSo);
            if (recipe != null)
            {
                if (fryStartTime == null)
                {
                    fryStartTime = DateTime.UtcNow; // fallback if missing
                }

                // Calculate elapsed time since frying started
                double elapsed = (DateTime.UtcNow - fryStartTime.Value).TotalSeconds;
                fryProgress = Mathf.Min((float)elapsed, recipe.fryProgressMax);

                UpdateProgress(recipe);

                // Check for intermediate stage
                if (fryProgress >= recipe.interMediateFryTime && currentObjectSo != recipe.intermediate)
                {
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
        else
        {
            fryProgress = 0f;
            fryStartTime = null;
            SoundManager.Instance.StopLoopingSound("frying");
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

        SoundManager.Instance.PlayLoopingSound("frying", machineTopPoint.position, false);

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
        ResetProgress();

        SoundManager.Instance.StopLoopingSound("frying");
    }

    private void ReplaceWithOutput(KitchenObjectSo outputSo)
    {
        KitchenObject.DestroyKitchenObject(this);
        KitchenObject.SpawnKitchenObject(outputSo, this);

        SoundManager.Instance.StopLoopingSound("frying");
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
