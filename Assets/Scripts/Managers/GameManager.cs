using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using player2_sdk;
using UnityEngine;

public class GameManager : SaveableBehaviour<GameData>
{
  public static GameManager Instance { get; private set; }

  [SerializeField] private Transform customerSpawnPoint;

  [SerializeField] private List<NightSo> nightSoList;

  private int currentNightIndex = 0;
  private int currentCustomerIndex = 0;
  private Player2Npc currentCustomer;

  private List<KitchenObjectSo> currentOrderItems = new();
  public event EventHandler<OnOrderUpdatedArgs> OnOrderUpdated;
  public class OnOrderUpdatedArgs : EventArgs
  {
    public List<KitchenObjectSo> orderItems;
  }

  public event EventHandler<int> OnNightChanged;
  public event EventHandler<OnCustomerChangedArgs> OnCustomerChanged;
  public class OnCustomerChangedArgs : EventArgs
  {
    public Customer newCustomer;
  }

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
    LoadNextCustomer();

    NpcManager.Instance.OnNpcRegistered += NpcManager_OnNpcRegistered;

    Load();
  }

  public void LoadNextNight()
  {
    currentNightIndex++;
    currentCustomerIndex = 0;
    OnNightChanged?.Invoke(this, currentNightIndex);
    LoadNextCustomer();
  }

  public void LoadNextCustomer()
  {
    if (currentNightIndex >= nightSoList.Count)
    {
      Debug.Log("All nights completed!");
      return;
    }

    NightSo currentNight = nightSoList[currentNightIndex];
    if (currentCustomerIndex >= currentNight.customers.Count)
    {
      Debug.Log("All customers for the night served!");
      return;
    }

    OnCustomerChanged?.Invoke(this, new OnCustomerChangedArgs { newCustomer = currentNight.customers[currentCustomerIndex] });
    currentCustomerIndex++;
  }

  public void SetCurrentCustomer(Player2Npc npc)
  {
    currentCustomer = npc;
  }

  public void NpcManager_OnNpcRegistered(object sender, EventArgs e)
  {
    if (currentCustomer != null)
    {
      _ = currentCustomer.SendChatMessageAsync("Hello! how was your day? say something");
    }
    else
    {
      Debug.LogWarning("No current customer to send message to.");
    }

    PlaceOrder(nightSoList[currentNightIndex].unlockedOrderItems);
  }

  public void PlaceOrder(List<KitchenObjectSo> orderItems)
  {
    currentOrderItems = orderItems;
    OnOrderUpdated?.Invoke(this, new OnOrderUpdatedArgs { orderItems = currentOrderItems });
    Save();
  }

  public string GetCurrentGameStateInfo()
  {
    return $"Current Night: {currentNightIndex + 1}." +
    $" Choose an order from the following available items only: {string.Join(", ", nightSoList[currentNightIndex].unlockedOrderItems.ConvertAll(item => item.name))}.";
  }

  public void HandleFunctionCall(FunctionCall functionCall)
  {
    Debug.Log($"Handling function call: {functionCall.name}");
    Debug.Log($"Handling arguments: {functionCall.arguments}");

    if (functionCall.name == "place-order")
    {
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
  }

  public override GameData GetSaveData()
  {
    return new GameData
    {
      currentNightIndex = currentNightIndex,
      currentCustomerIndex = currentCustomerIndex,
      currentOrderItemNames = currentOrderItems.ConvertAll(item => item.name)
    };
  }

  public override void LoadFromSaveData(GameData data)
  {
    currentNightIndex = data.currentNightIndex;
    currentCustomerIndex = data.currentCustomerIndex;

    currentOrderItems = data.currentOrderItemNames.ConvertAll(name => Resources.Load<KitchenObjectSo>($"ScriptableObjects/KitchenObjects/{name}"));
    OnOrderUpdated?.Invoke(this, new OnOrderUpdatedArgs { orderItems = currentOrderItems });
  }

  protected override string GetSaveKey() => "game";
}
