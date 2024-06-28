using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
public class DataConfig
{

}

[Serializable]
public enum BubbleType
{
    Normal = 0,
    Gem = 1,
    Spark = 2,
    Hole = 3,
    Jar = 4,
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
    public Sprite Sprite;
}

[Serializable]
public struct SpriteConfig
{
    public string Name;
    public Sprite Sprite;
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
    public string SpriteName;
    public TileInfo(TileBase tile, Vector3Int position, string spriteName)
    {
        this.TileBase = tile;
        this.Position = position;
        this.SpriteName = spriteName;
    }

    public TileInfo(TileBase tile, Vector3Int position)
    {
        this.TileBase = tile;
        this.Position = position;
    }

}

[Serializable]
public struct LevelInfo
{
    public Tilemap Tilemap;
    public LevelType LevelType;
    public int Point;
    public int GemCount;
    public int shootCount;
}

[SerializeField]
public enum LevelType
{
    Popup,
    CollectGem,
}