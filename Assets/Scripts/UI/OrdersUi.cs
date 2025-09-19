using UnityEngine.UI;
using UnityEngine;

public class OrdersUi : MonoBehaviour
{
  public static OrdersUi Instance { get; private set; }

  [SerializeField] private GameObject activeOrdersPanel;
  [SerializeField] private Button togglePanelButton;

  [SerializeField] private Transform ordersContainer;
  [SerializeField] private GameObject orderItemPrefab;
  [SerializeField] private GameObject noActiveOrdersText;

  private Animator animator;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    animator = GetComponent<Animator>();
    togglePanelButton.onClick.AddListener(() =>
    {
      animator.SetBool("open", !animator.GetBool("open"));
    });
  }

  private void Start()
  {
    GameManager.Instance.OnOrderUpdated += GameManager_OnOrderUpdated;
  }

  private void GameManager_OnOrderUpdated(object sender, GameManager.OnOrderUpdatedArgs e)
  {
    foreach (Transform child in ordersContainer)
    {
      if (child != noActiveOrdersText.transform)
        Destroy(child.gameObject);
    }

    foreach (var item in e.orderItems)
    {
      var orderItemObj = Instantiate(orderItemPrefab, ordersContainer);
      var orderItemUi = orderItemObj.GetComponent<OrderItemUi>();
      orderItemUi.SetKitchenObject(item, cryptic: e.cryptic);
    }

    noActiveOrdersText.SetActive(e.orderItems.Count == 0);
  }
}
