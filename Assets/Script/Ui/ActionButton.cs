using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    protected Button button;
    protected virtual void Awake()
    {
        if(button != null) button = GetComponent<Button>();
        button.onClick.AddListener(OnclickButton);
    }

    protected virtual void OnclickButton()
    {

    }
}
