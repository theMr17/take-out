using System;
using UnityEngine;

public class CoffeeMachine : BaseMachine, IHasProgress
{
  public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
  public event EventHandler OnFillInteractSuccess;

  [SerializeField] private CoffeeRecipeSo[] coffeeRecipeSoArray;

  private int fillProgress;

  public override void Interact()
  {
    if (HasKitchenObject())
    {
      HandleCupPickup();
      return;
    }

    var selectedObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
    if (selectedObjectSo == null) return;

    if (!HasRecipeWithInput(selectedObjectSo)) return;

    var takenObject = InventoryManager.Instance.TakeOneFromSelectedSlot();
    KitchenObject.SpawnKitchenObject(takenObject, this);

    SoundManager.Instance.PlaySound("place-cup", machineTopPoint.position);

    ResetProgress();
  }

  public override void InteractAlternate()
  {
    if (!HasKitchenObject())
    {
      return;
    }

    var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
    var recipe = GetCoffeeRecipeSoWithInput(currentObjectSo);

    if (recipe == null)
    {
      return;
    }

    fillProgress++;
    UpdateProgress(recipe);

    SoundManager.Instance?.PlaySound("fill-coffee-cup", machineTopPoint.position);

    if (fillProgress >= recipe.fillProgressMax)
    {
      ReplaceWithOutput(recipe.output);
    }

    OnFillInteractSuccess?.Invoke(this, EventArgs.Empty);
  }

  private void HandleCupPickup()
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
    fillProgress = 0;
    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
    {
      progressNormalized = 0f
    });
  }

  private void UpdateProgress(CoffeeRecipeSo recipe)
  {
    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
    {
      progressNormalized = (float)fillProgress / recipe.fillProgressMax
    });
  }

  private CoffeeRecipeSo GetCoffeeRecipeSoWithInput(KitchenObjectSo inputSo)
  {
    foreach (var recipe in coffeeRecipeSoArray)
    {
      if (recipe.input == inputSo) return recipe;
    }
    return null;
  }

  private bool HasRecipeWithInput(KitchenObjectSo inputSo) =>
    GetCoffeeRecipeSoWithInput(inputSo) != null;
}
