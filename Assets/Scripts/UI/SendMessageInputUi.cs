using UnityEngine;
using TMPro;
using player2_sdk;
using System.Collections.Generic;

public class SendMessageInputUi : MonoBehaviour
{
  public static SendMessageInputUi Instance { get; private set; }

  [SerializeField] private GameObject sendMessagePanel;
  [SerializeField] private TMP_InputField messageInputField;

  [SerializeField] private GameObject optionsPanel;
  [SerializeField] private GameObject optionButtonPrefab;

  private void Start()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    optionsPanel.SetActive(false);

    NpcManager.Instance.OnNpcResponseStateChanged += NpcManager_OnNpcResponseStateChanged;
    GameManager.Instance.OnOptionsReceived += GameManager_OnOptionsReceived;
  }

  private void NpcManager_OnNpcResponseStateChanged(object sender, NpcResponseEventArgs e)
  {
    switch (e.State)
    {
      case NpcResponseState.Received:
        Show();
        break;
      case NpcResponseState.Loading:
        optionsPanel.SetActive(false);
        break;
    }
  }

  private void GameManager_OnOptionsReceived(object sender, GameManager.OnOptionsReceivedArgs e)
  {
    ShowOptions(e.options);
  }

  private void ShowOptions(List<string> options)
  {
    optionsPanel.SetActive(true);

    // Clear existing buttons
    foreach (Transform child in optionsPanel.transform)
    {
      Destroy(child.gameObject);
    }

    // Create new buttons
    foreach (var option in options)
    {
      var buttonObj = Instantiate(optionButtonPrefab, optionsPanel.transform);
      var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
      buttonText.text = option;

      var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
      button.onClick.AddListener(() =>
      {
        GameManager.Instance.SendMessageToCurrentCustomer(option);
        optionsPanel.SetActive(false);
      });
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
