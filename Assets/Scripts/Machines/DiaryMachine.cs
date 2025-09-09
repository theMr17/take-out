public class DiaryMachine : BaseMachine<EmptyData>
{
  public override void Interact()
  {
    if (DiaryUi.Instance != null)
      DiaryUi.Instance.Show();
  }
}
