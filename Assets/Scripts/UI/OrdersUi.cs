using UnityEngine.UI;
using UnityEngine;

public class OrdersUi : MonoBehaviour
{
  [SerializeField] private GameObject activeOrdersPanel;
  [SerializeField] private Button togglePanelButton;

  [SerializeField] private Transform ordersContainer;
  [SerializeField] private GameObject orderItemPrefab;

  private Animator animator;

  private void Awake()
  {
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
      Destroy(child.gameObject);
    }

    foreach (var item in e.orderItems)
    {
      var orderItemObj = Instantiate(orderItemPrefab, ordersContainer);
      var orderItemUi = orderItemObj.GetComponent<OrderItemUi>();
      orderItemUi.SetKitchenObject(item);
    }
  }
}
