using player2_sdk;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class DialogUi : MonoBehaviour
{
  [SerializeField] private GameObject dialogPanel;
  [SerializeField] private TextMeshProUGUI dialogText;
  [SerializeField] private float typingSpeed = 0.05f;
  [SerializeField] private float paddingTop = 3f;
  [SerializeField] private float paddingBottom = 7f;

  private Coroutine typingCoroutine;

  private void Start()
  {
    dialogPanel.SetActive(false);

    UpdateDialogPanelSize();

    NpcManager.Instance.OnNpcResponseStateChanged += HandleNpcResponseStateChanged;
  }

  private void OnDestroy()
  {
    if (NpcManager.Instance != null)
      NpcManager.Instance.OnNpcResponseStateChanged -= HandleNpcResponseStateChanged;
  }

  private void HandleNpcResponseStateChanged(object sender, NpcResponseEventArgs e)
  {
    if (typingCoroutine != null)
    {
      StopCoroutine(typingCoroutine);
      typingCoroutine = null;
    }

    switch (e.State)
    {
      case NpcResponseState.Loading:
        dialogPanel.SetActive(true);
        StartPlayingTypingSound();
        typingCoroutine = StartCoroutine(TypeText("Thinking..."));
        break;

      case NpcResponseState.Received:
        dialogPanel.SetActive(true);
        StartPlayingTypingSound();
        typingCoroutine = StartCoroutine(TypeText(e.Message));
        break;

      case NpcResponseState.Failed:
        dialogPanel.SetActive(true);
        StartPlayingTypingSound();
        typingCoroutine = StartCoroutine(TypeText("Error: " + e.Message));
        break;
    }
  }

  private IEnumerator TypeText(string message)
  {
    dialogText.text = "";
    foreach (char c in message)
    {
      dialogText.text += c;
      UpdateDialogPanelSize();
      yield return new WaitForSeconds(typingSpeed);
    }
    StopPlayingTypingSound();
  }

  private void UpdateDialogPanelSize()
  {
    dialogPanel.GetComponent<LayoutElement>().preferredHeight =
      dialogText.preferredHeight + paddingTop + paddingBottom;
  }

  private void StartPlayingTypingSound()
  {
    SoundManager.Instance.PlayLoopingSound("typing", dialogText.transform.position, false);
  }

  private void StopPlayingTypingSound()
  {
    SoundManager.Instance.StopLoopingSound("typing");
  }
}
