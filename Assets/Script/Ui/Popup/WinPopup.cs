
public class WinPopup : UIPopup
{
    public override void OnClose()
    {
        base.OnClose();
        SceneTransition.Instance.LoadScene(0);
    }
}
