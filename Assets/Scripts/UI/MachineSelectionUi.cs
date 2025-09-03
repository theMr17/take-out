using UnityEngine;
using TMPro;

public class MachineSelectionUi : MonoBehaviour
{
  public static MachineSelectionUi Instance { get; private set; }

  [SerializeField] private GameObject container;
  [SerializeField] private TextMeshProUGUI machineNameText;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;

    container.SetActive(false);
  }

  public void SetMachineSelection(string machineName, bool isSelected)
  {
    if (isSelected)
    {
      container.SetActive(true);
      machineNameText.text = machineName;
    }
    else
    {
      container.SetActive(false);
      machineNameText.text = string.Empty;
    }
  }
}
