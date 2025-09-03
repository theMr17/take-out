using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseMachine<MachineSaveData> : SaveableBehaviour<MachineSaveData>,
    IKitchenObjectParent, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    where MachineSaveData : class, new()
{
  public static event EventHandler OnAnyObjectPlacedHere;

  [SerializeField] protected bool hasSpriteMask = false;
  [SerializeField] protected Transform machineTopPoint;

  private KitchenObject kitchenObject;

  public static void ResetStaticData()
  {
    OnAnyObjectPlacedHere = null;
  }

  public virtual void Interact()
  {
    Debug.LogError("BaseMachine.Interact() not overridden.");
  }

  public virtual void InteractAlternate()
  {
    Debug.LogError("BaseMachine.InteractAlternate() not overridden.");
  }

  public void ClearKitchenObject() => kitchenObject = null;

  public KitchenObject GetKitchenObject() => kitchenObject;

  public Transform GetKitchenObjectFollowTransform() => machineTopPoint;

  public bool HasKitchenObject() => kitchenObject != null;

  public void SetKitchenObject(KitchenObject kitchenObject)
  {
    this.kitchenObject = kitchenObject;

    if (kitchenObject != null)
    {
      OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
    }
  }

  protected override string GetSaveKey()
  {
    throw new NotImplementedException();
  }

  public override MachineSaveData GetSaveData()
  {
    throw new NotImplementedException();
  }

  public override void LoadFromSaveData(MachineSaveData data)
  {
    throw new NotImplementedException();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    switch (eventData.button)
    {
      case PointerEventData.InputButton.Left:
        Interact();
        break;
      case PointerEventData.InputButton.Right:
        InteractAlternate();
        break;
    }
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (TryGetComponent(out SpriteRenderer spriteRenderer))
    {
      spriteRenderer.color = hasSpriteMask
        ? new Color(1, 1, 1, 0.05f)
        : Color.lightGray;
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (TryGetComponent(out SpriteRenderer spriteRenderer))
    {
      spriteRenderer.color = hasSpriteMask
        ? new Color(1, 1, 1, 0f)
        : Color.white;
    }
  }
}
