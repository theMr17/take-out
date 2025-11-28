using System;
using UnityEngine;

public class AssemblyMachine : BaseMachine<AssemblyMachineData>
{
    public event EventHandler OnAssemblyInteractSuccess;

    [SerializeField] private AssemblyRecipeSo[] assemblyRecipeSoArray;

    private bool isLoadNeeded = true;

    protected override string GetSaveKey() => "assemblyMachine";

    private void Update()
    {
        if (isLoadNeeded)
        {
            Load();
            isLoadNeeded = false;
        }
    }

    public override void Interact()
    {
        var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();

        if (!HasKitchenObject())
        {
            TryPlaceObject(selectedObjectSo);
        }
        else
        {
            TryAssembleOrPickup(selectedObjectSo);
        }

        Save();
    }

    public override void InteractAlternate()
    {
        // No alternate interaction defined for assembly machine
    }

    private void TryPlaceObject(KitchenObjectSo selectedObjectSo)
    {
        if (selectedObjectSo == null) return;

        if (!HasRecipeWithInput(selectedObjectSo)) return;

        var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
        if (takenObject == null) return;

        KitchenObject.SpawnKitchenObject(takenObject, this);
    }

    private void TryAssembleOrPickup(KitchenObjectSo selectedObjectSo)
    {
        if (selectedObjectSo == null || selectedObjectSo == GetKitchenObject().GetKitchenObjectSO())
        {
            TryPickupFromMachine();
            return;
        }

        var recipe = GetAssemblyRecipeWithInput(selectedObjectSo);
        if (recipe == null) return;

        InventoryManager.Instance.TakeOneFromSelectedSlot();
        ReplaceWithOutput(recipe.output);

        OnAssemblyInteractSuccess?.Invoke(this, EventArgs.Empty);
    }

    private void TryPickupFromMachine()
    {
        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();

        if (InventoryManager.Instance.TryPickupObject(currentObjectSo))
        {
            KitchenObject.DestroyKitchenObject(this);
        }
    }

    private AssemblyRecipeSo GetAssemblyRecipeWithInput(KitchenObjectSo inputSo)
    {
        if (inputSo == null) return null;

        var machineObjectSo = HasKitchenObject() ? GetKitchenObject().GetKitchenObjectSO() : null;

        foreach (var recipe in assemblyRecipeSoArray)
        {
            if (machineObjectSo == null)
            {
                // Machine empty: check if input is valid as first or second ingredient
                if (recipe.input1 == inputSo || recipe.input2 == inputSo) return recipe;
            }
            else
            {
                // Machine has an object: check if selected + current form a valid pair
                if ((recipe.input1 == inputSo && recipe.input2 == machineObjectSo) ||
                    (recipe.input2 == inputSo && recipe.input1 == machineObjectSo))
                {
                    return recipe;
                }
            }
        }

        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
        GetAssemblyRecipeWithInput(inputSo) != null;

    private void ReplaceWithOutput(KitchenObjectSo outputSo)
    {
        KitchenObject.DestroyKitchenObject(this);
        KitchenObject.SpawnKitchenObject(outputSo, this);
    }

    public override AssemblyMachineData GetSaveData()
    {
        return new AssemblyMachineData
        {
            kitchenObjectId = HasKitchenObject()
                ? GetKitchenObject().GetKitchenObjectSO().name
                : string.Empty
        };
    }

    public override void LoadFromSaveData(AssemblyMachineData data)
    {
        if (string.IsNullOrEmpty(data.kitchenObjectId)) return;

        var kitchenObjectSo = Resources.Load<KitchenObjectSo>(
            $"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}"
        );

        if (kitchenObjectSo != null)
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);
        }
        else
        {
            Debug.LogWarning($"AssemblyMachine: Failed to load KitchenObjectSo {data.kitchenObjectId}");
        }
    }
}
