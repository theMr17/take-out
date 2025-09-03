using UnityEngine;

public class CounterSceneManager : MonoBehaviour
{
  [SerializeField] private Transform _rainSfxTransform;

  private void Start()
  {
    SoundManager.Instance.PlayLoopingSound("rain", _rainSfxTransform.position, false);
  }
}
