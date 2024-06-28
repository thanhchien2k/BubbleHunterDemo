
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public bool isOpenPanel = true;
    public bool isAutoClose = false;
    private void OnEnable()
    {
        StratPopup();
    }

    protected virtual void StratPopup()
    {
        PopupManager.Instance.OpenGameObjectPopup(this);
    }

    public virtual void OnClose()
    {

    }
}