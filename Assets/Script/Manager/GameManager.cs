using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] Transform grid;
    [SerializeField] Shooter shooter;

    Tilemap tilemap;
    [SerializeField] CameraController cameraController;
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

    private void Start()
    {
        bubbleSequence = new List<Bubble>();
        bubbleAnim = new List<Bubble>();

        tilemap = Instantiate(GameConfig.LevelMap[0], grid);
        _neighborDetectionRange = GameConfig.Bubble.circleCollider.radius * 1.3f;
        AddBubleToList();
        UpdateScene();
        MoveCamera();
        
    }
    public void MoveCamera()
    {
        IsCanPlay = false;
        Vector2 cellSize = tilemap.cellSize;
        Vector2 targetPos = new Vector2(0, GetLowestBubblePos().y);
        cameraController.StartMoveCamera(targetPos, 1.5f, OnEndStartMoveCamera);
    }

    public void AddBubleToList()
    {
        Bubble[] bubbles = tilemap.transform.GetComponentsInChildren<Bubble>();
        bubblesInScene = bubbles.ToList();

        highestPos = GetHighestBubblePos();
    }

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

    public void AddBubble(Bubble currentBubble)
    {
        bubblesInScene.Add(currentBubble);
        currentBubble.transform.SetParent(tilemap.transform);
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

    public Tilemap GetTilemmap()
    {
        return tilemap;
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

    public void RemoveBubble(Bubble bubble)
    {
        if (bubblesInScene.Contains(bubble))
        {
            bubble.DestroyFallingBubble();
            bubblesInScene.Remove(bubble);
        }
        if (bubblesInScene.Count <= 0)
        {
            //OnGameEnd();
            Debug.Log("end");
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

    private IEnumerator ProcessBubbles()
    {
        yield return new WaitForEndOfFrame();
        //bubbleSequence.Clear();
        UpdateScene();
        if (BubblesLeft <= 0) Debug.Log("end");
        shooter.OnBubbleCollided();
    }

    public void UpdateScene()
    {
        List<BubbleColor> colors = new List<BubbleColor>();

        foreach (Bubble bubble in bubblesInScene)
        {
            if (colors.Count < GameConfig.BubbleInfos.Count && !colors.Contains(bubble.color))
            {
                colors.Add(bubble.color);

            }
        }
        colorsInScene = colors;

    }

    public void OnEndGame()
    {
        // end game
    }

    public void OnEndStartMoveCamera()
    {
        shooter.SpawnBubble();
        IsCanPlay = true;
    }

}
