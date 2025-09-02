using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUi : MonoBehaviour
{
  [SerializeField] private Image iconImage;
  [SerializeField] private TextMeshProUGUI quantityText;
  [SerializeField] private GameObject selectedIndicator;

  public void SetItem(KitchenObjectSo kitchenObjectSo, int quantity)
  {
    iconImage.sprite = kitchenObjectSo.icon;
    iconImage.gameObject.SetActive(true);

    quantityText.text = quantity.ToString();
    quantityText.gameObject.SetActive(true);
  }

  public void ClearSlot()
  {
    iconImage.sprite = null;
    iconImage.gameObject.SetActive(false);

    quantityText.text = "0";
    quantityText.gameObject.SetActive(false);
  }

  public void SetSelected(bool isSelected)
  {
    GetComponent<Image>().color = isSelected ? Color.lightGray : Color.white;
    selectedIndicator.SetActive(isSelected);
  }
}
