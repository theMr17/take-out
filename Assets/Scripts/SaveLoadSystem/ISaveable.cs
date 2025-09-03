public interface ISaveable<T>
{
  T GetSaveData();
  void LoadFromSaveData(T data);
}
