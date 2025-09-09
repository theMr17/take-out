using System;
using UnityEngine;

public class InventoryManager : SaveableBehaviour<InventoryData>
{
  public static InventoryManager Instance { get; private set; }

  private const int INVENTORY_SLOT_COUNT = 8;

  class InventoryItem
  {
    public KitchenObjectSo KitchenObjectSo { get; }
    public int Quantity { get; set; }

    public InventoryItem(KitchenObjectSo kitchenObjectSo, int quantity)
    {
      KitchenObjectSo = kitchenObjectSo;
      Quantity = quantity;
    }
  }

  private InventoryItem[] inventoryItems;
  private int selectedSlot = 0;

  public event EventHandler<SelectedSlotEventArgs> OnSlotSelectionChanged;
  public class SelectedSlotEventArgs : EventArgs
  {
    public int SelectedSlot;
  }

  public event EventHandler<InventorySlotEventArgs> OnInventorySlotUpdated;
  public class InventorySlotEventArgs : EventArgs
  {
    public int SlotIndex;
    public KitchenObjectSo KitchenObjectSo;
    public int Quantity;
  }

  protected override string GetSaveKey() => "inventory";

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    inventoryItems = new InventoryItem[INVENTORY_SLOT_COUNT];
  }

  private void Start()
  {
    if (InputManager.Instance != null)
    {
      InputManager.Instance.OnSlotKeyPressed += SelectSlot;
      InputManager.Instance.OnScroll += HandleScroll;
    }

    Load();
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
    int newSlot = direction == 1
      ? (selectedSlot + 1) % INVENTORY_SLOT_COUNT
      : (selectedSlot - 1 + INVENTORY_SLOT_COUNT) % INVENTORY_SLOT_COUNT;

    SelectSlot(newSlot);
  }

  private void SelectSlot(int slotIndex)
  {
    if (slotIndex < 0 || slotIndex >= INVENTORY_SLOT_COUNT) return;

    selectedSlot = slotIndex;
    OnSlotSelectionChanged?.Invoke(this, new SelectedSlotEventArgs { SelectedSlot = selectedSlot });
    Save();
  }

  public bool TryPickupObject(KitchenObjectSo kitchenObjectSo)
  {
    if (kitchenObjectSo == null || selectedSlot < 0 || selectedSlot >= INVENTORY_SLOT_COUNT) return false;

    InventoryItem slotItem = inventoryItems[selectedSlot];

    // Check if the slot is already occupied with the same item
    if (slotItem != null && slotItem.KitchenObjectSo == kitchenObjectSo)
    {
      // Check if the slot is full
      if (slotItem.Quantity >= kitchenObjectSo.maxStackedQuantity)
      {
        SoundManager.Instance?.PlaySound("inventory-interact-error", Vector3.zero);
        return false;
      }

      slotItem.Quantity++;
      OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
      {
        SlotIndex = selectedSlot,
        KitchenObjectSo = kitchenObjectSo,
        Quantity = slotItem.Quantity
      });

      SoundManager.Instance?.PlaySound("inventory-interact-success", Vector3.zero);
      Save();
    }
    else if (slotItem == null)
    {
      // Create a new inventory item
      inventoryItems[selectedSlot] = new InventoryItem(kitchenObjectSo, 1);
      OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
      {
        SlotIndex = selectedSlot,
        KitchenObjectSo = kitchenObjectSo,
        Quantity = 1
      });

      SoundManager.Instance?.PlaySound("inventory-interact-success", Vector3.zero);
      Save();
    }
    else
    {
      // Slot is occupied by a different item
      SoundManager.Instance?.PlaySound("inventory-interact-error", Vector3.zero);
      return false;
    }
    return true;
  }

  public KitchenObjectSo GetKitchenObjectSoFromSelectedSlot()
  {
    if (selectedSlot < 0 || selectedSlot >= INVENTORY_SLOT_COUNT) return null;

    InventoryItem slotItem = inventoryItems[selectedSlot];
    return slotItem?.KitchenObjectSo;
  }

  public KitchenObjectSo TakeOneFromSelectedSlot()
  {
    if (selectedSlot < 0 || selectedSlot >= INVENTORY_SLOT_COUNT) return null;

    InventoryItem slotItem = inventoryItems[selectedSlot];
    if (slotItem != null)
    {
      slotItem.Quantity--;

      if (slotItem.Quantity <= 0)
      {
        inventoryItems[selectedSlot] = null;
      }

      OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
      {
        SlotIndex = selectedSlot,
        KitchenObjectSo = inventoryItems[selectedSlot]?.KitchenObjectSo,
        Quantity = inventoryItems[selectedSlot]?.Quantity ?? 0
      });

      Save();

      return slotItem.KitchenObjectSo;
    }

    return null;
  }

  public static int GetInventorySize() => INVENTORY_SLOT_COUNT;

  public override InventoryData GetSaveData()
  {
    var saveData = new InventoryData();

    foreach (InventoryItem item in inventoryItems)
    {
      if (item != null)
      {
        saveData.slots.Add(new InventorySlotData
        {
          kitchenObjectId = item.KitchenObjectSo.name,
          quantity = item.Quantity
        });
      }
      else
      {
        saveData.slots.Add(new InventorySlotData { kitchenObjectId = "", quantity = 0 });
      }
    }

    saveData.selectedSlot = selectedSlot;
    return saveData;
  }

  public override void LoadFromSaveData(InventoryData data)
  {
    for (int i = 0; i < inventoryItems.Length && i < data.slots.Count; i++)
    {
      var slotData = data.slots[i];
      if (!string.IsNullOrEmpty(slotData.kitchenObjectId))
      {
        var kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{slotData.kitchenObjectId}");
        inventoryItems[i] = new InventoryItem(kitchenObjectSo, slotData.quantity);

        OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
        {
          SlotIndex = i,
          KitchenObjectSo = kitchenObjectSo,
          Quantity = slotData.quantity
        });
      }
      else
      {
        inventoryItems[i] = null;
      }
    }

    SelectSlot(data.selectedSlot);
  }
}
