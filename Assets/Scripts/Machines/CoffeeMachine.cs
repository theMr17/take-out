using UnityEngine;

public class CoffeeMachine : BaseMachine
{
  [SerializeField] private KitchenObjectSo cupKitchenObjectSo;

  public override void Interact()
  {
    if (!HasKitchenObject())
    {
      KitchenObject.SpawnKitchenObject(cupKitchenObjectSo, this);
      SoundManager.Instance.PlaySound("place-cup", machineTopPoint.position);
    }
    else
    {
      Debug.Log("CoffeeMachine.Interact() - The slot is full.");
    }
  }
}
