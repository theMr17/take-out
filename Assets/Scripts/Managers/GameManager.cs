using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using player2_sdk;
using UnityEditor.SearchService;
using UnityEngine;

public class GameManager : SaveableBehaviour<GameData>
{
  public static GameManager Instance { get; private set; }

  [SerializeField] private Transform customerSpawnPoint;

  [SerializeField] private List<NightSo> nightSoList;

  private int currentNightIndex = 0;
  private int currentCustomerIndex = 0;
  private Player2Npc currentCustomer;

  public int totalLives = 5;
  public float remainingLives = 3.5f;

  private List<KitchenObjectSo> currentOrderItems = new();
  public event EventHandler<OnOrderUpdatedArgs> OnOrderUpdated;
  public class OnOrderUpdatedArgs : EventArgs
  {
    public List<KitchenObjectSo> orderItems;
  }

  public event EventHandler<int> OnNightChanged;
  public event EventHandler<OnCustomerChangedArgs> OnNewCustomerSpawned;
  public class OnCustomerChangedArgs : EventArgs
  {
    public Customer newCustomer;
  }
  public event EventHandler<OnLivesChangeArgs> OnLivesChanged;
  public class OnLivesChangeArgs : EventArgs
  {
    public float remainingLives;
  }

  private bool leaveAfterThisDialog = false;

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
    Load();

    OnLivesChanged?.Invoke(this, new OnLivesChangeArgs { remainingLives = remainingLives });

