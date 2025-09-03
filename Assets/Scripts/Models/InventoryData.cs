using System;
using System.Collections.Generic;

[Serializable]
public class InventorySlotData
{
  public string kitchenObjectId; // Reference ID of KitchenObjectSo
  public int quantity;
}

[Serializable]
public class InventoryData
{
  public List<InventorySlotData> slots = new();
  public int selectedSlot = 0;
}