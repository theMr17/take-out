using UnityEngine;

public class CounterSceneManager : MonoBehaviour
{
  [SerializeField] private Transform rainSfxTransform;
  [SerializeField] private Transform customerTransformReference;

  private void Start()
  {
    SoundManager.Instance.PlayLoopingSound("rain", rainSfxTransform.position, false);
    SoundManager.Instance.PlayLoopingSound("light-flicker", transform.position, false);

    SetupCustomerVisual();

    GameManager.Instance.OnNewCustomerSpawned += GameManager_OnCustomerChanged;
  }

  private void GameManager_OnCustomerChanged(object sender, GameManager.OnCustomerChangedArgs e)
  {
    // SetupCustomerVisual();
  }

  private void SetupCustomerVisual()
  {
    GameManager.Instance.SetCustomerPosition(customerTransformReference, false);
  }
}
