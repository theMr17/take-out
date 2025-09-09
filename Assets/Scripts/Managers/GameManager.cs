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
  public event EventHandler<OnCustomerChangedArgs> OnNewCustomerSpawned;
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
    Load();

    NpcManager.Instance.OnNpcRegistered += NpcManager_OnNpcRegistered;
  }

  public void LoadNextNight()
  {
    currentNightIndex++;
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
    LoadCustomer();
  }

  public void LoadCustomer()
  {
    if (currentNightIndex >= nightSoList.Count)
    {
      Debug.Log("All nights completed!");
      return;
    }

    NightSo currentNight = nightSoList[currentNightIndex];
    if (currentCustomerIndex >= currentNight.customers.Count)
    {
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
    }
  }

  public void NpcManager_OnNpcRegistered(object sender, EventArgs e)
  {
    if (currentCustomer != null)
    {
      _ = currentCustomer.SendChatMessageAsync("Hello! how was your day?");
    }
    else
    {
      Debug.LogWarning("No current customer to send message to.");
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
    return $"Current Night: {currentNightIndex + 1}."
    + $"If you want to place and order, Choose an order from the following available items only: {string.Join(", ", nightSoList[currentNightIndex].unlockedOrderItems.ConvertAll(item => item.name))}. Placing order is not compulsory"
    ;
  }

  public void HandleFunctionCall(FunctionCall functionCall)
  {
    Debug.Log($"Handling function call: {functionCall.name}");
    Debug.Log($"Handling arguments: {functionCall.arguments}");

    switch (functionCall.name)
    {
      case "place-order":
        HandlePlaceOrderFunction(functionCall);
        break;
      case "leave":
        Destroy(currentCustomer.gameObject);
        LoadNextCustomer();
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

  public bool CanSubmitOrder(KitchenObjectSo kitchenObjectSo)
  {
    return currentOrderItems.Contains(kitchenObjectSo);
  }

  public bool SubmitOrder()
  {
    var selectedKitchenObjectSo = InventoryManager.Instance.TakeOneFromSelectedSlot();
    if (selectedKitchenObjectSo != null && CanSubmitOrder(selectedKitchenObjectSo))
    {
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
    else
    {
      Debug.Log("Submitted item is not part of the order.");
      _ = currentCustomer.SendChatMessageAsync("I didn't order that. Please give me what I ordered.");
      return false;
    }
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
