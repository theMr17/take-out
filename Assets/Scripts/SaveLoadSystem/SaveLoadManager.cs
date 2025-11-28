using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
  private static string SaveFolder => Application.persistentDataPath;

  public static void Save<T>(T data, string fileName)
  {
    string path = Path.Combine(SaveFolder, $"{fileName}.json");
    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(path, json);
    // Debug.Log($"Saved {typeof(T).Name} -> {path}");
  }

  public static T Load<T>(string fileName) where T : new()
  {
    string path = Path.Combine(SaveFolder, $"{fileName}.json");
    if (!File.Exists(path))
    {
      Debug.LogWarning($"No save file found for {fileName}, returning default.");
      return new T();
    }
    string json = File.ReadAllText(path);
    return JsonUtility.FromJson<T>(json);
  }

  public static void Delete(string fileName)
  {
    string path = Path.Combine(SaveFolder, $"{fileName}.json");
    if (File.Exists(path))
    {
      File.Delete(path);
      Debug.Log($"Deleted save file {fileName}");
    }
  }

  public static bool Exists(string fileName)
  {
    string path = Path.Combine(SaveFolder, $"{fileName}.json");
    return File.Exists(path);
  }
}
