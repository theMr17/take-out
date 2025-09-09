using UnityEngine;

[CreateAssetMenu()]
public class GrillRecipeSo : RecipeSo
{
    public KitchenObjectSo input;
    public KitchenObjectSo intermediate;
    public KitchenObjectSo output;
    public KitchenObjectSo burntOutput;
    public float interMediateGrillTime;
    public float burntGrillTime;
    public int grillProgressMax;
}
