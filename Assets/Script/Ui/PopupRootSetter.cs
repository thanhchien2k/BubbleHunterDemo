using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupRootSetter : MonoBehaviour
{
    public GameObject panelPopup;

    private void Start()
    {
        if (panelPopup == null) panelPopup = this.gameObject;
        panelPopup.SetActive(false);
        PopupManager.Instance.SetPopUpRoot(this);
    }
}
