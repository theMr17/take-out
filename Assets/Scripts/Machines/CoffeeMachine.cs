using System;
using UnityEngine;

public class CoffeeMachine : BaseMachine, IHasProgress, ISaveable<CoffeeMachineData>
{
  public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
  public event EventHandler OnFillInteractSuccess;

  [SerializeField] private CoffeeRecipeSo[] coffeeRecipeSoArray;
  [SerializeField] private ParticleSystem coffeeFillEffect;

  private int fillProgress;

  private bool isLoadNeeded = true;

  private SaveableHelper<CoffeeMachineData> saveHelper;

  private void Awake()
  {
    saveHelper = new SaveableHelper<CoffeeMachineData>(GetSaveData, LoadFromSaveData);
  }

  private void Update()
  {
    if (isLoadNeeded)
    {
      LoadCoffeeMachine();
      isLoadNeeded = false;
    }
  }

  public override void Interact()
  {
    if (HasKitchenObject())
    {
      HandleCupPickup(); // Player takes cup back from machine
      SaveCoffeeMachine(); // Save after change
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

    ResetProgress(); // Start with 0 progress
    SaveCoffeeMachine(); // Save after placing cup
  }

  public override void InteractAlternate()
  {
    if (!HasKitchenObject())
    {
      return;
    }

    var currentObjectSo = GetKitchenObject().GetKitchenObjectSO();
    var recipe = GetCoffeeRecipeSoWithInput(currentObjectSo);

    if (recipe == null) return; // Not a valid recipe for this machine

    // Increase fill progress each interaction
    fillProgress++;
    UpdateProgress(recipe);

    // Play filling sound effect
    SoundManager.Instance?.PlaySound("fill-coffee-cup", machineTopPoint.position);

    // Check if the cup is fully filled
    if (fillProgress >= recipe.fillProgressMax)
    {
      ReplaceWithOutput(recipe.output);
    }

    SaveCoffeeMachine(); // Save after filling
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

    // Notify listeners that a successful fill interaction happened
    OnFillInteractSuccess?.Invoke(this, EventArgs.Empty);

    coffeeFillEffect.Play();
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

  public CoffeeMachineData GetSaveData()
  {
    var data = new CoffeeMachineData();

    if (HasKitchenObject())
    {
      data.kitchenObjectId = GetKitchenObject().GetKitchenObjectSO().name;
      data.fillProgress = fillProgress;
    }
    else
    {
      data.kitchenObjectId = "";
      data.fillProgress = 0;
    }

    return data;
  }

  public void LoadFromSaveData(CoffeeMachineData data)
  {
    fillProgress = data.fillProgress;

    if (!string.IsNullOrEmpty(data.kitchenObjectId))
    {
      var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{data.kitchenObjectId}");
      if (kitchenObjectSo != null)
      {
        KitchenObject.SpawnKitchenObject(kitchenObjectSo, this);

        var recipe = GetCoffeeRecipeSoWithInput(kitchenObjectSo);
        if (recipe != null)
        {
          OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
          {
            progressNormalized = (float)fillProgress / recipe.fillProgressMax
          });
        }
      }
    }
  }

  public void SaveCoffeeMachine() => saveHelper.Save("coffeeMachine");
  public void LoadCoffeeMachine() => saveHelper.Load("coffeeMachine");
}
