using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [SerializeField] private EdgeCollider2D edgeCollider;
    [SerializeField] private BoxCollider2D roofCollider;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float _cameraTransitionSpeed = 0.05f;

    Vector3 _newCameraPosition;
    bool _cameraIsMoving;
    int _minBubbleHeightIndex;

    Vector2 _edgeLeftBottom, _edgeRightBottom;
    public Camera _camera;
    public float _fieldVertExtent, _fieldHorExtent;
    void Start()
    {
        FitCollider();
        //_camera = Camera.main;
        //_camera.transform.position = Vector3.zero;
        //_newCameraPosition = _camera.transform.position;
        _cameraIsMoving = false;
        _minBubbleHeightIndex = 5;

        //_fieldVertExtent = _camera.orthographicSize;
        _fieldHorExtent = _fieldVertExtent * Screen.width / Screen.height;

        //StartMoveCamera();
    }

    void FitCollider()
    {
        _edgeLeftBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector2 topLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, mainCamera.pixelHeight, mainCamera.nearClipPlane));
        Vector2 topRight = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, mainCamera.nearClipPlane));
        _edgeRightBottom = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, 0, mainCamera.nearClipPlane));

        Vector2[] points = new Vector2[5];
        points[0] = _edgeLeftBottom;
        points[1] = topLeft;
        points[2] = topRight;
        points[3] = _edgeRightBottom;
        points[4] = _edgeLeftBottom;

        edgeCollider.points = points;
        roofCollider.size = new Vector2(topRight.x - topLeft.x, 0.1f); ;
        roofCollider.offset = new Vector2(0 ,(topRight.y - _edgeRightBottom.y) / 2);
    }

    public void StartMoveCamera(Vector3 targetPos, float time ,Action action)
    {
        _newCameraPosition = targetPos;
        mainCamera.transform.DOMove(_newCameraPosition, time).SetEase(Ease.Linear).OnComplete(action.Invoke);
        
    }


    public void AdjustCamera()
    {
        Tilemap tilemap = GameManager.Instance.GetTilemmap();
        Vector3 highestPoint = GameManager.Instance.GetHighestBubblePos();

        int highestCellIndex = tilemap.WorldToCell(highestPoint).y;
        int lowestCellIndex = tilemap.WorldToCell(GameManager.Instance.GetLowestBubblePos()).y;
        Vector3 centerPosWorld = _camera.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
        int cameraCenterCellIndex = tilemap.WorldToCell(centerPosWorld).y;

        if (highestCellIndex < tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y - 1)
        {
            Debug.Log("1");
            var differenceIndex = tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y - 1 - highestCellIndex;
            var newPos = tilemap.CellToWorld(new Vector3Int(0, cameraCenterCellIndex - differenceIndex, 0));
            var differenceHeight = newPos.y - _camera.transform.position.y;
            var adjustVector = new Vector3(0, differenceHeight, 0);

            _newCameraPosition += adjustVector;
            //_roof.position += adjustVector;
            _minBubbleHeightIndex -= differenceIndex;
            _cameraIsMoving = true;
        }

        else if (lowestCellIndex > _minBubbleHeightIndex)
        {
            Debug.Log("2");

            var differenceIndex = lowestCellIndex - _minBubbleHeightIndex;

            if (highestCellIndex - differenceIndex <= tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y + 1)
            {
                differenceIndex = highestCellIndex - tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y + 1;
            }

            var newPos = tilemap.CellToWorld(new Vector3Int(0, cameraCenterCellIndex + differenceIndex, 0));
            var differenceHeight = newPos.y - _camera.transform.position.y;
            var adjustVector = new Vector3(0, differenceHeight, 0);

            _newCameraPosition += adjustVector;
            //_roof.position += adjustVector;
            _minBubbleHeightIndex += differenceIndex;
            _cameraIsMoving = true;
        }
        else if (lowestCellIndex < _minBubbleHeightIndex)
        {
            Debug.Log("3");

            var differenceIndex = _minBubbleHeightIndex - lowestCellIndex;
            var newPos = tilemap.CellToWorld(new Vector3Int(0, cameraCenterCellIndex - differenceIndex, 0));
            var differenceHeight = newPos.y - _camera.transform.position.y;
            var adjustVector = new Vector3(0, differenceHeight, 0);

            _newCameraPosition += adjustVector;
            //_roof.position += adjustVector;
            _minBubbleHeightIndex -= differenceIndex;
            _cameraIsMoving = true;
        }
    }

    //private void Update()
    //{
    //    if (_camera != null && _newCameraPosition != null && _cameraIsMoving)
    //    {
    //        _camera.transform.position = Vector2.Lerp(_camera.transform.position, _newCameraPosition, _cameraTransitionSpeed);
    //        if (Vector2.Distance(_camera.transform.position, _newCameraPosition) <= 0.2f)
    //        {
    //            _cameraIsMoving = false;
    //        }
    //    }
    //}
}
