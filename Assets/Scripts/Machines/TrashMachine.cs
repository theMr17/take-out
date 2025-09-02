public class TrashMachine : BaseMachine
{
  public override void Interact()
  {
    var kitchenObjectSo = InventoryManager.Instance.GetOneFromSelectedSlot();
    if (kitchenObjectSo == null)
    {
      SoundManager.Instance.PlaySound("inventory-interact-error", transform.position);
    }
    else
    {
      SoundManager.Instance.PlaySound("inventory-interact-success", transform.position);
    }

  }
}
