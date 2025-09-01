using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectSo : ScriptableObject
{
  public Transform prefab;
  public Sprite icon;
  public string objectName;
  public int maxStackedQuantity;
}
