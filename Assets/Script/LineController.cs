using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    [SerializeField] private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    public void SetUpLine(RectTransform startPoint, Gradient gradient)
    {
        if (lr != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, startPoint.position);
            lr.SetPosition(1, startPoint.position);
            lr.colorGradient = gradient;
        }
    }

    public void SetUpLine(Transform startPoint)
    {
        if (lr != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, startPoint.position);
            lr.SetPosition(1, startPoint.position);
        }
    }

    public void UpdatePoint(Vector3 pos)
    {
        lr.positionCount = 2;
        lr.SetPosition(1, pos);
    }

    public void UpdateNewPoint(Vector3 pos)
    {
        lr.positionCount = 3;
        lr.SetPosition(2, pos);
    }

    public void UpdateGradient(Gradient gradient)
    {
        lr.colorGradient = gradient;
    }


}
