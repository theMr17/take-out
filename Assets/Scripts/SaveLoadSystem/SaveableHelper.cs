using System;

public class SaveableHelper<T> where T : class, new()
{
  private readonly Func<T> _getSaveData;
  private readonly Action<T> _loadFromSaveData;

  public SaveableHelper(Func<T> getSaveData, Action<T> loadFromSaveData)
  {
    _getSaveData = getSaveData;
    _loadFromSaveData = loadFromSaveData;
  }

  public void Save(string key)
  {
    SaveLoadManager.Save(_getSaveData(), key);
  }

  public void Load(string key)
  {
    var saveData = SaveLoadManager.Load<T>(key);
    if (saveData != null) _loadFromSaveData(saveData);
  }
}
