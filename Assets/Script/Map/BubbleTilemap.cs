
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BubbleTilemap : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public List<Bubble> bubbles = new List<Bubble>();
    
    public void LoadTilemap()
    {
        tilemap = GetComponent<Tilemap>();
    }
}
