using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
  private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

  public static SoundManager Instance { get; private set; }

  [SerializeField] private AudioClipRefsSo _audioClipRefsSo;

  private float _volume = 1f;
  private Dictionary<string, AudioSource> _loopingSounds = new Dictionary<string, AudioSource>();

  private void Awake()
  {
    Instance = this;
    _volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 0.5f);
  }

  public void PlaySound(string key, Vector3 position)
  {
    AudioClip clip = _audioClipRefsSo.GetClip(key);
    if (clip != null)
    {
      GameObject soundObj = new("OneShotSound_" + key);
      soundObj.transform.position = position;

      AudioSource source = soundObj.AddComponent<AudioSource>();
      source.clip = clip;
      source.volume = _volume;
      source.spatialBlend = 0f; // 2D sound
      source.Play();

      Destroy(soundObj, clip.length);
    }
  }

  public void PlayLoopingSound(string key, Vector3 position, bool spatial = true)
  {
    if (_loopingSounds.ContainsKey(key)) return;

    AudioClip clip = _audioClipRefsSo.GetClip(key);
    if (clip == null) return;

    GameObject soundObj = new GameObject("LoopingSound_" + key);
    soundObj.transform.position = position;

    AudioSource source = soundObj.AddComponent<AudioSource>();
    source.clip = clip;
    source.loop = true;
    source.volume = _volume;
    source.spatialBlend = spatial ? 1f : 0f; // 3D or 2D
    source.Play();

    _loopingSounds[key] = source;
  }

  public void StopLoopingSound(string key)
  {
    if (_loopingSounds.TryGetValue(key, out var source))
    {
      source.Stop();
      Destroy(source.gameObject);
      _loopingSounds.Remove(key);
    }
  }

  public void StopAllLoopingSounds()
  {
    foreach (var source in _loopingSounds.Values)
    {
      source.Stop();
      Destroy(source.gameObject);
    }
    _loopingSounds.Clear();
  }

  public void ChangeVolume(float volume)
  {
    _volume = volume;
    PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
    PlayerPrefs.Save();

    foreach (var source in _loopingSounds.Values)
    {
      source.volume = _volume;
    }
  }

  public float GetVolume() => _volume;
}
