using UnityEngine;
using UnityEngine.UI;

public class GameOverUi : MonoBehaviour
{
  [SerializeField] private GameObject gameOverPanel;
  [SerializeField] private Button restartButton;
  [SerializeField] private Button mainMenuButton;

  private void Start()
  {
    restartButton.onClick.AddListener(() => { SceneLoader.Instance.LoadScene(SceneLoader.Scene.CounterScene); });
    mainMenuButton.onClick.AddListener(() => { SceneLoader.Instance.LoadScene(SceneLoader.Scene.MainMenuScene); });

    SaveLoadManager.Delete("game");
  }

  public void Show()
  {
    gameOverPanel.SetActive(true);
  }

  public void Hide()
  {
    gameOverPanel.SetActive(false);
  }
}
