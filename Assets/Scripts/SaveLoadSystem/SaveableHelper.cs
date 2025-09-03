using System;

public class SaveableHelper<T> where T : class, new()
{
  private readonly string _saveKey;
  private readonly Func<T> _getSaveData;
  private readonly Action<T> _loadFromSaveData;

  public SaveableHelper(string saveKey, Func<T> getSaveData, Action<T> loadFromSaveData)
  {
    _saveKey = saveKey;
    _getSaveData = getSaveData;
    _loadFromSaveData = loadFromSaveData;
  }

  public void Save()
  {
    SaveLoadManager.Save(_getSaveData(), _saveKey);
  }

  public void Load()
  {
    var saveData = SaveLoadManager.Load<T>(_saveKey);
    if (saveData != null) _loadFromSaveData(saveData);
  }
}
