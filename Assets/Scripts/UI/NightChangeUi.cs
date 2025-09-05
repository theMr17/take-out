using System.Collections;
using UnityEngine;

public class NightChangeUi : MonoBehaviour
{
  [SerializeField] private GameObject nightChangePanel;
  [SerializeField] private float displayDuration = 2f;

  private void Start()
  {
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
