using UnityEngine;

[CreateAssetMenu()]
public class OvenRecipeSo : RecipeSo
{
    public KitchenObjectSo input;
    public KitchenObjectSo intermediate;
    public KitchenObjectSo output;
    public KitchenObjectSo burntOutput;
    public float interMediateBakeTime;
    public float burntBakeTime;
    public int bakeProgressMax;
}
