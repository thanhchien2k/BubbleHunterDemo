using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using EditorAttributes;

public class MapHandle : MonoBehaviour
{
    //Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    //[SerializeField] Tilemap[] maps;
    //[SerializeField] string fileName = "tilemapsConfig";
    [SerializeField] GameObject prefabs;
    [SerializeField] Tilemap SellectTile;
    public Grid GridParent;
    Dictionary<string, BubbleInfo> infosDictionary = new Dictionary<string, BubbleInfo>();

    private void Start()
    {
        //InitTitlemap();
        SetUpDictionary();
    }
    public void SetUpDictionary()
    {
        List<BubbleInfo> temp = GameConfig.BubbleInfos;
        for (int i = 0; i < temp.Count; i++) 
        {
            infosDictionary.Add(temp[i].Sprite.name, temp[i]);
        }
    }

    //private void InitTitlemap()
    //{
    //    foreach (var map in maps)
    //    {
    //        tilemaps.Add(map.name, map);
    //    }
    //}
    //[Button("Save Map")]

    //public void OnSave()
    //{
    //    List<TilemapData> datas = new List<TilemapData>();

    //    foreach (var mapObject in tilemaps)
    //    {
    //        TilemapData curData = GetTileMapData(mapObject.Value);
    //        curData.Key = mapObject.Key;
    //        datas.Add(curData);
    //    }

    //    DataManager.SaveToJSON<TilemapData>(datas, fileName);

    //}


    //[Button("Load Map")]

    //public void OnLoad()
    //{
    //    List<TilemapData> data = DataManager.ReadListFromJSON<TilemapData>(fileName);

    //    foreach (var mapData in data)
    //    {
    //        // if key does NOT exist in dictionary skip it
    //        if (!tilemaps.ContainsKey(mapData.Key))
    //        {
    //            Debug.LogError("Found saved data for tilemap called '" + mapData.Key + "', but Tilemap does not exist in scene.");
    //            continue;
    //        }

    //        // get according map
    //        var map = tilemaps[mapData.Key];

    //        // clear map
    //        map.ClearAllTiles();

    //        if (mapData.Tiles != null && mapData.Tiles.Count > 0)
    //        {
    //            foreach (var tile in mapData.Tiles)
    //            {
    //                Vector3 worldPosition = map.CellToWorld(tile.Position);
    //                Bubble bubble = Instantiate(GameConfig.Bubble, worldPosition, Quaternion.identity);
    //                bubble.transform.SetParent(maps[0].transform);
    //                bubble.EnabbleCircleCollider();
    //            }
    //        }
    //    }
    //}

    public TilemapData GetTileMapData(Tilemap tilemap)
    {
        TilemapData curData = new TilemapData();

        BoundsInt boundsInt = tilemap.cellBounds;

        for (int x = boundsInt.xMin; x < boundsInt.xMax; x++)
        {
            for (int y = boundsInt.yMin; y < boundsInt.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tileBase = tilemap.GetTile(pos);

                if (tileBase != null)
                {   
                   
                    TileInfo infos = new TileInfo(tileBase, pos, infosDictionary[((Tile)tileBase).sprite.name]);
                    curData.Tiles.Add(infos);
                }
            }
        }

        return curData;

    }

    public void LoadTilemap(Tilemap map)
    {
        if (map == null) return;

        map = Instantiate(map, GridParent.transform);
        TilemapData data = GetTileMapData(map);
        SaveDataToFile(data);

        map.ClearAllTiles();

        if (data.Tiles != null && data.Tiles.Count > 0)
        {
            foreach (var tile in data.Tiles)
            {
                Vector3 worldPosition = map.CellToWorld(tile.Position);
                Bubble bubble = Instantiate(GameConfig.Bubble, worldPosition, Quaternion.identity);
                bubble.transform.SetParent(map.transform);
                bubble.SetupBubble(tile.BubbleInfo);
            }
        }
    }

    public void SaveDataToFile(TilemapData data)
    {
        List<TilemapData> datas = new List<TilemapData>();

   
            data.Key = "1";
            datas.Add(data);
        Debug.Log(data.Tiles[0].BubbleInfo.Color);

        DataManager.SaveToJSON<TilemapData>(datas, SellectTile.name);

    }

    public void LoadDataFromFile()
    {

    }

    [Button("Load Tilemap")]
    public void TestLoad()
    {
        LoadTilemap(SellectTile);
    }
}

