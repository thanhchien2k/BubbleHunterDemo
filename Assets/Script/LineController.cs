using System;
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

    public void UpdatePoint(int index, Vector3 pos)
    {
        if (lr == null) return;
        lr.positionCount = index + 1;
        lr.SetPosition(index, pos);
    }

}
