public class GameManager : SaveableBehaviour<GameData>
{
  public static GameManager Instance { get; private set; }

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
    throw new System.NotImplementedException();
  }

  public override void LoadFromSaveData(GameData data)
  {
    throw new System.NotImplementedException();
  }

  protected override string GetSaveKey() => "gameState";
}
