using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  public static InventoryManager Instance { get; private set; }

  [Range(1, 9)]
  [SerializeField] private int maxInventorySize = 6;

  class InventoryItem
  {
    public KitchenObject KitchenObject { get; }
    public int Quantity { get; private set; }

    public InventoryItem(KitchenObject kitchenObject, int quantity)
    {
      KitchenObject = kitchenObject;
      Quantity = quantity;
    }
  }

  private List<InventoryItem> inventoryItems;
  private int selectedSlot = -1;

  public event EventHandler<SelectedSlotEventArgs> OnSlotSelectionChanged;
  public class SelectedSlotEventArgs : EventArgs
  {
    public int selectedSlot;
  }

  private void Awake()
  {
    Instance = this;
    inventoryItems = new List<InventoryItem>(maxInventorySize);
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
    InputManager.Instance.OnSlotKeyPressed += SelectSlot;
    InputManager.Instance.OnScroll += HandleScroll;
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

  public int GetInventorySize() => maxInventorySize;
}
