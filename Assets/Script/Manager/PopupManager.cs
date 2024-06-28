using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] Vector3 startLocalScale = 0.5f * Vector3.one;
    [SerializeField] float timePopup = 0.5f;
    public PopupRootSetter popupRoot { get; private set; }
    public UIPopup currentPopup { get; private set; }
    Dictionary<string, UIPopup> createdPopups = new();
    bool isOpenPanel = true;

    public void OpenGameObjectPopup(UIPopup gO, bool isOpenPanel = true)
    {
        currentPopup = gO;
        this.isOpenPanel = isOpenPanel;
        PopupEffect(gO);
    }

    public void CreatePopup(UIPopup popup, bool isOpenPanel = true)
    {
        if (popupRoot == null)
        {
            FindPopupRoot();
        }

        this.isOpenPanel = isOpenPanel;

        if (createdPopups.ContainsKey(popup.name))
        {
            if (createdPopups[popup.name] != null)
            {
                currentPopup = createdPopups[popup.name];
                currentPopup.gameObject.SetActive(true);
                return;
            }
            else
            {
                createdPopups.Remove(popup.name);
            }
        }

        currentPopup = Instantiate(popup, popupRoot.transform);
        createdPopups.Add(popup.name, currentPopup);
    }

    public void PopupEffect(UIPopup go)
    {
        Transform goTransform = go.transform;

        if (isOpenPanel)
        {
            popupRoot.panelPopup.SetActive(true);
        }
        else
        {
            popupRoot.panelPopup.SetActive(false);
        }

        goTransform.gameObject.SetActive(true);
        timePopup = timePopup != 0 ? timePopup : 1;
        goTransform.localScale = startLocalScale;
        goTransform.DOScale(Vector3.one, timePopup).SetEase(Ease.Linear).OnComplete(() =>
        {
            if(go.isAutoClose)
            {
                DOVirtual.DelayedCall(1f, ClosePopup);
            }
        });
    }

    public void ClosePopup()
    {
        if (currentPopup == null) return;

        currentPopup.transform.DOScale(Vector3.zero, timePopup).OnComplete(() =>
        {
            popupRoot.panelPopup.SetActive(false);
            currentPopup.gameObject.SetActive(false);
            currentPopup.OnClose();
        });
    }

    public void SetPopUpRoot(PopupRootSetter root)
    {
        popupRoot = root;
    }

    private void FindPopupRoot()
    {
        Debug.Log("find Popup Root");
        popupRoot = FindFirstObjectByType<PopupRootSetter>();

        if (popupRoot == null)
        {
            Canvas curCanVas = FindFirstObjectByType<Canvas>();
            //PopupRootSetter root = Instantiate(GameConfig.PopupRoot);
            //root.transform.SetParent(curCanVas.transform);
        }
    }

    public void SetPanelActive(bool _active)
    {
        popupRoot.panelPopup.gameObject.SetActive(_active);
    }
}
