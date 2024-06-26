using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] Transform grid;
    [SerializeField] Shooter shooter;
    [SerializeField] CameraController cameraController;

    Tilemap tilemap;
    int _minSequenceSize = 3;
    public int ShotsLeft { get; private set; } = 20;
    public int BubblesLeft { get { return bubblesInScene.Count; } }
    public float _neighborDetectionRange;

    private Vector3 highestPos;
    private Vector3 newPos;
    
    private List<Bubble> bubblesInScene  = new List<Bubble>();
    private List<Bubble> bubbleSequence;
    private List<Bubble> bubbleAnim;
    public List<BubbleColor> colorsInScene { get; private set; }
    public bool IsCanPlay { get; set; } = false;
    public bool IsEndGame { get; set; } = false;
    public Vector2 cellSize { get; set; }

    protected override void Awake()
    {
        base.Awake();   

        bubbleSequence = new List<Bubble>();
        bubbleAnim = new List<Bubble>();
        tilemap = Instantiate(GameConfig.LevelMap[0], grid);
        _neighborDetectionRange = GameConfig.Bubble.circleCollider.radius * 1.2f;
        cellSize = tilemap.cellSize;
        if (cameraController == null) cameraController = CameraController.Instance;
        AddBubleToList();
        UpdateScene();
    }

    public void AddBubleToList()
    {
        Bubble[] bubbles = tilemap.transform.GetComponentsInChildren<Bubble>();
        bubblesInScene = bubbles.ToList();

        highestPos = GetHighestBubblePos();
    }
    public void AddBubble(Bubble currentBubble)
    {
        bubblesInScene.Add(currentBubble);
        currentBubble.transform.SetParent(tilemap.transform);
    }

    #region Process Function
    public void ProcessTurn(Bubble currentBubble)
    {
        bubbleSequence.Clear();
        newPos = currentBubble.transform.position;

        StratAnimHited(currentBubble);
        CheckBubbleSequence(currentBubble);

        if (bubbleSequence.Count >= _minSequenceSize)
        {
            DestroyBubblesInSequence();
            DropDisconectedBubbles();
        }

        StartCoroutine(ProcessBubbles());
    }

    private void CheckBubbleSequence(Bubble bubble)
    {
        bubbleSequence.Add(bubble);

        List<Bubble> neighbors = bubble.GetNeighbors();


        foreach (var neighbor in neighbors)
        {
            if (!bubbleSequence.Contains(neighbor))
            {
                if (neighbor.color == bubble.color)
                {
                    //Debug.Log(bubble.color + "  " + neighbor.color);
                    CheckBubbleSequence(neighbor);
                }
            }
        }
    }

    private void DestroyBubblesInSequence()
    {
        foreach (Bubble bubble in bubbleSequence)
        {
            if (bubble.gameObject != null) RecycleBubble(bubble);
        }
    }

    private void DropDisconectedBubbles()
    {
        SetAllBubblesConnectionToFalse();
        SetConnectedBubblesToTrue();
        SetGravityToDisconectedBubbles();
    }

    private void SetAllBubblesConnectionToFalse()
    {
        foreach (Bubble bubble in bubblesInScene)
        {
            bubble.IsConnected = false;
        }
    }

    private void SetConnectedBubblesToTrue()
    {
        bubbleSequence.Clear();

        RaycastHit2D[] hits = Physics2D.RaycastAll(highestPos, Vector3.right);

        for (int i = 0; i < hits.Length; i++)
        {
            Bubble bubble = hits[i].transform.gameObject.GetComponent<Bubble>();
            if (bubble != null)
                SetNeighboursConnectionToTrue(bubble);
        }
    }

    private void StratAnimHited(Bubble bubble)
    {
        bubbleAnim.Clear();
        PlayNeighboursAnimHited(bubble);
        bubbleAnim.Clear();
    }

    private void PlayNeighboursAnimHited(Bubble bubble)
    {
        bubbleAnim.Add(bubble);

        foreach (Bubble bubbleNeighbor in bubble.GetNeighbors())
        {
            if (!bubbleAnim.Contains(bubbleNeighbor))
            {
                bubbleNeighbor.PlayHitAnim(newPos);
                PlayNeighboursAnimHited(bubbleNeighbor);
            }
        }

    }

    private void SetNeighboursConnectionToTrue(Bubble bubble)
    {
        bubble.IsConnected = true;
        bubbleSequence.Add(bubble);

        foreach (Bubble bubbleNeighbor in bubble.GetNeighbors())
        {
            if (!bubbleSequence.Contains(bubbleNeighbor))
            {
                SetNeighboursConnectionToTrue(bubbleNeighbor);
            }
        }
    }

    private void SetGravityToDisconectedBubbles()
    {
        var bufferToRemove = new List<Bubble>();
        foreach (Bubble bubble in bubblesInScene)
        {
            if(bubble == null)
            {
                Debug.Log("bubble null");
                return;
            }
            if (!bubble.IsConnected)
            {
               bufferToRemove.Add(bubble);
               bubble.circleCollider.enabled = false;
               bubble.SetUpRigibody();
            }
        }

        foreach (Bubble bubble in bufferToRemove)
        {
            RemoveBubble(bubble);
        }
    }
    #endregion


    public Vector3 GetHighestBubblePos()
    {
        var lastBubbleIndexVert = tilemap.WorldToCell(bubblesInScene[0].transform.position);
        Vector3Int maxBubbleIndexVert;

        for (int i = 0; i < bubblesInScene.Count; i++)
        {
            var bubble = bubblesInScene[i];
            maxBubbleIndexVert = tilemap.WorldToCell(bubble.transform.position);
            if (maxBubbleIndexVert.y > lastBubbleIndexVert.y) lastBubbleIndexVert = maxBubbleIndexVert;
        }

        return tilemap.CellToWorld(lastBubbleIndexVert);
    }

    public Vector3 GetLowestBubblePos()
    {
        var lastBubbleIndexVert = tilemap.WorldToCell(bubblesInScene[0].transform.position);
        Vector3Int maxBubbleIndexVert;

        for (int i = 0; i < bubblesInScene.Count; i++)
        {
            var bubble = bubblesInScene[i];
            maxBubbleIndexVert = tilemap.WorldToCell(bubble.transform.position);
            if (maxBubbleIndexVert.y < lastBubbleIndexVert.y) lastBubbleIndexVert = maxBubbleIndexVert;
        }

        return tilemap.CellToWorld(lastBubbleIndexVert);
    }



    public void RemoveBubble(Bubble bubble)
    {
        if (bubblesInScene.Contains(bubble))
        {
            bubble.DestroyFallingBubble();
            bubblesInScene.Remove(bubble);
        }
        if (bubblesInScene.Count <= 0)
        {
            OnEndGame();
        }
    }

    public void RecycleBubble(Bubble bubble)
    {
        if (bubbleSequence.Contains(bubble))
        {
            bubblesInScene.Remove(bubble);
            bubble.OnRecycle();
        }
        else if (bubble.gameObject != null)
        {
            Debug.Log("no contain");
            bubble.OnRecycle();
        }
    }

    public void SnapToNearestGripPosition(Bubble bubble)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(bubble.transform.position);
        bubble.transform.position = tilemap.GetCellCenterWorld(cellPosition);
        AddBubble(bubble);
    }

    //public Vector3 GetTargetPos(Bubble bubble, Bubble moveBubble)
    //{
    //    List<Vector3> listNearPos = new();
    //    Vector3 setBubblePos  = bubble.transform.position;
    //    Vector3 nearPos;
    //    Vector3Int cellPosition = tilemap.WorldToCell(setBubblePos);
    //    Vector3Int cellPos;
    //    for (int i = -1; i < 2; i++)
    //    {
    //        for (int j = -1; j < 2; j++)
    //        {
    //            cellPos = cellPosition + new Vector3Int(i, j, 0);
    //            nearPos = tilemap.CellToWorld(cellPos);
    //            var hits = Physics2D.OverlapCircle(nearPos, _neighborDetectionRange / 4);
    //            if (hits != null && hits.gameObject != moveBubble.gameObject)
    //            {
    //                Debug.Log("near" + gameObject);
    //                break;
    //            }
    //            listNearPos.Add(tilemap.GetCellCenterWorld(cellPos));
    //        }
    //    }

    //    if (listNearPos.Count <= 0)
    //    {
    //        Debug.Log("< 0");
    //        return Vector3.zero;
    //    }
    //    else
    //    {
    //        Debug.Log(listNearPos.Count);
    //    }

    //    var Pos = listNearPos[0];
    //    var lastDistance = Vector2.Distance(moveBubble.transform.position, listNearPos[0]);

    //    for (int i = 1; i< listNearPos.Count; i++)
    //    {
    //        var tempDis = Vector2.Distance(moveBubble.transform.position, listNearPos[i]);
    //        if(tempDis < lastDistance)
    //        {
    //            lastDistance = tempDis;
    //            Pos = listNearPos[i];
    //        }
    //    }

    //    return Pos;
    //}

    public void GetTargetTransform(Vector3 point)
    {

    }

    private IEnumerator ProcessBubbles()
    {
        yield return new WaitForEndOfFrame();
        //bubbleSequence.Clear();

        UpdateScene();

    }

    public void UpdateScene()
    {
        if (BubblesLeft <= 0) return;
        List<BubbleColor> colors = new List<BubbleColor>();

        foreach (Bubble bubble in bubblesInScene)
        {
            if (colors.Count < GameConfig.BubbleInfos.Count && !colors.Contains(bubble.color))
            {
                colors.Add(bubble.color);

            }
        }
        colorsInScene = colors;

        MoveCamera();
    }

    public void MoveCamera()
    {
        cameraController.StartMoveCamera(highestPos, GetLowestBubblePos(), cellSize.y, OnEndStartMoveCamera);
    }

    public void OnEndGame()
    {
        IsEndGame = true;
    }

    public void OnEndStartMoveCamera()
    {
        shooter.OnBubbleCollided();
    }

}
