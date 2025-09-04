using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NightSo : ScriptableObject
{
  public List<RecipeSo> unlockedRecipes;
  public List<Customer> customers;
}
