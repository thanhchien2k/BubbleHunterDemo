using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] Transform grid;

    private Tilemap tilemap;
    private int minSequenceSize = 3;
    private Vector3 highestPos;
    private Vector3 newPos;
    private List<Bubble> bubblesInScene;
    private List<Bubble> bubbleSequence;
    private List<Bubble> bubbleAnim;
    int levelID;

    public LevelInfo levelInfo { get; private set; }

    public int BubblesLeft { get { return bubblesInScene.Count; } }
    public float NeighborDetectionRange { get; private set; } 
    public List<BubbleColor> colorsInScene { get; private set; }
    public bool IsCanPlay { get; set; } = false;
    public bool IsEndGame { get; set; } = false;
    public Vector2 CellSize { get; set; }

    protected override void Awake()
    {
        base.Awake();
        bubbleSequence = new List<Bubble>();
        bubbleAnim = new List<Bubble>();
        bubblesInScene = new List<Bubble>();
        levelID = PlayerPrefs.GetInt("LevelID", 0);
        levelInfo = LevelConfig.LevelInfos[levelID];

        tilemap = Instantiate(levelInfo.Tilemap, grid);
        NeighborDetectionRange = GameConfig.Bubble.circleCollider.radius * 1.5f;
        CellSize = tilemap.cellSize;
        AddBubleToList();
    }

    private void Start()
    {
        UpdateScene();
    }

    public void AddBubleToList()
    {
        Bubble[] bubbles = tilemap.transform.GetComponentsInChildren<Bubble>();

        bubblesInScene = bubbles.OrderBy(b => b.transform.position.y).ToList();

        highestPos = GetHighestBubblePos();
    }
    public void AddBubble(Bubble currentBubble)
    {
        int index = bubblesInScene.BinarySearch(currentBubble, Comparer<Bubble>.Create((a, b) => a.transform.position.y.CompareTo(b.transform.position.y)));

        if (index < 0)
        {
            index = ~index;
        }

        bubblesInScene.Insert(index, currentBubble);
        currentBubble.transform.SetParent(tilemap.transform);
    }

    #region Process Function
    public void ProcessTurn(Bubble currentBubble)
    {
        bubbleSequence.Clear();
        newPos = currentBubble.transform.position;

        StratAnimHited(currentBubble);
        CheckBubbleSequence(currentBubble);

        if (bubbleSequence.Count >= minSequenceSize)
        {
            DestroyBubblesInSequence();
            //DropDisconectedBubbles();
        }
        else
        {
            StartCoroutine(ProcessBubbles());
        }

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
                    CheckBubbleSequence(neighbor);
                }
            }
        }
    }

    private void DestroyBubblesInSequence()
    {
        //foreach (Bubble bubble in bubbleSequence)
        //{
        //    if (bubble.gameObject != null) RecycleBubble(bubble);
        //}
        ReSortBubblesSequece();
        StartCoroutine(DestroyListBubble());
    }

    private void ReSortBubblesSequece()
    {
        //int count = bubbleSequence.Count;
        //Bubble temp;

        //for (int i = 0; i < count; i++)
        //{
        //    for (int j = i; j < count; j++)
        //    {
        //        if (tilemap.WorldToCell(bubbleSequence[j].transform.position).y < tilemap.WorldToCell(bubbleSequence[i].transform.position).y)
        //        {
        //            temp = bubbleSequence[j];
        //            bubbleSequence[j] = bubbleSequence[i];
        //            bubbleSequence[i] = temp;
        //        }
        //    }
        //}
        bubbleSequence = bubbleSequence.OrderBy(b => b.transform.position.y).ToList();
    }

    IEnumerator DestroyListBubble()
    {
        if(bubbleSequence.Count == 0)
        {
            yield return new WaitForEndOfFrame();
            DropDisconectedBubbles();
            StartCoroutine(ProcessBubbles());
            yield break;
        }

        RecycleBubble(bubbleSequence.First());
        yield return new WaitForSeconds(0.02f);
        StartCoroutine(DestroyListBubble());
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

        RaycastHit2D[] hits = Physics2D.RaycastAll(highestPos, Vector3.left);

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
        //var lastBubbleIndexVert = tilemap.WorldToCell(bubblesInScene[0].transform.position);
        //Vector3Int maxBubbleIndexVert;

        //for (int i = 0; i < bubblesInScene.Count; i++)
        //{
        //    var bubble = bubblesInScene[i];
        //    maxBubbleIndexVert = tilemap.WorldToCell(bubble.transform.position);
        //    if (maxBubbleIndexVert.y > lastBubbleIndexVert.y) lastBubbleIndexVert = maxBubbleIndexVert;
        //}

        //return tilemap.CellToWorld(lastBubbleIndexVert);

        var temp = tilemap.WorldToCell(bubblesInScene.Last().transform.position);
        return tilemap.CellToWorld(temp);
    }

    public Vector3 GetLowestBubblePos()
    {
        //var lastBubbleIndexVert = tilemap.WorldToCell(bubblesInScene[0].transform.position);
        //Vector3Int maxBubbleIndexVert;

        //for (int i = 0; i < bubblesInScene.Count; i++)
        //{
        //    var bubble = bubblesInScene[i];
        //    maxBubbleIndexVert = tilemap.WorldToCell(bubble.transform.position);
        //    if (maxBubbleIndexVert.y < lastBubbleIndexVert.y) lastBubbleIndexVert = maxBubbleIndexVert;
        //}

        //return tilemap.CellToWorld(lastBubbleIndexVert);

        var temp = tilemap.WorldToCell(bubblesInScene.First().transform.position);
        return tilemap.CellToWorld(temp);
    }



    public void RemoveBubble(Bubble bubble)
    {
        if (bubblesInScene.Contains(bubble))
        {
            bubble.CheckFallingBubble();
            bubblesInScene.Remove(bubble);
        }
    }

    public void RecycleBubble(Bubble bubble)
    {
        if (bubbleSequence.Contains(bubble))
        {
            bubble.OnRecycle();
            bubbleSequence.Remove(bubble);
            bubblesInScene.Remove(bubble);
        }
        else if (bubble.gameObject != null)
        {
            bubble.OnRecycle();
        }
    }

    public void SnapToNearestGripPosition(Bubble bubble)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(bubble.transform.position);

        //if (Mathf.Abs(cellPosition.y % 2) == 1)
        //{
        //    Collider2D hit;

        //    if (cellPosition.x == -6)
        //    {
        //        cellPosition -= new Vector3Int(-1, 0, 0);
        //        hit = Physics2D.OverlapCircle(tilemap.GetCellCenterWorld(cellPosition), 0.1f);
        //        if(hit != null && hit.gameObject != bubble.gameObject)
        //        {
        //            cellPosition -= Vector3Int.up;
        //        }
        //    }
        //    else if(cellPosition.x == 5)
        //    {
        //        cellPosition -= new Vector3Int(1, 0, 0);

        //        hit = Physics2D.OverlapCircle(tilemap.GetCellCenterWorld(cellPosition), 0.1f);

        //        if (hit != null && hit.gameObject != bubble.gameObject) 
        //        {
        //            cellPosition -= new Vector3Int(-1, 1, 0);
        //        }
        //    }
        //}

        bubble.transform.position = tilemap.GetCellCenterWorld(cellPosition);

        AddBubble(bubble);
    }

    public Vector2 FixPoint(Vector2 point)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(point);

        if (Mathf.Abs(cellPosition.y % 2) == 1)
        {
            Collider2D hit;

            if (cellPosition.x == -6)
            {
                cellPosition -= new Vector3Int(-1, 0, 0);
                hit = Physics2D.OverlapCircle(tilemap.GetCellCenterWorld(cellPosition), 0.1f);
                if (hit != null)
                {
                    cellPosition -= Vector3Int.up;
                }
            }
            else if (cellPosition.x == 5)
            {
                cellPosition -= new Vector3Int(1, 0, 0);

                hit = Physics2D.OverlapCircle(tilemap.GetCellCenterWorld(cellPosition), 0.1f);

                if (hit != null)
                {
                    cellPosition -= new Vector3Int(-1, 1, 0);
                }
            }
        }

        return tilemap.GetCellCenterWorld(cellPosition);

    }

    private IEnumerator ProcessBubbles()
    {
        yield return new WaitForEndOfFrame();
       
        if(BubblesLeft <= 0)
        {
            OnEndGame();
        }
        else
        {
            GamePlayCanvasControl.Instance.CheckGemCount();
            UpdateScene();
        }

    }

    public void UpdateScene()
    {
        if (BubblesLeft <= 0 || IsEndGame) return;
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
        CameraController.Instance.StartMoveCamera(highestPos, GetLowestBubblePos(), CellSize.y, OnEndStartMoveCamera);
    }

    public void OnEndGame()
    {
        IsEndGame = true;
        GamePlayCanvasControl.Instance.ActiveWinPopup();
    }

    public void OnEndStartMoveCamera()
    {
        Shooter.Instance.OnBubbleCollided();
    }

    public bool IsableReflect(Vector3 pos)
    {
        var temp = tilemap.WorldToCell(pos);

        if (Mathf.Abs(temp.y % 2) == 1 && (temp.x == -6 || temp.x == 5) && tilemap.GetCellCenterWorld(temp).y >= GetLowestBubblePos().y)
        {
            Debug.Log(tilemap.GetCellCenterWorld(temp).y + "***" + GetLowestBubblePos().y);
            return false;
        }
        return true;
    }
}
