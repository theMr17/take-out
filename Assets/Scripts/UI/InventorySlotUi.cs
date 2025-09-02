using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUi : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private Image iconImage;
  [SerializeField] private TextMeshProUGUI quantityText;
  [SerializeField] private GameObject selectedIndicator;

  private Image _backgroundImage;

  private void Awake()
  {
    _backgroundImage = GetComponent<Image>();

    // Ensure UI is cleared on start
    ClearSlot();
    SetSelected(false);
  }

  public void SetItem(KitchenObjectSo kitchenObjectSo, int quantity)
  {
    if (kitchenObjectSo == null)
    {
      ClearSlot();
      return;
    }

    iconImage.sprite = kitchenObjectSo.icon;
    iconImage.gameObject.SetActive(true);

    quantityText.text = quantity.ToString();
    quantityText.gameObject.SetActive(true);
  }

  public void ClearSlot()
  {
    iconImage.sprite = null;
    iconImage.gameObject.SetActive(false);

    quantityText.text = string.Empty;
    quantityText.gameObject.SetActive(false);
  }

  public void SetSelected(bool isSelected)
  {
    if (_backgroundImage != null)
    {
      _backgroundImage.color = isSelected ? Color.lightGray : Color.white;
    }

    if (selectedIndicator != null)
    {
      selectedIndicator.SetActive(isSelected);
    }
  }
}
