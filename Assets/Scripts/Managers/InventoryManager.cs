using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  public static InventoryManager Instance { get; private set; }

  [Range(1, 9)]
  [SerializeField] private static int maxInventorySize = 8;

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
  private int selectedSlot = 0;

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
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    inventoryItems = new InventoryItem[maxInventorySize];
  }

  private void Start()
  {
    if (InputManager.Instance != null)
    {
      InputManager.Instance.OnSlotKeyPressed += SelectSlot;
      InputManager.Instance.OnScroll += HandleScroll;
    }

    LoadInventory();
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
    if (direction == 1)
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
      SaveInventory();
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

      SaveInventory();
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

      SaveInventory();
    }
    else
    {
      // Slot is occupied by a different item
      SoundManager.Instance.PlaySound("inventory-interact-error", Vector3.zero);
    }
  }

  public static int GetInventorySize() => maxInventorySize;

  public InventoryData GetSaveData()
  {
    InventoryData saveData = new();
    foreach (InventoryItem item in inventoryItems)
    {
      if (item != null)
      {
        saveData.slots.Add(new InventorySlotData
        {
          kitchenObjectId = item.kitchenObjectSo.name, // Using name as ID
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

  public void LoadFromSaveData(InventoryData data)
  {
    for (int i = 0; i < data.slots.Count && i < inventoryItems.Length; i++)
    {
      var slotData = data.slots[i];
      if (!string.IsNullOrEmpty(slotData.kitchenObjectId))
      {
        KitchenObjectSo so = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{slotData.kitchenObjectId}");
        inventoryItems[i] = new InventoryItem(so, slotData.quantity);

        OnInventorySlotUpdated?.Invoke(this, new InventorySlotEventArgs
        {
          slotIndex = i,
          kitchenObjectSo = inventoryItems[i].kitchenObjectSo,
          quantity = inventoryItems[i].Quantity
        });
      }
      else
      {
        inventoryItems[i] = null;
      }
    }
    SelectSlot(data.selectedSlot);
  }

  public void SaveInventory()
  {
    var saveData = GetSaveData();
    SaveLoadManager.Save(saveData, "inventory");
  }

  public void LoadInventory()
  {
    var saveData = SaveLoadManager.Load<InventoryData>("inventory");
    LoadFromSaveData(saveData);
  }
}
