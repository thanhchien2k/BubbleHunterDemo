using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] MapHandle mapHandle;
    [SerializeField] private Transform _bubblesArea;
    [SerializeField] int _minSequenceSize = 2;
    [SerializeField] Tilemap tilemap;
    public float _neighborDetectionRange;
    
    List<Bubble> Bubbles { get; set; } = new List<Bubble>();
    public List<Bubble> bubbleSequence;

    private void Start()
    {
        _neighborDetectionRange = GameConfig.Bubble.circleCollider.radius * 1.2f;
        AddBubleToList();
    }

    public void AddBubleToList()
    {
        Bubble[] bubbles = GetComponentsInChildren<Bubble>();
        Bubbles = bubbles.ToList();
    }

    public void ProcessTurn(Bubble currentBubble)
    {
        bubbleSequence.Clear();
        CheckBubbleSequence(currentBubble);
        if (bubbleSequence.Count >= _minSequenceSize)
        {
            DestroyBubblesInSequence();
            DropDisconectedBubbles();
        }

        //StartCoroutine(ProcessBubbles());
        Shooter.Instance.SpawnBubble();
    }

    //public void SetAsBubbleAreaChild(Bubble bubble)
    //{
    //    SnapToNearestGripPosition(bubble.transform);
    //    //bubbleTransform.SetParent(_bubblesArea);
    //    bubblesTransform.Add(bubble);
    //}

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
        //SetConnectedBubblesToTrue();
        //SetGravityToDisconectedBubbles();
    }

    private void SetAllBubblesConnectionToFalse()
    {
        foreach (Bubble bubble in Bubbles)
        {
            bubble.IsConnected = false;
        }
    }

    //private void SetConnectedBubblesToTrue()
    //{
    //    bubbleSequence.Clear();

    //    RaycastHit2D[] hits = Physics2D.RaycastAll(GameLevelManager.instance.HighestPos, Vector3.right);

    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        if (hits[i].transform.gameObject.GetComponent<Bubble>() != null)
    //            SetNeighboursConnectionToTrue(hits[i].transform);
    //    }
    //}

    //private void SetNeighboursConnectionToTrue(Transform bubbleTransform)
    //{
    //    Bubble bubble = bubbleTransform.GetComponent<Bubble>();
    //    bubble.IsConnected = true;
    //    _bubbleSequence.Add(bubbleTransform);

    //    foreach (Transform bubbleNeighborTransform in bubble.GetNeighbors())
    //    {
    //        if (!_bubbleSequence.Contains(bubbleNeighborTransform))
    //        {
    //            SetNeighboursConnectionToTrue(bubbleNeighborTransform);
    //        }
    //    }
    //}

    //private void SetGravityToDisconectedBubbles()
    //{
    //    var bufferToRemove = new List<Transform>();
    //    foreach (Transform bubbleTransform in GameLevelManager.instance.BubblesTransform)
    //    {
    //        if (!bubbleTransform.GetComponent<Bubble>().IsConnected)
    //        {
    //            bufferToRemove.Add(bubbleTransform);
    //            bubbleTransform.gameObject.GetComponent<CircleCollider2D>().enabled = false;
    //            if (!bubbleTransform.GetComponent<Rigidbody2D>())
    //            {
    //                Rigidbody2D rb2d = bubbleTransform.gameObject.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
    //            }
    //        }
    //    }
    //    foreach (Transform bubbleTransform in bufferToRemove)
    //    {
    //        GameLevelManager.instance.RemoveBubbleTransform(bubbleTransform);
    //    }
    //}

    public void RecycleBubble(Bubble bubble)
    {
        if (bubbleSequence.Contains(bubble))
        {
            Destroy(bubble.gameObject);
            //RemoveBubbleTransform(bubbleTransform);
            //bubbleTransform.gameObject.GetComponent<Bubble>().OnRecycle();
        }
        //else if (bubbleTransform.gameObject != null)
        //{
        //    bubbleTransform.gameObject.GetComponent<Bubble>().OnRecycle();
        //}
    }

    private void SnapToNearestGripPosition(Transform bubbleTransform)
    {
        Vector3Int cellPosition = mapHandle.GridParent.WorldToCell(bubbleTransform.position);
        bubbleTransform.position = mapHandle.GridParent.GetCellCenterWorld(cellPosition);
    }
}
