using player2_sdk;
using UnityEngine;

public class CounterSceneManager : MonoBehaviour
{
  [SerializeField] private Transform rainSfxTransform;
  [SerializeField] private Transform customerSpawnPoint;

  private void Start()
  {
    SoundManager.Instance.PlayLoopingSound("rain", rainSfxTransform.position, false);
    SoundManager.Instance.PlayLoopingSound("light-flicker", transform.position, false);

    GameManager.Instance.OnCustomerChanged += GameManager_OnCustomerChanged;
  }

  private void GameManager_OnCustomerChanged(object sender, GameManager.OnCustomerChangedArgs e)
  {
    if (e.newCustomer != null)
    {
      var customer = Instantiate(e.newCustomer, customerSpawnPoint);
      customer.transform.localPosition = Vector3.zero;
      customer.transform.localRotation = Quaternion.identity;

      var player2Npc = customer.GetComponent<Player2Npc>();
      player2Npc.SetNpcManager(NpcManager.Instance);
      player2Npc.SetInputField(SendMessageInputUi.Instance.GetInputField());
      _ = player2Npc.SpawnNpcAsync();
      GameManager.Instance.SetCurrentCustomer(player2Npc);
    }
  }
}
