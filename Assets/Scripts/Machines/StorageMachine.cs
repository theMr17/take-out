using UnityEngine;

public class StorageMachine : BaseMachine<EmptyData>
{
  [SerializeField] private KitchenObjectSo kitchenObjectSO;

  public override void Interact()
  {
    InventoryManager.Instance.TryPickupObject(kitchenObjectSO);
  }
}
