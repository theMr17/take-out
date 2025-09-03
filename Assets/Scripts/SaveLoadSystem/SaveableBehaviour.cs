using UnityEngine;

public abstract class SaveableBehaviour<T> : MonoBehaviour, ISaveable<T> where T : class, new()
{
  public string SaveKey => GetSaveKey();

  protected abstract string GetSaveKey();
  public abstract T GetSaveData();
  public abstract void LoadFromSaveData(T data);

  public void Save()
  {
    SaveLoadManager.Save(GetSaveData(), SaveKey);
  }

  public void Load()
  {
    var saveData = SaveLoadManager.Load<T>(SaveKey);
    if (saveData != null) LoadFromSaveData(saveData);
  }
}
