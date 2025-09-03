using UnityEngine;

public class KitchenObject : MonoBehaviour
{
  [SerializeField] private KitchenObjectSo kitchenObjectSO;

  public KitchenObjectSo GetKitchenObjectSO()
  {
    return kitchenObjectSO;
  }

  public static void SpawnKitchenObject(KitchenObjectSo kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
  {
    KitchenObject kitchenObject = Instantiate(kitchenObjectSO.prefab, kitchenObjectParent.GetKitchenObjectFollowTransform()).GetComponent<KitchenObject>();
    kitchenObjectParent.SetKitchenObject(kitchenObject);
  }

  public static void DestroyKitchenObject(IKitchenObjectParent kitchenObjectParent)
  {
    Destroy(kitchenObjectParent.GetKitchenObject().gameObject);
    kitchenObjectParent.ClearKitchenObject();
  }
}
