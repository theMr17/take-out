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
        if (!HasKitchenObject())
        {
            // Get currently selected object from inventory
            var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
            if (selectedObjectSo == null) return;

            // Ensure this object can be used in a recipe
            if (!HasRecipeWithInput(selectedObjectSo)) return;

            // Place the object in the machine and remove it from inventory
            var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
            KitchenObject.SpawnKitchenObject(takenObject, this);
        }
        else
        {
            // Machine already has an object, try to combine with selected object
            var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
            if (selectedObjectSo == null || selectedObjectSo == GetKitchenObject().GetKitchenObjectSO())
            {
                HandlePickup();
                Save();
                return;
            }

            var recipe = GetAssemblyRecipeSoWithInput(selectedObjectSo);
            if (recipe == null) return;

            // Remove the selected object from inventory
            InventoryManager.Instance.TakeOneFromSelectedSlot();

            // Replace the current object in the machine with the output
            ReplaceWithOutput(recipe.output);

            OnAssemblyInteractSuccess?.Invoke(this, EventArgs.Empty);
        }

        Save(); // Save after placing object
    }

    public override void InteractAlternate()
    {
        return; // No alternate interaction needed for fryer
    }

    private void HandlePickup()
    {
        var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();

        if (!InventoryManager.Instance.TryPickupObject(currentObjectSo)) return;

        KitchenObject.DestroyKitchenObject(this);
    }

    private AssemblyRecipeSo GetAssemblyRecipeSoWithInput(KitchenObjectSo inputSo)
    {
        foreach (var recipe in assemblyRecipeSoArray)
        {

            if (!HasKitchenObject())
            {
                if (recipe.input1 == inputSo || recipe.input2 == inputSo)
                    return recipe;
            }
            else
            {
                var machineObjectSo = GetKitchenObject().GetKitchenObjectSO();
                if ((recipe.input1 == inputSo && recipe.input2 == machineObjectSo)
                 || (recipe.input2 == inputSo && recipe.input1 == machineObjectSo))
                    return recipe;
            }
        }
        return null;
    }

    private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
        GetAssemblyRecipeSoWithInput(inputSo) != null;

    private void ReplaceWithOutput(KitchenObjectSo outputSo)
    {
        KitchenObject.DestroyKitchenObject(this);
        KitchenObject.SpawnKitchenObject(outputSo, this);
    }

    public override AssemblyMachineData GetSaveData()
    {
        var data = new AssemblyMachineData();

        if (HasKitchenObject())
        {
            data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
        }
        else
        {
            data.kitchenObjectId = "";
        }

        return data;
    }

    public override void LoadFromSaveData(AssemblyMachineData data)
    {
        if (!string.IsNullOrEmpty(data.kitchenObjectId))
        {
            var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
            if (kitchenObjectSo != null)
            {
                KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);
            }
        }
    }
}
