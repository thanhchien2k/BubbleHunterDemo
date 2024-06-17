using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using EditorAttributes;
using UnityEditor.U2D.Aseprite;

public class MapHandle : MonoBehaviour
{
    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    [SerializeField] BoundsInt bounds;
    [SerializeField] Tilemap[] maps;
    [SerializeField] string fileName = "tilemapsConfig";
    [SerializeField] GameObject prefabs;

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
    [Button("Save Map")]
    public void OnSave()
    {
        List<TilemapData> datas = new List<TilemapData>();

        foreach (var mapObject in tilemaps)
        {
            TilemapData curData = GetTileMapData(mapObject.Value);
            curData.Key = mapObject.Key;
            datas.Add(curData);
        }

        DataManager.SaveToJSON<TilemapData>(datas, fileName);

    }
    [Button("Load Map")]

    public void OnLoad()
    {
        List<TilemapData> data = DataManager.ReadListFromJSON<TilemapData>(fileName);

        foreach (var mapData in data)
        {
            // if key does NOT exist in dictionary skip it
            if (!tilemaps.ContainsKey(mapData.Key))
            {
                Debug.LogError("Found saved data for tilemap called '" + mapData.Key + "', but Tilemap does not exist in scene.");
                continue;
            }

            // get according map
            var map = tilemaps[mapData.Key];

            // clear map
            map.ClearAllTiles();

            if (mapData.Tiles != null && mapData.Tiles.Count > 0)
            {
                foreach (var tile in mapData.Tiles)
                {

                    //if (guidToTileBase.ContainsKey(tile.guidForBuildable))
                    //{
                    //    map.SetTile(tile.position, guidToTileBase[tile.guidForBuildable]);
                    //}
                    //else
                    //{
                    //    Debug.LogError("Refernce " + tile.guidForBuildable + " could not be found.");
                    //}
                    //map.SetTile(tile.Position, tile.TileBase);
                    Vector3 worldPosition = map.CellToWorld(tile.Position);
                    GameObject instantiatedObject = Instantiate(prefabs, worldPosition, Quaternion.identity);
                    instantiatedObject.transform.SetParent(maps[0].transform);
                }
            }
        }
    }

    public TilemapData GetTileMapData(Tilemap tile)
    {
        TilemapData curData = new TilemapData();

        BoundsInt boundsInt = tile.cellBounds;

        Debug.Log(boundsInt.xMin + " " + boundsInt.xMax + " " + boundsInt.yMin + " " + boundsInt.yMax);
        for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
        {
            for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tileBase = tile.GetTile(pos);

                if (tileBase != null)
                {
                    Debug.Log("!= null");
                    TileInfo infos = new TileInfo(tileBase, pos);
                    curData.Tiles.Add(infos);
                }
            }
        }

        return curData;
    }

    public void LoadTilemap(Tilemap tilemap)
    {
        TilemapData data = GetTileMapData(tilemap);

        if (data.Tiles != null && data.Tiles.Count > 0)
        {
            foreach (var tile in data.Tiles)
            {
                Vector3 worldPosition = tilemap.CellToWorld(tile.Position);
                GameObject instantiatedObject = Instantiate(prefabs, worldPosition, Quaternion.identity);
                instantiatedObject.transform.SetParent(maps[0].transform);
            }
        }
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

    public TileInfo(TileBase tile, Vector3Int position)
    {
        this.TileBase = tile;
        this.Position = position;
    }
}
