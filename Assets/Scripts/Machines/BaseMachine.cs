using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseMachine : MonoBehaviour, IKitchenObjectParent, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
  public static event EventHandler OnAnyObjectPlacedHere;

  public static void ResetStaticData()
  {
    OnAnyObjectPlacedHere = null;
  }

  [SerializeField] protected bool hasSpriteMask = false;

  [SerializeField] protected Transform machineTopPoint;

  private KitchenObject kitchenObject;

  public virtual void Interact()
  {
    Debug.LogError("BaseCounter.Interact()");
  }

  public void ClearKitchenObject()
  {
    kitchenObject = null;
  }

  public KitchenObject GetKitchenObject()
  {
    return kitchenObject;
  }

  public Transform GetKitchenObjectFollowTransform()
  {
    return machineTopPoint;
  }

  public bool HasKitchenObject()
  {
    return kitchenObject != null;
  }

  public void SetKitchenObject(KitchenObject kitchenObject)
  {
    this.kitchenObject = kitchenObject;

    if (kitchenObject != null)
    {
      OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
    }
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Interact();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (TryGetComponent(out SpriteRenderer spriteRenderer))
    {
      if (hasSpriteMask)
      {
        spriteRenderer.color = new Color(1, 1, 1, 0.05f);
      }
      else
      {
        spriteRenderer.color = Color.lightGray;
      }
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (TryGetComponent(out SpriteRenderer spriteRenderer))
    {
      if (hasSpriteMask)
      {
        spriteRenderer.color = new Color(1, 1, 1, 0);
      }
      else
      {
        spriteRenderer.color = Color.white;
      }
    }
  }
}
