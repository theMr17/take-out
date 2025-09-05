using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderItemUi : MonoBehaviour
{
  [SerializeField] private Image iconImage;
  [SerializeField] private TextMeshProUGUI orderNameText;

  [SerializeField] private TextMeshProUGUI orderQuantityText;


  public void SetKitchenObject(KitchenObjectSo kitchenObjectSo, int quantity = 1)
  {
    iconImage.sprite = kitchenObjectSo.icon;
    orderNameText.text = kitchenObjectSo.name;
    orderQuantityText.text = $"x{quantity}";
  }
}
