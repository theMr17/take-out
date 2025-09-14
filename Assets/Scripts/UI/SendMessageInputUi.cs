using UnityEngine;
using TMPro;
using player2_sdk;

public class SendMessageInputUi : MonoBehaviour
{
  public static SendMessageInputUi Instance { get; private set; }

  [SerializeField] private GameObject sendMessagePanel;
  [SerializeField] private TMP_InputField messageInputField;

  private void Start()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    NpcManager.Instance.OnNpcResponseStateChanged += NpcManager_OnNpcResponseStateChanged;
  }

  private void NpcManager_OnNpcResponseStateChanged(object sender, NpcResponseEventArgs e)
  {
    switch (e.State)
    {
      case NpcResponseState.Received:
        Show();
        break;
    }
  }

  public void Show()
  {
    sendMessagePanel.SetActive(true);
    messageInputField.text = string.Empty;
    messageInputField.ActivateInputField();
  }

  public void Hide()
  {
    sendMessagePanel.SetActive(false);
  }

  public TMP_InputField GetInputField()
  {
    return messageInputField;
  }
}
