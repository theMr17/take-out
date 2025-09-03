using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioClipRefs", menuName = "Sound/AudioClipRefs")]
public class AudioClipRefsSo : ScriptableObject
{
  [System.Serializable]
  public class NamedClip
  {
    public string key;          // e.g. "Rain", "Wind", "Fire"
    public AudioClip clip;
  }

  public List<NamedClip> clips;

  public AudioClip GetClip(string key)
  {
    foreach (var entry in clips)
    {
      if (entry.key == key) return entry.clip;
    }
    Debug.LogWarning("No clip found for key: " + key);
    return null;
  }
}
