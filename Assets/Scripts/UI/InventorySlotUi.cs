using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUi : MonoBehaviour
{
  [SerializeField] private Image iconImage;
  [SerializeField] private TextMeshProUGUI quantityText;

  public void SetItem(KitchenObjectSO kitchenObjectSO, int quantity)
  {
    iconImage.sprite = kitchenObjectSO.icon;
    iconImage.color = Color.white;
    quantityText.text = quantity.ToString();
    quantityText.gameObject.SetActive(true);
  }

  public void ClearSlot()
  {
    iconImage.sprite = null;
    iconImage.color = new Color(1, 1, 1, 0);
    quantityText.text = "0";
    quantityText.gameObject.SetActive(false);
  }
}
