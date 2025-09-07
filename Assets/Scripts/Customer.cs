using UnityEngine;
using UnityEngine.EventSystems;

public class Customer : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
  public void OnPointerClick(PointerEventData eventData)
  {
    var selectedKitchenObjectSo = InventoryManager.Instance.GetKitchenObjectSoFromSelectedSlot();
    if (GameManager.Instance.CanSubmitOrder(selectedKitchenObjectSo))
    {
      GameManager.Instance.SubmitOrder();
    }
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (TryGetComponent(out SpriteRenderer spriteRenderer))
    {
      spriteRenderer.color = Color.gray;
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (TryGetComponent(out SpriteRenderer spriteRenderer))
    {
      spriteRenderer.color = Color.white;
    }
  }
}
