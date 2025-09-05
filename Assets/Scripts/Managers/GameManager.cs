using System.Collections.Generic;
using UnityEngine;

public class GameManager : SaveableBehaviour<GameData>
{
  public static GameManager Instance { get; private set; }

  [SerializeField] private Transform customerSpawnPoint;

  [SerializeField] private List<NightSo> nightSoList;
  private int currentNightIndex = 0;

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

  public override GameData GetSaveData()
  {
    return new GameData
    {
      currentNightIndex = currentNightIndex
    };
  }

  public override void LoadFromSaveData(GameData data)
  {
    currentNightIndex = data.currentNightIndex;
  }

  protected override string GetSaveKey() => "game";
}
