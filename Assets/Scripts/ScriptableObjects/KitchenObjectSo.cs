using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject
{
  public Transform prefab;
  public Sprite icon;
  public string objectName;
  public int maxStackedQuantity;
}
