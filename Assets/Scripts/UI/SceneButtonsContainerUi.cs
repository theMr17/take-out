using UnityEngine;
using UnityEngine.UI;

public class SceneButtonsContainerUi : MonoBehaviour
{
  [SerializeField] private Button counterSceneButton;
  [SerializeField] private Button storageSceneButton;
  [SerializeField] private Button kitchenSceneButton;

  private void Start()
  {
    if (counterSceneButton != null)
      counterSceneButton.onClick.AddListener(() => OnButtonClicked(SceneLoader.Scene.CounterScene));
    if (storageSceneButton != null)
      storageSceneButton.onClick.AddListener(() => OnButtonClicked(SceneLoader.Scene.StorageScene));
    if (kitchenSceneButton != null)
      kitchenSceneButton.onClick.AddListener(() => OnButtonClicked(SceneLoader.Scene.KitchenScene));
  }

  public void OnButtonClicked(SceneLoader.Scene scene)
  {
    SceneLoader.Instance.LoadScene(scene);
  }
}
