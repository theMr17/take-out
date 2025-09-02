using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
  public InventoryData inventory;
  public List<MachineData> machines = new();
}