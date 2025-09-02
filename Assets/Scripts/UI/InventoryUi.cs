using System.Collections.Generic;
using UnityEngine;

public class InventoryUi : MonoBehaviour
{
  [SerializeField] private Transform inventorySlotsContainer;
  [SerializeField] private GameObject inventorySlotPrefab;

  private List<InventorySlotUi> slotUiList = new();

  private void Awake()
  {
    CreateInventorySlots();
  }

  private void Start()
  {
    InventoryManager.Instance.OnSlotSelectionChanged += InventoryManager_OnSlotSelectionChanged;
    InventoryManager.Instance.OnInventorySlotUpdated += InventoryManager_OnInventorySlotUpdated;
  }

  private void OnDestroy()
  {
    InventoryManager.Instance.OnSlotSelectionChanged -= InventoryManager_OnSlotSelectionChanged;
    InventoryManager.Instance.OnInventorySlotUpdated -= InventoryManager_OnInventorySlotUpdated;
  }

  private void CreateInventorySlots()
  {
    slotUiList.Clear();
    foreach (Transform child in inventorySlotsContainer)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < InventoryManager.GetInventorySize(); i++)
    {
      InventorySlotUi slotItem = Instantiate(inventorySlotPrefab, inventorySlotsContainer).GetComponent<InventorySlotUi>();
      slotItem.SetSelected(false);
      slotItem.ClearSlot();
      slotUiList.Add(slotItem);
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
    for (int i = 0; i < InventoryManager.GetInventorySize(); i++)
    {
      slotUiList[i].SetSelected(i == selectedSlot);
    }
  }

  private void UpdateInventorySlot(int slotIndex, KitchenObjectSo kitchenObjectSo, int quantity)
  {
    InventorySlotUi slotItem = slotUiList[slotIndex];
    slotItem.SetItem(kitchenObjectSo, quantity);
  }
}
