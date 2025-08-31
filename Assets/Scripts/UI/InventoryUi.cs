using UnityEngine;
using UnityEngine.UI;

public class InventoryUi : MonoBehaviour
{
  [SerializeField] private Transform inventorySlotsContainer;
  [SerializeField] private GameObject inventorySlotPrefab;

  private void Start()
  {
    CreateInventorySlots();

    InventoryManager.Instance.OnSlotSelectionChanged += InventoryManager_OnSlotSelectionChanged;
    InventoryManager.Instance.OnInventorySlotUpdated += InventoryManager_OnInventorySlotUpdated;
  }

  private void CreateInventorySlots()
  {
    foreach (Transform child in inventorySlotsContainer)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < InventoryManager.Instance.GetInventorySize(); i++)
    {
      InventorySlotUi slotItem = Instantiate(inventorySlotPrefab, inventorySlotsContainer).GetComponent<InventorySlotUi>();
      slotItem.SetSelected(false);
      slotItem.ClearSlot();
    }
  }

  private void InventoryManager_OnSlotSelectionChanged(object sender, InventoryManager.SelectedSlotEventArgs e)
  {
    UpdateSelectionIndicator(e.selectedSlot);
  }

  private void InventoryManager_OnInventorySlotUpdated(object sender, InventoryManager.InventorySlotEventArgs e)
  {
    UpdateInventorySlot(e.slotIndex, e.kitchenObjectSo, e.quantity);
  }

  private void UpdateSelectionIndicator(int selectedSlot)
  {
    GameObject selectedSlotObject = inventorySlotsContainer.GetChild(selectedSlot).gameObject;

    // Highlight the selected slot
    selectedSlotObject.GetComponent<Image>().color = Color.lightGray;
    selectedSlotObject.GetComponent<InventorySlotUi>().SetSelected(true);

    // Reset colors for all other slots
    for (int i = 0; i < inventorySlotsContainer.childCount; i++)
    {
      if (i == selectedSlot) continue;

      inventorySlotsContainer.GetChild(i).GetComponent<Image>().color = Color.white;
      inventorySlotsContainer.GetChild(i).GetComponent<InventorySlotUi>().SetSelected(false);
    }
  }

  private void UpdateInventorySlot(int slotIndex, KitchenObjectSO kitchenObjectSo, int quantity)
  {
    InventorySlotUi slotItem = inventorySlotsContainer.GetChild(slotIndex).GetComponent<InventorySlotUi>();
    slotItem.SetItem(kitchenObjectSo, quantity);
  }
}
