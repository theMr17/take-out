using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUi : MonoBehaviour
{
  [SerializeField] private Transform inventorySlotsContainer;
  [SerializeField] private GameObject inventorySlotPrefab;
  [SerializeField] private TextMeshProUGUI currentSelectedItemText;

  private readonly List<InventorySlotUi> slotUiList = new();

  private void Awake()
  {
    InitializeSlots();
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

  private void InitializeSlots()
  {
    slotUiList.Clear();

    foreach (Transform child in inventorySlotsContainer)
    {
      Destroy(child.gameObject);
    }

    int inventorySize = InventoryManager.GetInventorySize();
    for (int i = 0; i < inventorySize; i++)
    {
      var slotUi = Instantiate(inventorySlotPrefab, inventorySlotsContainer)
        .GetComponent<InventorySlotUi>();

      slotUi.SetSelected(false);
      slotUi.ClearSlot();

      slotUiList.Add(slotUi);
    }
  }

  private void InventoryManager_OnSlotSelectionChanged(object sender, InventoryManager.SelectedSlotEventArgs e)
  {
    for (int i = 0; i < slotUiList.Count; i++)
    {
      if (i == e.SelectedSlot)
      {
        currentSelectedItemText.text = slotUiList[i].GetItemName();
      }

      slotUiList[i].SetSelected(i == e.SelectedSlot);
    }
  }

  private void InventoryManager_OnInventorySlotUpdated(object sender, InventoryManager.InventorySlotEventArgs e)
  {
    if (e.SlotIndex < 0 || e.SlotIndex >= slotUiList.Count) return;

    slotUiList[e.SlotIndex].SetItem(e.KitchenObjectSo, e.Quantity);

    if (e.SlotIndex == InventoryManager.Instance.GetSelectedSlotIndex())
    {
      currentSelectedItemText.text = slotUiList[e.SlotIndex].GetItemName();
    }
  }
}
