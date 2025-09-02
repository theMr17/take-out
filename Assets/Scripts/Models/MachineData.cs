using System;

[Serializable]
public class MachineData
{
  public string machineId; // Unique identifier for machine instance
  public string kitchenObjectId;
  public string state; // Example: brewing, finished, empty
}
