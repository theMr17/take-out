using UnityEngine;

public class KitchenSceneManager : MonoBehaviour
{
  [SerializeField] private Transform _rainSfxTransform;
  [SerializeField] private Transform customerTransformReference;

  private void Start()
  {
    SoundManager.Instance.PlayLoopingSound("rain", _rainSfxTransform.position, true, 2f);

    SetupCustomerVisual();

    GameManager.Instance.OnNewCustomerSpawned += GameManager_OnCustomerChanged;
  }

  private void GameManager_OnCustomerChanged(object sender, GameManager.OnCustomerChangedArgs e)
  {
    SetupCustomerVisual();
  }

  private void SetupCustomerVisual()
  {
    GameManager.Instance.SetCustomerPosition(customerTransformReference, true);
  }
}
