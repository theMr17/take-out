using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DiaryUi : MonoBehaviour
{
  [SerializeField] private GameObject diaryPanel;
  [SerializeField] private TextMeshProUGUI titleText;
  [SerializeField] private TextMeshProUGUI contentText;
  [SerializeField] private Button nextPage;
  [SerializeField] private Button previousPage;
  [SerializeField] private Button closeButton;
  [SerializeField] private List<DiaryPageSo> diaryPages;

  private Animator animator;
  private int currentPageIndex = 0;

  public static DiaryUi Instance { get; private set; }

  private void Awake()
  {
    if (Instance != null)
    {
      Debug.LogError("There is more than one DiaryUi instance");
      Destroy(gameObject);
      return;
    }
    Instance = this;

    animator = GetComponent<Animator>();

    previousPage.gameObject.SetActive(currentPageIndex > 0);
    nextPage.gameObject.SetActive(currentPageIndex < diaryPages.Count - 1);

    nextPage.onClick.AddListener(() => ChangePage(+1));
    previousPage.onClick.AddListener(() => ChangePage(-1));

    closeButton.onClick.AddListener(Hide);
  }

  private void ChangePage(int direction)
  {
    if (diaryPages == null || diaryPages.Count == 0) return;

    int newPageIndex = currentPageIndex + direction;
    newPageIndex = Mathf.Clamp(newPageIndex, 0, diaryPages.Count - 1);

    if (newPageIndex == currentPageIndex) return;

    currentPageIndex = newPageIndex;

    if (direction > 0)
      animator.SetTrigger("backward");
    else
      animator.SetTrigger("forward");

    SoundManager.Instance.PlaySound("flip-page", this.transform.position);

    var page = diaryPages[currentPageIndex];
    titleText.text = page.pageTitle;
    contentText.text = page.pageContent;

    previousPage.gameObject.SetActive(currentPageIndex > 0);
    nextPage.gameObject.SetActive(currentPageIndex < diaryPages.Count - 1);
  }

  public void Show()
  {
    diaryPanel.SetActive(true);
  }

  public void Hide()
  {
    diaryPanel.SetActive(false);
  }
}
