using System;
using UnityEngine;

public class StorageMachine : BaseMachine
{
  [SerializeField] private KitchenObjectSO kitchenObjectSO;

  public override void Interact()
  {
    InventoryManager.Instance.TryPickupObject(kitchenObjectSO);
  }
}
