using System.Collections;
using TMPro;
using UnityEngine;

public class NightChangeUi : MonoBehaviour
{
  [SerializeField] private GameObject nightChangePanel;
  [SerializeField] private TextMeshProUGUI nightText;
  [SerializeField] private float displayDuration = 2f;

  private void Start()
  {
    GameManager.Instance.OnNightChanged += GameManager_OnNightChanged;
  }

  private void GameManager_OnNightChanged(object sender, int newNightIndex)
  {
    nightText.text = $"Night {newNightIndex + 1}";
    Show();
    StartCoroutine(HideAfterDelay());
  }

  public void Show()
  {
    nightChangePanel.SetActive(true);
  }

  private IEnumerator HideAfterDelay()
  {
    yield return new WaitForSeconds(displayDuration);
    Hide();
  }

  public void Hide()
  {
    nightChangePanel.SetActive(false);
  }
}
