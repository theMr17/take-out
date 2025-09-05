using UnityEngine.UI;
using UnityEngine;

public class ActiveOrdersUi : MonoBehaviour
{
  [SerializeField] private GameObject activeOrdersPanel;
  [SerializeField] private Button togglePanelButton;

  private Animator animator;

  private void Awake()
  {
    animator = GetComponent<Animator>();
    togglePanelButton.onClick.AddListener(() =>
    {
      animator.SetBool("open", !animator.GetBool("open"));
    });
  }
}
