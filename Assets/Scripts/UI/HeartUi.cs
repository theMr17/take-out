using UnityEngine;
using UnityEngine.UI;

public class HeartUi : MonoBehaviour
{
  [SerializeField] private Sprite heartFull;
  [SerializeField] private Sprite heartEmpty;

  public void SetHeart(bool isFull)
  {
    GetComponent<Image>().sprite = isFull ? heartFull : heartEmpty;
  }
}
