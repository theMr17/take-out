using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
  public static SceneLoader Instance { get; private set; }

  public Animator transitionAnimator;
  public float transitionDuration = 1f;

  public enum Scene
  {
    MainMenuScene,
    CounterScene,
    StorageScene,
    KitchenScene,
    GameOverScene,
  }

  private void Awake()
  {
    Instance = this;
  }

  public void LoadScene(Scene targetScene, bool useTransition = true)
  {
    if (useTransition && transitionAnimator != null)
    {
      StartCoroutine(LoadSceneWithTransition(targetScene));
    }
    else
    {
      SceneManager.LoadScene(targetScene.ToString());
    }
  }

  IEnumerator LoadSceneWithTransition(Scene targetScene)
  {
    transitionAnimator.SetTrigger("Start");
    yield return new WaitForSeconds(transitionDuration);
    SceneManager.LoadScene(targetScene.ToString());
  }

  private Scene GetCurrentScene()
  {
    return (Scene)System.Enum.Parse(typeof(Scene), SceneManager.GetActiveScene().name);
  }
}