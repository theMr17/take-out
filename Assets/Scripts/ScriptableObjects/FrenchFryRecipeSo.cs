using UnityEngine;

[CreateAssetMenu()]
public class FrenchFryRecipeSo : RecipeSo
{
    public KitchenObjectSo input;
    public KitchenObjectSo intermediate;
    public KitchenObjectSo output;
    public float interMediateFryTime;
    public int fryProgressMax;
}
