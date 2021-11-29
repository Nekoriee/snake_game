using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditorMode { tile, spawn, trigger }

public class EditorDirector : MonoBehaviour
{

    public const int cntTiles_H = 19;
    public const int cntTiles_V = 17;

    public Camera mainCamera;

    MapInfo mapInfo;
    [HideInInspector] public TileInfo tileInfo;
    List<Tile_MapInfo> tilesToSave = new List<Tile_MapInfo>();
    Dictionary<Vector3, Tile_Editor> tiles = new Dictionary<Vector3, Tile_Editor>();
    Dictionary<Vector3, TileState> tilesState = new Dictionary<Vector3, TileState>();
    Dictionary<string, Object> prefabList = new Dictionary<string, Object>();

    Spawn_MapInfo spawn = new Spawn_MapInfo();

    string currentTileId = "grass";
    float currentRotation = 90f;
    Vector3 currentSelectedTile;
    EditorMode mode = EditorMode.tile;

   

    private void CheckInput()
    {
        Vector3 mousePos = new Vector3(Mathf.Round(mainCamera.ScreenToWorldPoint(Input.mousePosition).x),
            Mathf.Round(mainCamera.ScreenToWorldPoint(Input.mousePosition).y), 0);
        if (tiles.ContainsKey(mousePos))
        {
            if (mousePos != currentSelectedTile) tiles[currentSelectedTile].UnselectTile();
            currentSelectedTile = mousePos;
            tiles[mousePos].SelectTile(currentTileId, currentRotation);
        }


        if (Input.GetMouseButtonDown(1)) currentRotation += 90f;
        if (Input.GetMouseButtonDown(0))
        {
            if (mode == EditorMode.tile)
            {
                float rotation;
                if (currentTileId != "grass" && currentTileId != "sand" && currentTileId != "water") rotation = currentRotation;
                else rotation = 90f * Random.Range(0, 5);

                Destroy(tiles[currentSelectedTile].gameObject);
                tiles.Remove(currentSelectedTile);
                
                tiles.Add(currentSelectedTile, new Tile_Editor(currentSelectedTile, currentTileId, new Vector3(rotation, 270f, 90f), transform, this, prefabList["BaseTile"]));
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) currentTileId = "grass";
        if (Input.GetKeyDown(KeyCode.W)) currentTileId = "sand";
        if (Input.GetKeyDown(KeyCode.E)) currentTileId = "water";
        if (Input.GetKeyDown(KeyCode.R)) currentTileId = "g2s_s";
        if (Input.GetKeyDown(KeyCode.T)) currentTileId = "g2s_ic";
        if (Input.GetKeyDown(KeyCode.Y)) currentTileId = "g2s_oc";
        if (Input.GetKeyDown(KeyCode.U)) currentTileId = "s2w_s";
        if (Input.GetKeyDown(KeyCode.I)) currentTileId = "s2w_ic";
        if (Input.GetKeyDown(KeyCode.O)) currentTileId = "s2w_oc";
        if (Input.GetKeyDown(KeyCode.A)) currentTileId = "grass_rock";
        if (Input.GetKeyDown(KeyCode.S)) currentTileId = "sand_rock";

        if (Input.GetKeyDown(KeyCode.Z)) currentTileId = "spawn";
        if (Input.GetKeyDown(KeyCode.Z)) currentTileId = "nofood";

        if (Input.GetKeyDown(KeyCode.M))
        {
            int curMode = (int)mode + 1;
            if (curMode >= System.Enum.GetValues(typeof(EditorMode)).Length) curMode = 0;
            mode = (EditorMode)curMode;
            switch (mode)
            {
                case EditorMode.tile:
                    Debug.Log("Tile editor");
                    break;
                case EditorMode.spawn:
                    Debug.Log("Spawn editor");
                    break;
                case EditorMode.trigger:
                    Debug.Log("Trigger editor");
                    break;
            }
        }
    }

    private void Awake()
    {
        prefabList.Add("BaseSprite", Resources.Load<Object>("Prefabs/Game/BaseSprite"));
        prefabList.Add("BaseTile", Resources.Load<Object>("Prefabs/Game/BaseTile"));

        string path = Application.dataPath + "/Resources/tileInfo.json";
        System.IO.StreamReader reader = new System.IO.StreamReader(path);
        tileInfo = JsonUtility.FromJson<TileInfo>(reader.ReadToEnd());
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < cntTiles_V; i++)
        {
            for (int j = 0; j < cntTiles_H; j++)
            {
                Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);

                tiles.Add(pos, new Tile_Editor(pos, "grass", new Vector3(90f * Random.Range(0, 5), 270f, 90f), transform, this, prefabList["BaseTile"]));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }
}
