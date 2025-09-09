using UnityEngine;
using UnityEngine.EventSystems;

public class Customer : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] private float lifeDecreaseOnWrongItem = 0.5f;

  public void OnPointerClick(PointerEventData eventData)
  {
    GameManager.Instance.SubmitOrder();
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

  public void MirrorDialogUi(bool mirrored)
  {
    if (DialogUi.Instance == null) return;
    DialogUi.Instance.SetDialogBackground(mirrored);
  }

  public float GetLifeDecreaseOnWrongItem()
  {
    return lifeDecreaseOnWrongItem;
  }
}
