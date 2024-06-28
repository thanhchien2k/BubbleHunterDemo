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
    private int shotsLeft;
    private Vector3 highestPos;
    private Vector3 newPos;
    private List<Bubble> bubblesInScene;
    private List<Bubble> bubbleSequence;
    private List<Bubble> bubbleAnim;
    int levelID;

    public LevelInfo levelInfo { get; private set; }
    public int ShotsLeft { 
        get { return shotsLeft; }
        
        set 
        { 
            shotsLeft = value;
        }
    }
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
        ShotsLeft = levelInfo.shootCount;
        NeighborDetectionRange = GameConfig.Bubble.circleCollider.radius * 1.5f;
        CellSize = tilemap.cellSize;
        AddBubleToList();
    }

    private void Start()
    {
        UpdateScene();
        AdjustTilemap();
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

        if (bubbleSequence.Count >= minSequenceSize)
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
            bubble.CheckFallingBubble();
            bubblesInScene.Remove(bubble);
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
            bubble.OnRecycle();
        }
    }

    public void SnapToNearestGripPosition(Bubble bubble)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(bubble.transform.position);
        bubble.transform.position = tilemap.GetCellCenterWorld(cellPosition);
        AddBubble(bubble);
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

    void AdjustTilemap()
    {

        //float screenWidth = Camera.main.orthographicSize * 2.0f * Screen.width / Screen.height;
        //float screenHeight = Camera.main.orthographicSize * 2.0f;


        //float tileWidth = screenWidth / 11;
        //float tileHeight = screenHeight / 11;


        //float tileSize = Mathf.Min(tileWidth, tileHeight);

        //tilemap.layoutGrid.cellSize = new Vector3(tileSize, tileSize, 1);

        float screenWidth = CellSize.x * 11;
        Camera.main.orthographicSize = screenWidth / ( 2f * Screen.width / Screen.height);

    }

}
