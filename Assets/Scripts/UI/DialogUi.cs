using player2_sdk;
using TMPro;
using UnityEngine;

public class DialogUi : MonoBehaviour
{
  [SerializeField] private GameObject dialogPanel;
  [SerializeField] private TextMeshProUGUI dialogText;

  private void Start()
  {
    dialogPanel.SetActive(false);
    NpcManager.Instance.OnNpcResponseStateChanged += HandleNpcResponseStateChanged;
  }

  private void OnDestroy()
  {
    NpcManager.Instance.OnNpcResponseStateChanged -= HandleNpcResponseStateChanged;
  }

  private void HandleNpcResponseStateChanged(object sender, NpcResponseEventArgs e)
  {
    switch (e.State)
    {
      case NpcResponseState.Loading:
        dialogText.text = "Thinking...";
        dialogPanel.SetActive(true);
        break;
      case NpcResponseState.Received:
        dialogText.text = e.Message;
        dialogPanel.SetActive(true);
        break;
      case NpcResponseState.Failed:
        dialogText.text = "Error: " + e.Message;
        dialogPanel.SetActive(true);
        break;
    }
  }
}
