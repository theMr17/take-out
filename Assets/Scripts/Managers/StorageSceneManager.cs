using UnityEngine;

public class StorageSceneManager : MonoBehaviour
{
  [SerializeField] private Transform _rainSfxTransform;

  private void Start()
  {
    SoundManager.Instance.PlayLoopingSound("rain", _rainSfxTransform.position, true, 2f);
  }
}
