using DG.Tweening;
using UnityEngine;

public class ChangeBubbleButton : MonoBehaviour
{
    [SerializeField] float timeScale;

    void OnMouseDown()
    {
        if (name == "Change" && !Shooter.Instance.IsChange && GameManager.Instance.IsCanPlay)
        {
           Shooter.Instance.IsChange = true;
           OnChangeScale();
        }
    }

    private void OnChangeScale()
    {
        transform.DOScale(Vector3.one * 0.5f, timeScale).SetEase(Ease.Linear).OnComplete(() =>
        {
            Shooter.Instance.ChangeBubble(timeScale);
            transform.DOScale(Vector3.one, timeScale).SetEase(Ease.Linear);
        });
    }

    void OnPress(bool press)
    {
        if (press) return;
    }

}
