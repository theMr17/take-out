using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  public static InventoryManager Instance { get; private set; }

  [Range(1, 9)]
  [SerializeField] private int maxInventorySize = 6;

  class InventoryItem
  {
    public KitchenObjectSo kitchenObjectSo;
    public int Quantity;

    public InventoryItem(KitchenObjectSo kitchenObjectSo, int quantity)
    {
      this.kitchenObjectSo = kitchenObjectSo;
      Quantity = quantity;
    }
  }

  private InventoryItem[] inventoryItems;
  private int selectedSlot = -1;

  public event EventHandler<SelectedSlotEventArgs> OnSlotSelectionChanged;
  public class SelectedSlotEventArgs : EventArgs
  {
    public int selectedSlot;
  }
  public event EventHandler<InventorySlotEventArgs> OnInventorySlotUpdated;
  public class InventorySlotEventArgs : EventArgs
  {
    public int slotIndex;
    public KitchenObjectSo kitchenObjectSo;
    public int quantity;
  }

  private void Awake()
  {
    Instance = this;
    inventoryItems = new InventoryItem[maxInventorySize];
  }

  private void Update()
  {
    if (selectedSlot == -1)
    {
      SelectSlot(0);
    }
  }

  private void Start()
  {
    if (InputManager.Instance != null)
    {
      InputManager.Instance.OnSlotKeyPressed += SelectSlot;
      InputManager.Instance.OnScroll += HandleScroll;
    }
  }

  private void OnDestroy()
  {
    if (InputManager.Instance != null)
    {
      InputManager.Instance.OnSlotKeyPressed -= SelectSlot;
      InputManager.Instance.OnScroll -= HandleScroll;
    }
  }

  private void HandleScroll(int direction)
  {
    if (direction > 0)
    {
      int nextSlot = (selectedSlot + 1) % maxInventorySize;
      SelectSlot(nextSlot);
    }
    else
    {
      int prevSlot = (selectedSlot - 1 + maxInventorySize) % maxInventorySize;
      SelectSlot(prevSlot);
    }
  }

  private void SelectSlot(int slotIndex)
  {
    if (slotIndex >= 0 && slotIndex < maxInventorySize)
    {
      selectedSlot = slotIndex;
      OnSlotSelectionChanged?.Invoke(this, new SelectedSlotEventArgs { selectedSlot = selectedSlot });
    }
  }

  public void TryPickupObject(KitchenObjectSo kitchenObjectSO)
  {
    if (kitchenObjectSO == null)
    {
      return;
    }

    // Check if the selected slot is valid
    if (selectedSlot < 0 || selectedSlot >= maxInventorySize)
    {
      return;
    }

    InventoryItem slotItem = inventoryItems[selectedSlot];

    // Check if the slot is already occupied with the same item
    if (slotItem != null && slotItem.kitchenObjectSo == kitchenObjectSO)
    {
      // Check if the slot is full
      if (slotItem.Quantity >= kitchenObjectSO.maxStackedQuantity)
      {
        SoundManager.Instance.PlaySound("inventory-interact-error", Vector3.zero);
        return;
      }
      slotItem.Quantity++;
      OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
      {
        slotIndex = selectedSlot,
        kitchenObjectSo = kitchenObjectSO,
        quantity = slotItem.Quantity
      });

      SoundManager.Instance.PlaySound("inventory-interact-success", Vector3.zero);
    }
    else if (slotItem == null)
    {
      // Create a new inventory item
      inventoryItems[selectedSlot] = new InventoryItem(kitchenObjectSO, 1);
      OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
      {
        slotIndex = selectedSlot,
        kitchenObjectSo = kitchenObjectSO,
        quantity = 1
      });

      SoundManager.Instance.PlaySound("inventory-interact-success", Vector3.zero);
    }
    else
    {
      // Slot is occupied by a different item
      SoundManager.Instance.PlaySound("inventory-interact-error", Vector3.zero);
    }
  }

  public int GetInventorySize() => maxInventorySize;
}
