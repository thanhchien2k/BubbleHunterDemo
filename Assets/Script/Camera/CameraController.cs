using DG.Tweening;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [SerializeField] private EdgeCollider2D edgeCollider;
    [SerializeField] private BoxCollider2D roofCollider;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraTransitionTime = 0.1f;
    [SerializeField] int rowMoveCamera = 3;
    [SerializeField] int rowFixCamera = 7;

    Vector3 lowestPos;
    int minBubbleHeightIndex;
    Vector2 _edgeLeftBottom, _edgeRightBottom;

    public  bool CameraIsMoving { get; set; }
    public bool IsStart { get; set; } = true;
    public bool IsFixed { get; set; } = false;

    void Start()
    {
        AdjustTilemap();
        FitCollider();
        CameraIsMoving = false;
    }

    void FitCollider()
    {
        _edgeLeftBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector2 topLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, mainCamera.pixelHeight, mainCamera.nearClipPlane));
        Vector2 topRight = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, mainCamera.nearClipPlane));
        _edgeRightBottom = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, 0, mainCamera.nearClipPlane));

        Vector2[] points = new Vector2[5];
        points[0] = _edgeLeftBottom;
        points[1] = topLeft + Vector2.up * 5;
        points[2] = topRight + Vector2.up * 5;
        points[3] = _edgeRightBottom;
        points[4] = _edgeLeftBottom;

        edgeCollider.points = points;
        roofCollider.size = new Vector2(topRight.x - topLeft.x, 0.1f); ;
        roofCollider.offset = new Vector2(0 , GameManager.Instance.GetHighestBubblePos().y + GameManager.Instance.CellSize.y / 2);
    }

    public void StartMoveCamera(Vector3 heigest,Vector3 lowest, float ySize,Action action)
    {
        if(lowestPos == lowest)
        {
            action.Invoke();
            CameraIsMoving = false;
            return;
        }

        Vector2 targetPos = new Vector2(0, lowest.y - ySize * rowMoveCamera);

        if (IsStart)
        {
            IsStart = false;
        }
        else
        {
            float distance = heigest.y - lowest.y - rowFixCamera * ySize;

            if (distance < 0)
            {
                targetPos.y = GamePlayCanvasControl.Instance.headerDis;
            }

        }

        CameraIsMoving = true;
        float time = (Mathf.Abs(targetPos.y - mainCamera.transform.position.y)) * cameraTransitionTime / ySize ;
        mainCamera.transform.DOMove(targetPos, time).SetEase(Ease.Linear).OnComplete(() =>
        {
            action.Invoke();
            CameraIsMoving = false;
        });
        
    }

    public void AdjustTilemap()
    {

        //float screenWidth = Camera.main.orthographicSize * 2.0f * Screen.width / Screen.height;
        //float screenHeight = Camera.main.orthographicSize * 2.0f;


        //float tileWidth = screenWidth / 11;
        //float tileHeight = screenHeight / 11;


        //float tileSize = Mathf.Min(tileWidth, tileHeight);

        //tilemap.layoutGrid.cellSize = new Vector3(tileSize, tileSize, 1);
        // cung cell pos x thi pos word hang le se lon hon
        float screenWidth =GameManager.Instance.CellSize.x * 11;
        Camera.main.orthographicSize = screenWidth / (2f * Screen.width / Screen.height);

    }


    //public void AdjustCamera()
    //{
    //    Tilemap tilemap = GameManager.Instance.GetTilemmap();
    //    Vector3 highestPoint = GameManager.Instance.GetHighestBubblePos();

    //    int highestCellIndex = tilemap.WorldToCell(highestPoint).y;
    //    int lowestCellIndex = tilemap.WorldToCell(GameManager.Instance.GetLowestBubblePos()).y;
    //    Vector3 centerPosWorld = _camera.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
    //    int cameraCenterCellIndex = tilemap.WorldToCell(centerPosWorld).y;

    //    if (highestCellIndex < tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y - 1)
    //    {
    //        Debug.Log("1");
    //        var differenceIndex = tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y - 1 - highestCellIndex;
    //        var newPos = tilemap.CellToWorld(new Vector3Int(0, cameraCenterCellIndex - differenceIndex, 0));
    //        var differenceHeight = newPos.y - _camera.transform.position.y;
    //        var adjustVector = new Vector3(0, differenceHeight, 0);

    //        _newCameraPosition += adjustVector;
    //        //_roof.position += adjustVector;
    //        _minBubbleHeightIndex -= differenceIndex;
    //        _cameraIsMoving = true;
    //    }

    //    else if (lowestCellIndex > _minBubbleHeightIndex)
    //    {
    //        Debug.Log("2");

    //        var differenceIndex = lowestCellIndex - _minBubbleHeightIndex;

    //        if (highestCellIndex - differenceIndex <= tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y + 1)
    //        {
    //            differenceIndex = highestCellIndex - tilemap.WorldToCell(new Vector3(0, _fieldVertExtent + _camera.transform.position.y, 0)).y + 1;
    //        }

    //        var newPos = tilemap.CellToWorld(new Vector3Int(0, cameraCenterCellIndex + differenceIndex, 0));
    //        var differenceHeight = newPos.y - _camera.transform.position.y;
    //        var adjustVector = new Vector3(0, differenceHeight, 0);

    //        _newCameraPosition += adjustVector;
    //        //_roof.position += adjustVector;
    //        _minBubbleHeightIndex += differenceIndex;
    //        _cameraIsMoving = true;
    //    }
    //    else if (lowestCellIndex < _minBubbleHeightIndex)
    //    {
    //        Debug.Log("3");

    //        var differenceIndex = _minBubbleHeightIndex - lowestCellIndex;
    //        var newPos = tilemap.CellToWorld(new Vector3Int(0, cameraCenterCellIndex - differenceIndex, 0));
    //        var differenceHeight = newPos.y - _camera.transform.position.y;
    //        var adjustVector = new Vector3(0, differenceHeight, 0);

    //        _newCameraPosition += adjustVector;
    //        //_roof.position += adjustVector;
    //        _minBubbleHeightIndex -= differenceIndex;
    //        _cameraIsMoving = true;
    //    }
    //}

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
