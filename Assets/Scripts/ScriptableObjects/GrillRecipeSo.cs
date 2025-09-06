using UnityEngine;

[CreateAssetMenu()]
public class GrillRecipeSo : RecipeSo
{
    public KitchenObjectSo input;
    public KitchenObjectSo intermediate;
    public KitchenObjectSo output;
    public float interMediateGrillTime;
    public int grillProgressMax;
}
