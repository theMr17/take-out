using System;
using UnityEngine;

public class StorageMachine : BaseMachine
{
  [SerializeField] private KitchenObjectSo kitchenObjectSO;

  public override void Interact()
  {
    InventoryManager.Instance.TryPickupObject(kitchenObjectSO);
  }
}
