using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private EdgeCollider2D edgeCollider;
    [SerializeField]private Camera mainCamera;

    void Start()
    {
        FitCollider();
    }

    void FitCollider()
    {
        Vector2 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector2 topLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, mainCamera.pixelHeight, mainCamera.nearClipPlane));
        Vector2 topRight = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, mainCamera.nearClipPlane));
        Vector2 bottomRight = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, 0, mainCamera.nearClipPlane));

        Vector2[] points = new Vector2[5];
        points[0] = bottomLeft;
        points[1] = topLeft;
        points[2] = topRight;
        points[3] = bottomRight;
        points[4] = bottomLeft;

        edgeCollider.points = points;
    }
}
