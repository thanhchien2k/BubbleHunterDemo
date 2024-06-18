using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] MapHandle mapHandle;
    [SerializeField] private Transform _bubblesArea;
    public List<Bubble> bubblesTransform {  get; private set; }

    private void Start()
    {
        bubblesTransform = new();
    }

    public void SetAsBubbleAreaChild(Bubble bubble)
    {
        SnapToNearestGripPosition(bubble.transform);
        //bubbleTransform.SetParent(_bubblesArea);
        bubblesTransform.Add(bubble);
    }

    private void SnapToNearestGripPosition(Transform bubbleTransform)
    {
        Vector3Int cellPosition = mapHandle.GridParent.WorldToCell(bubbleTransform.position);
        bubbleTransform.position = mapHandle.GridParent.GetCellCenterWorld(cellPosition);
    }
}
