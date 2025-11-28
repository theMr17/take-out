using System.Collections;
using TMPro;
using UnityEngine;

public class NightChangeUi : MonoBehaviour
{
  public static NightChangeUi Instance { get; private set; }

  [SerializeField] private GameObject nightChangePanel;
  [SerializeField] private TextMeshProUGUI nightText;
  [SerializeField] private float displayDuration = 2f;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

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
