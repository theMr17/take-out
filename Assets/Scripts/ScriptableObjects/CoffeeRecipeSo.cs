using UnityEngine;

[CreateAssetMenu()]
public class CoffeeRecipeSo : ScriptableObject
{
  public KitchenObjectSo input;
  public KitchenObjectSo output;
  public int fillProgressMax;
}