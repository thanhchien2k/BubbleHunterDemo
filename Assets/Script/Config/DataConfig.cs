using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using JetBrains.Annotations;
public class DataConfig
{

}

[Serializable]
public enum BubbleType
{
    Normal = 0,

}


[Serializable]
public enum BubbleColor
{
    Red = 0,
    Green = 1,
    Blue = 2,
    Yellow = 3,
}

[Serializable]
public class BubbleInfo
{
    public BubbleType Type;
    public BubbleColor Color;

    public BubbleInfo(BubbleType type, BubbleColor color)
    {
        this.Type = type;
        this.Color = color;
    }
}

[Serializable]
public class TilemapData
{
    public string Key;
    public List<TileInfo> Tiles = new();
}

[Serializable]
public class TileInfo
{
    public TileBase TileBase;
    public Vector3Int Position;
    public BubbleInfo BubbleInfo;
    public TileInfo(TileBase tile, Vector3Int position, BubbleInfo bubbleInfo)
    {
        this.TileBase = tile;
        this.Position = position;
        this.BubbleInfo = bubbleInfo;
    }

    public TileInfo(TileBase tile, Vector3Int position)
    {
        this.TileBase = tile;
        this.Position = position;
    }
}
