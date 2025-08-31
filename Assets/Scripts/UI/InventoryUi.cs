using UnityEngine;

public class InventoryUi : MonoBehaviour
{
  [SerializeField] private Transform inventorySlotsContainer;
  [SerializeField] private GameObject inventorySlotPrefab;
  [SerializeField] private GameObject inventorySelectionIndicator;

  private void Start()
  {
    CreateInventorySlots();

    InventoryManager.Instance.OnSlotSelectionChanged += InventoryManager_OnSlotSelectionChanged;
  }

  private void CreateInventorySlots()
  {
    foreach (Transform child in inventorySlotsContainer)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < InventoryManager.Instance.GetInventorySize(); i++)
    {
      Instantiate(inventorySlotPrefab, inventorySlotsContainer);
    }
  }

  private void InventoryManager_OnSlotSelectionChanged(object sender, InventoryManager.SelectedSlotEventArgs e)
  {
    UpdateSelectionIndicator(e.selectedSlot);
  }

  private void UpdateSelectionIndicator(int selectedSlot)
  {
    inventorySelectionIndicator.transform.SetParent(inventorySlotsContainer.GetChild(selectedSlot));
    inventorySelectionIndicator.transform.localPosition = Vector3.zero;
  }
}
