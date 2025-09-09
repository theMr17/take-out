using Unity.AppUI.UI;
using UnityEngine;

[CreateAssetMenu()]
public class DiaryPageSo : ScriptableObject
{
  public string pageTitle;
  [TextArea(3, 10)]
  public string pageContent;
}