    NpcManager.Instance.OnNpcRegistered += NpcManager_OnNpcRegistered;
  }

  public void LoadNextNight()
  {
    currentNightIndex++;
    currentCustomerIndex = 0;

    if (currentNightIndex >= nightSoList.Count)
    {
      Debug.Log("All nights completed!");
      SceneLoader.Instance.LoadScene(SceneLoader.Scene.WinScene);
    }

    Save();
    LoadNight();
  }

  private void LoadNight()
  {
    OnNightChanged?.Invoke(this, currentNightIndex);
    LoadCustomer();
  }

  public void LoadNextCustomer()
  {
    currentCustomerIndex++;
    Save();

    // add a random small delay before loading the next customer
    float randomDelay = UnityEngine.Random.Range(2f, 4f);
    Invoke(nameof(LoadCustomer), randomDelay);
  }

  public void LoadCustomer()
  {
    leaveAfterThisDialog = false;

    if (currentNightIndex >= nightSoList.Count)
    {
      Debug.Log("All nights completed!");
      return;
    }

    NightSo currentNight = nightSoList[currentNightIndex];
    if (currentCustomerIndex >= currentNight.customers.Count)
    {
      Debug.Log("All customers for this night served! Loading next night...");
      LoadNextNight();
      return;
    }

    var newCustomer = currentNight.customers[currentCustomerIndex];

    // Spawn the customer
    if (newCustomer != null)
    {
      var customer = Instantiate(newCustomer, customerSpawnPoint);
      customer.transform.localPosition = Vector3.zero;
      customer.transform.localRotation = Quaternion.identity;

      var player2Npc = customer.GetComponent<Player2Npc>();
      player2Npc.SetNpcManager(NpcManager.Instance);
      player2Npc.SetInputField(SendMessageInputUi.Instance.GetInputField());
      _ = player2Npc.SpawnNpcAsync();
      currentCustomer = player2Npc;

      OnNewCustomerSpawned?.Invoke(this, new OnCustomerChangedArgs { newCustomer = newCustomer });


      SoundManager.Instance.PlaySound("bell", transform.position);
    }
  }

  private void NpcManager_OnNpcRegistered(object sender, EventArgs e)
  {
    if (currentCustomer != null)
    {
      var baseMsg = "Hello! how was your day? ";

      var generalCustomerMsg = $"Place an order, choosing from the following available items only: " +
        $"{string.Join(", ", nightSoList[currentNightIndex].unlockedOrderItems.ConvertAll(item => item.name))}";

      var hoodedStrangerMsg = $"You are a cryptic person, don't place the order directly, but hint the worker that you want {string.Join(", ", nightSoList[currentNightIndex].strangerOrderItems.ConvertAll(item => item.name))}";

      string finalMsg;

      if (currentCustomer.gameObject.name.Contains("Hooded Stranger"))
      {
        finalMsg = baseMsg + hoodedStrangerMsg;
      }
      else
      {
        finalMsg = baseMsg + generalCustomerMsg;
      }

      _ = currentCustomer.SendChatMessageAsync(finalMsg);
    }
    else
    {
      Debug.LogWarning("No current customer to send message to.");
    }
  }

  public void LeaveIfLastDialog()
  {
    if (leaveAfterThisDialog && currentCustomer != null)
    {
      Destroy(currentCustomer.gameObject);
      LoadNextCustomer();

      leaveAfterThisDialog = false;
    }
  }

  public void PlaceOrder(List<KitchenObjectSo> orderItems)
  {
    currentOrderItems = orderItems;
    OnOrderUpdated?.Invoke(this, new OnOrderUpdatedArgs { orderItems = currentOrderItems });
    Save();
  }

  public string GetCurrentGameStateInfo()
  {
    return $"Current Night: {currentNightIndex + 1}.";
  }

  public void HandleFunctionCall(FunctionCall functionCall)
  {
    Debug.Log($"Handling function call: {functionCall.name}");
    Debug.Log($"Handling arguments: {functionCall.arguments}");

    leaveAfterThisDialog = false;

    switch (functionCall.name)
    {
      case "place-order":
        HandlePlaceOrderFunction(functionCall);
        break;
      case "leave":
        HandleLeaveFunction();
        break;
      case "return-wrong-item":
        HandleReturnWrongItemFunction(functionCall);
        break;
      case "heal-player":
        UpdateLife(1f);
        break;
      default:
        Debug.LogWarning($"Unknown function call: {functionCall.name}");
        break;
    }
  }

  private void HandlePlaceOrderFunction(FunctionCall functionCall)
  {
    Debug.Log($"Handling place-order function call");
    Debug.Log($"Handling arguments: {functionCall.arguments}");

    // Access arguments from the JObject
    if (functionCall.arguments.TryGetValue("orderItems", out JToken orderItemsToken))
    {
      List<KitchenObjectSo> orderItems = new();
      foreach (var item in orderItemsToken)
      {
        string itemName = item.ToString();
        KitchenObjectSo kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{itemName}");
        if (kitchenObjectSo != null)
        {
          orderItems.Add(kitchenObjectSo);
        }
        else
        {
          Debug.LogWarning($"KitchenObjectSo with name {itemName} not found.");
        }
      }
      PlaceOrder(orderItems);
    }
    else
    {
      Debug.LogWarning("Function call 'place-order' missing 'orderItems' argument.");
    }
  }

  private void HandleLeaveFunction()
  {
    leaveAfterThisDialog = true;
  }

  private void HandleReturnWrongItemFunction(FunctionCall functionCall)
  {
    Debug.Log($"Handling return-wrong-item function call");
    Debug.Log($"Handling arguments: {functionCall.arguments}");

    if (functionCall.arguments.TryGetValue("kitchenObjectSo", out JToken kitchenObjectSoToken))
    {
      string itemName = kitchenObjectSoToken.ToString();
      KitchenObjectSo kitchenObjectSo = Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{itemName}");
      if (kitchenObjectSo != null)
      {
        InventoryManager.Instance.TryAddToInventory(kitchenObjectSo);
        _ = currentCustomer.SendChatMessageAsync($"You returned ${kitchenObjectSo.objectName}. Thank you for returning the wrong item. Some people just take them away.");
      }
      else
      {
        Debug.LogWarning($"KitchenObjectSo with name {itemName} not found.");
      }
    }
    else
    {
      Debug.LogWarning("Function call 'return-wrong-item' missing 'kitchenObjectSo' argument.");
    }
  }

  public bool IsItemOrdered(KitchenObjectSo kitchenObjectSo)
  {
    return currentOrderItems.Contains(kitchenObjectSo);
  }

  public bool SubmitOrder()
  {
    var selectedKitchenObjectSo = InventoryManager.Instance.TakeOneFromSelectedSlot();
    if (selectedKitchenObjectSo != null)
    {
      if (!IsItemOrdered(selectedKitchenObjectSo))
      {
        _ = currentCustomer.SendChatMessageAsync($"You received ${selectedKitchenObjectSo.objectName}. You did not order that. You can return it to the customer and say something and keep it.");
        UpdateLife(-currentCustomer.GetComponent<Customer>().GetLifeDecreaseOnWrongItem());
        return false;
      }

      currentOrderItems.Remove(selectedKitchenObjectSo);
      OnOrderUpdated?.Invoke(this, new OnOrderUpdatedArgs { orderItems = currentOrderItems });

      if (currentOrderItems.Count == 0)
      {
        Debug.Log("Order completed!");
        _ = currentCustomer.SendChatMessageAsync("The order is complete. Don't order anything else. Just leave now.");
      }
      else
      {
        Debug.Log("Item submitted! Remaining items: " + string.Join(", ", currentOrderItems.ConvertAll(item => item.name)));
        _ = currentCustomer.SendChatMessageAsync($"You received ${selectedKitchenObjectSo.objectName}. Remaining items from your order: " + string.Join(", ", currentOrderItems.ConvertAll(item => item.objectName)));
      }

      Save();
      return true;
    }
    return false;
  }

  private void UpdateLife(float amount)
  {
    remainingLives += amount;
    if (remainingLives < 0) remainingLives = 0;
    if (remainingLives > totalLives) remainingLives = totalLives;

    OnLivesChanged?.Invoke(this, new OnLivesChangeArgs { remainingLives = remainingLives });

    if (remainingLives <= 0)
    {
      Debug.Log("Game Over!");

      SceneLoader.Instance.LoadScene(SceneLoader.Scene.GameOverScene);
    }

    Save();
  }

  public void StartGame()
  {
    LoadNight();
  }

  public void SetCustomerPosition(Transform customerTransformRef, bool mirrorDialogUi = false)
  {
    customerSpawnPoint.transform.localPosition = customerTransformRef.localPosition;
    customerSpawnPoint.transform.localRotation = customerTransformRef.localRotation;

    if (currentCustomer != null)
      currentCustomer.GetComponent<Customer>().MirrorDialogUi(mirrorDialogUi);
  }

  public override GameData GetSaveData()
  {
    return new GameData
    {
      currentNightIndex = currentNightIndex,
      currentCustomerIndex = currentCustomerIndex,
      currentOrderItemNames = currentOrderItems.ConvertAll(item => item.name),
      remainingLives = remainingLives
    };
  }

  public override void LoadFromSaveData(GameData data)
  {
    currentNightIndex = data.currentNightIndex;
    currentCustomerIndex = data.currentCustomerIndex;

    currentOrderItems = data.currentOrderItemNames.ConvertAll(name => Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{name}"));
    OnOrderUpdated?.Invoke(this, new OnOrderUpdatedArgs { orderItems = currentOrderItems });

    remainingLives = data.remainingLives;
  }

  protected override string GetSaveKey() => "game";
}
