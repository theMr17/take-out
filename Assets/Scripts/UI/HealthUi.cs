using System.Collections.Generic;
using UnityEngine;

public class HealthUi : MonoBehaviour
{
  public static HealthUi Instance { get; private set; }
  [SerializeField] private GameObject heartsContainer;
  [SerializeField] private GameObject heartPrefab;

  private readonly List<HeartUi> heartUiList = new();

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
    CreateHeartContainers();

    GameManager.Instance.OnLivesChanged += GameManager_OnLivesChanged;
  }

  private void GameManager_OnLivesChanged(object sender, GameManager.OnLivesChangeArgs e)
  {
    UpdateHealth(e.remainingLives);
  }

  private void CreateHeartContainers()
  {
    heartUiList.Clear();
    foreach (Transform child in heartsContainer.transform)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < GameManager.Instance.totalLives; i++)
    {
      var heartUi = Instantiate(heartPrefab, heartsContainer.transform).GetComponent<HeartUi>();
      heartUiList.Add(heartUi);
    }
  }

  public void UpdateHealth(int currentHealth)
  {
    Debug.Log($"Updating health UI: {currentHealth} hearts");
    for (int i = 0; i < heartUiList.Count; i++)
    {
      heartUiList[i].SetHeart(i < currentHealth);
    }
  }
}
