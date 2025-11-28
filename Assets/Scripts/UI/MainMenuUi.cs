using UnityEngine;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private GameObject _settingsPanel;

    private void Awake()
    {
        _continueButton.onClick.AddListener(OnContinueButtonClicked);
        _newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        _settingsButton.onClick.AddListener(OnSettingsButtonClicked);

        _continueButton.interactable = SaveLoadManager.Exists("game");
    }

    private void OnContinueButtonClicked()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.Scene.CounterScene);
    }

    private void OnSettingsButtonClicked()
    {
        _settingsPanel.SetActive(true);
    }

    private void OnNewGameButtonClicked()
    {
        SaveLoadManager.Delete("game");
        SceneLoader.Instance.LoadScene(SceneLoader.Scene.CounterScene);
    }
}
