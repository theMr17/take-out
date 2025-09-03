public interface ISaveable<T>
{
  string SaveKey { get; }

  T GetSaveData();
  void LoadFromSaveData(T data);
}
