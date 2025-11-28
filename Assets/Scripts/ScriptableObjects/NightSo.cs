using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NightSo : ScriptableObject
{
  public List<KitchenObjectSo> unlockedOrderItems;
  public List<Customer> customers;
  public List<KitchenObjectSo> strangerOrderItems;
}
