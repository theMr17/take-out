using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
  public static InputManager Instance { get; private set; }

  private InputSystem_Actions inputActions;

  // Events for slot selection and scroll
  public event Action<int> OnSlotKeyPressed;
  public event Action<int> OnScroll;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    inputActions = new InputSystem_Actions();
  }

  private void OnEnable()
  {
    inputActions.Enable();

    // Number keys (1–9)
    inputActions.Inventory.Select1.performed += ctx => OnSlotKeyPressed?.Invoke(0);
    inputActions.Inventory.Select2.performed += ctx => OnSlotKeyPressed?.Invoke(1);
    inputActions.Inventory.Select3.performed += ctx => OnSlotKeyPressed?.Invoke(2);
    inputActions.Inventory.Select4.performed += ctx => OnSlotKeyPressed?.Invoke(3);
    inputActions.Inventory.Select5.performed += ctx => OnSlotKeyPressed?.Invoke(4);
    inputActions.Inventory.Select6.performed += ctx => OnSlotKeyPressed?.Invoke(5);
    inputActions.Inventory.Select7.performed += ctx => OnSlotKeyPressed?.Invoke(6);
    inputActions.Inventory.Select8.performed += ctx => OnSlotKeyPressed?.Invoke(7);
    inputActions.Inventory.Select9.performed += ctx => OnSlotKeyPressed?.Invoke(8);

    // Scroll
    inputActions.Inventory.Scroll.performed += OnScrollPerformed;
  }

  private void OnScrollPerformed(InputAction.CallbackContext ctx)
  {
    Vector2 scroll = ctx.ReadValue<Vector2>();
    Debug.Log($"Scroll input detected: {scroll}");

    if (scroll.y > 0f)
      OnScroll?.Invoke(-1);
    else if (scroll.y < 0f)
      OnScroll?.Invoke(+1);
  }
}
