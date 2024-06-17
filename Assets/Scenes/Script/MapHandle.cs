using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapHandle : MonoBehaviour
{
    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    [SerializeField] BoundsInt bounds;
    [SerializeField] Tilemap[] maps;
    [SerializeField] string fileName = "tilemapsConfig";

    private void Start()
    {
        InitTitlemap();
    }

    private void InitTitlemap()
    {
        foreach (var map in maps)
        {
            tilemaps.Add(map.name, map);
        }
    }

    public void OnSave()
    {
        List<TilemapData> datas = new List<TilemapData>();
        foreach (var mapObject in tilemaps) 
        {
            TilemapData curData = new TilemapData();
            curData.Key = mapObject.Key;

            BoundsInt boundsInt = mapObject.Value.cellBounds;

            for (int x = 0; x < boundsInt.xMin; x++) 
            {
                for (int y = 0; y < boundsInt.yMin; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tileBase = mapObject.Value.GetTile(pos);

                    if (tileBase != null)
                    {
                        TileInfo infos = new TileInfo(tileBase, pos);
                        curData.TilemapInfos.Add(infos);
                    }
                }
            }

            datas.Add(curData);
        }
    }

    public void OnLoad()
    {

    }
}

[Serializable]
public class TilemapData
{
    public string Key;
    public List<TileInfo> TilemapInfos = new();
}

[Serializable]
public class TileInfo
{
    public TileBase TileBase;
    public Vector3 Position;

    public TileInfo(TileBase tile, Vector3 position)
    {
        this.TileBase = tile;
        this.Position = position;
    }
}
