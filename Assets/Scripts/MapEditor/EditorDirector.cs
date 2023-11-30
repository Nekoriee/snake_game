using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFB;

public enum EditorMode { tile, spawn, trigger }

public class EditorDirector : MonoBehaviour
{

    public const int cntTiles_H = 19;
    public const int cntTiles_V = 17;

    public Camera mainCamera;
    [SerializeField] private TMPro.TextMeshProUGUI textHint;
    [SerializeField] private TMPro.TextMeshProUGUI textTile;
    [SerializeField] private TMPro.TextMeshProUGUI modeHint;
    [SerializeField] private TMPro.TextMeshProUGUI textError;
    [SerializeField] private TextAsset tileInfoJson;

    MapInfo mapInfo = new MapInfo();
    [HideInInspector] public TileInfo tileInfo;
    List<Tile_MapInfo> tilesToSave = new List<Tile_MapInfo>();
    public Dictionary<Vector3, Tile_Editor> tiles = new Dictionary<Vector3, Tile_Editor>();
    public Dictionary<Vector3, NoFood_Editor> tilesTrigger = new Dictionary<Vector3, NoFood_Editor>();
    Dictionary<string, Object> prefabList = new Dictionary<string, Object>();

    Spawn_MapInfo spawn = new Spawn_MapInfo();

    string currentTileId = "grass";
    float currentRotation = 90f;
    Vector3 currentSelectedTile = Vector3.zero;
    EditorMode mode = EditorMode.tile;
    Vector3 mousePrevPos;

    Snake_Editor snakeFake;
    Snake_Editor snakeReal;
    GameObject currentTileFrame;

    NoFood_Editor noFoodFake;

    int availableNoFoodTiles = cntTiles_H * cntTiles_V;

    string levelPath = "";
    string levelName = "MyLevel";
    List<string> tileIDs = new List<string>();
    int currentTileListID = 0;

    private void UpdateTextMode()
    {
        switch (mode)
        {
            case EditorMode.tile:
                textHint.text = "M - change mode | mouse/arrows - move | alt/rmb - rotate | space/lmb - place | tab - switch   tile";
                modeHint.text = "mode: tile";
                textTile.alpha = 0.6f;
                break;
            case EditorMode.spawn:
                textHint.text = "M - change mode | mouse/arrows - move | alt/rmb - rotate | space/lmb - place";
                modeHint.text = "mode: spawn";
                textTile.alpha = 0;
                break;
            case EditorMode.trigger:
                textHint.text = "M - change mode | mouse/arrows - move | space/lmb - place | delete - delete";
                modeHint.text = "mode: nofood";
                textTile.alpha = 0;
                break;
        }
    }

    private void UpdateTextTile()
    {
        if (mode == EditorMode.tile)
        {
            textTile.text = "Tile: " + currentTileId;
        }
    }

    private void DestroyTiles()
    {
        foreach (Tile_Editor tile in tiles.Values)
        {
            Destroy(tile.gameObject);
        }
    }

    private void DestroyTileTriggers()
    {
        foreach (NoFood_Editor tileTrigger in tilesTrigger.Values)
        {
            Destroy(tileTrigger.gameObject);
        }
    }

    private void CreateLevelField()
    {
        if (levelPath != "" && File.Exists(levelPath))
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(levelPath);
            mapInfo = JsonUtility.FromJson<MapInfo>(reader.ReadToEnd());
            reader.Close();
        }
        if (mapInfo != null)
        {
            availableNoFoodTiles = cntTiles_H * cntTiles_V;
            DestroyTiles();
            DestroyTileTriggers();
            tiles = new Dictionary<Vector3, Tile_Editor>();
            tilesTrigger = new Dictionary<Vector3, NoFood_Editor>();
            foreach (Tile_MapInfo tile in mapInfo.tiles)
            {
                Vector3 pos = new Vector3(tile.x, tile.y, 0);
                tiles.Add(pos, new Tile_Editor(pos, tile.tileID, tile.rotation.eulerAngles, transform, this, prefabList["BaseTile"]));
                if (tile.state == "nofood")
                {
                    tiles[pos].state = TileState.nofood;
                    tilesTrigger.Add(pos, new NoFood_Editor(pos, this, prefabList["BaseSprite"]));
                    availableNoFoodTiles--;
                }
                else
                {
                    tiles[pos].state = TileState.free;
                }
                
            }
            Heading heading = Heading.N;
            switch (Mathf.RoundToInt(mapInfo.spawn.rotation))
            {
                case 0:
                    heading = Heading.N;
                    break;

                case 90:
                    heading = Heading.W;
                    break;

                case 180:
                    heading = Heading.S;
                    break;

                case 270:
                    heading = Heading.E;
                    break;

                default:
                    break;
            }
            if (snakeReal != null)
            {
                Destroy(snakeReal.snake.gameObject);
                snakeReal = null;
            }
            snakeReal = new Snake_Editor(new Vector3(mapInfo.spawn.x, mapInfo.spawn.y, 0), heading, 3, this, prefabList["BaseSprite"]);
            spawn = mapInfo.spawn;
        }
    }

    public void SaveMap()
    {
        if (snakeReal != null)
        {
            textError.alpha = 0f;
            tilesToSave = new List<Tile_MapInfo>();

            for (int i = 0; i < cntTiles_V; i++)
            {
                for (int j = 0; j < cntTiles_H; j++)
                {
                    Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);
                    Tile_MapInfo tile = new Tile_MapInfo();
                    tile.x = Mathf.RoundToInt(tiles[pos].gameObject.transform.position.x);
                    tile.y = Mathf.RoundToInt(tiles[pos].gameObject.transform.position.y);
                    tile.rotation = tiles[pos].gameObject.transform.rotation;
                    tile.state = System.Enum.GetName(typeof(TileState), tiles[pos].state);
                    tile.tileID = tiles[pos].tileId;
                    tilesToSave.Add(tile);
                }
            }

            string pathLevelFolder = Application.dataPath + "/Resources/Levels";
            string pathLevel = StandaloneFileBrowser.SaveFilePanel("Save .wld file", pathLevelFolder, levelName, "wld");
            
            if (pathLevel != "")
            {
                mapInfo.map_name = "";
                mapInfo.spawn = spawn;
                mapInfo.tiles = tilesToSave;

                string json = JsonUtility.ToJson(mapInfo);

                System.IO.File.Delete(pathLevel);
                System.IO.File.WriteAllText(pathLevel, json);
            }
        }
        else textError.alpha = 0.52f;
    }

    private void OpenMap()
    {
        string pathLevelFolder = Application.dataPath + "/Resources/Levels";
        string[] pathLevel = StandaloneFileBrowser.OpenFilePanel("Select .wld file", pathLevelFolder, "wld", false);
        if (pathLevel.Length > 0 && pathLevel[0] != "")
        {
            levelPath = pathLevel[0];
            levelName = System.IO.Path.GetFileName(pathLevel[0]);
            CreateLevelField();
        }
    }

    private void CheckInput()
    {
        Vector3 curPos;

        if (Input.mousePosition != mousePrevPos)
        {
            Cursor.visible = true;
            curPos = new Vector3(Mathf.Round(mainCamera.ScreenToWorldPoint(Input.mousePosition).x),
            Mathf.Round(mainCamera.ScreenToWorldPoint(Input.mousePosition).y), 0);
            if (tiles.ContainsKey(curPos))
            {
                if (curPos != currentSelectedTile) tiles[currentSelectedTile].UnselectTile();
                currentSelectedTile = curPos;
            }
        }
        else
        {
            curPos = currentSelectedTile;
        } 

        mousePrevPos = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentSelectedTile.x != -1 * Mathf.FloorToInt(cntTiles_H / 2))
        {
            Cursor.visible = false;
            curPos = new Vector3(currentSelectedTile.x - 1, currentSelectedTile.y, 0);
            if (curPos != currentSelectedTile) tiles[currentSelectedTile].UnselectTile();
            currentSelectedTile = curPos;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentSelectedTile.x != Mathf.FloorToInt(cntTiles_H / 2))
        {
            Cursor.visible = false;
            curPos = new Vector3(currentSelectedTile.x + 1, currentSelectedTile.y, 0);
            if (curPos != currentSelectedTile) tiles[currentSelectedTile].UnselectTile();
            currentSelectedTile = curPos;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentSelectedTile.y != Mathf.FloorToInt(cntTiles_V / 2))
        {
            Cursor.visible = false;
            curPos = new Vector3(currentSelectedTile.x, currentSelectedTile.y + 1, 0);
            if (curPos != currentSelectedTile) tiles[currentSelectedTile].UnselectTile();
            currentSelectedTile = curPos;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && currentSelectedTile.y != -1 * Mathf.FloorToInt(cntTiles_V / 2))
        {
            Cursor.visible = false;
            curPos = new Vector3(currentSelectedTile.x, currentSelectedTile.y - 1, 0);
            if (curPos != currentSelectedTile) tiles[currentSelectedTile].UnselectTile();
            currentSelectedTile = curPos;
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftAlt)) currentRotation += 90f;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            switch (mode)
            {
                case EditorMode.tile:
                    bool canBePlaced = true;
                    if (snakeReal != null)
                    {
                        foreach (SnakeBody_Editor body in snakeReal.bodyList)
                        {
                            Vector3 bodyPos = body.gameObject.transform.position;
                            Vector3 pos = new Vector3(Mathf.Round(bodyPos.x), Mathf.Round(bodyPos.y), 0);
                            if (pos == currentSelectedTile)
                            {
                                canBePlaced = false;
                                break;
                            }
                        }
                    }
                    if (canBePlaced)
                    {
                        if (tiles[currentSelectedTile].state != TileState.free) canBePlaced = false;
                    }
                    if (canBePlaced)
                    {
                        float rotation;
                        if (currentTileId != "grass" 
                            && currentTileId != "sand" 
                            && currentTileId != "water"
                            && currentTileId != "snow"
                            && currentTileId != "ice") rotation = currentRotation;
                        else rotation = 90f * Random.Range(0, 5);

                        Destroy(tiles[currentSelectedTile].gameObject);
                        tiles.Remove(currentSelectedTile);

                        tiles.Add(currentSelectedTile, new Tile_Editor(currentSelectedTile, currentTileId, new Vector3(rotation, 270f, 90f), transform, this, prefabList["BaseTile"]));
                    }
                    break;

                case EditorMode.spawn:
                    if (snakeFake.CanBePlaced())
                    {
                        if (snakeReal != null)
                        {
                            Destroy(snakeReal.snake.gameObject);
                            snakeReal = null;
                        }
                        snakeReal = new Snake_Editor(Vector3.zero, Heading.N, 3, this, prefabList["BaseSprite"]);
                        snakeReal.snake.transform.position = snakeFake.snake.transform.position;
                        snakeReal.snake.transform.rotation = snakeFake.snake.transform.rotation;
                        spawn.x = snakeReal.snake.transform.position.x;
                        spawn.y = snakeReal.snake.transform.position.y;
                        spawn.rotation = snakeReal.snake.transform.rotation.eulerAngles.z;
                    }

                    break;

                case EditorMode.trigger:
                    if (noFoodFake.CanBePlaced() && availableNoFoodTiles > 0)
                    {
                        tiles[currentSelectedTile].state = TileState.nofood;
                        tilesTrigger.Add(currentSelectedTile, new NoFood_Editor(currentSelectedTile, this, prefabList["BaseSprite"]));
                        availableNoFoodTiles--;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && mode == EditorMode.tile)
        {
            currentTileListID += 1;
            if (currentTileListID >= tileIDs.Count) currentTileListID = 0;
            currentTileId = tileIDs[currentTileListID];
            UpdateTextTile();
        }

        //if (Input.GetKeyDown(KeyCode.Z)) currentTileId = "spawn";
        //if (Input.GetKeyDown(KeyCode.Z)) currentTileId = "nofood";



#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.S)) SaveMap();
        if (Input.GetKey(KeyCode.O))
        {
            OpenMap();
        }
#else
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftControl)) SaveMap();
        if (Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.LeftControl))
        {
            OpenMap();
        }
#endif


        if (Input.GetKeyDown(KeyCode.Delete) && mode == EditorMode.trigger) {
            if (tiles[currentSelectedTile].state == TileState.nofood)
            {
                tiles[currentSelectedTile].state = TileState.free;
                Destroy(tilesTrigger[currentSelectedTile].gameObject);
                tilesTrigger.Remove(currentSelectedTile);
                availableNoFoodTiles++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            int curMode = (int)mode + 1;
            if (curMode >= System.Enum.GetValues(typeof(EditorMode)).Length) curMode = 0;
            mode = (EditorMode)curMode;
            switch (mode)
            {
                case EditorMode.tile:
                    snakeFake.snake.SetActive(false);
                    noFoodFake.gameObject.SetActive(false);
                    break;
                case EditorMode.spawn:
                    snakeFake.snake.SetActive(true);
                    noFoodFake.gameObject.SetActive(false);
                    break;
                case EditorMode.trigger:
                    snakeFake.snake.SetActive(false);
                    noFoodFake.gameObject.SetActive(true);
                    break;
            }
            UpdateTextMode();
        }

        switch (mode)
        {
            case EditorMode.tile:
                tiles[currentSelectedTile].SelectTile(currentTileId, currentRotation);
                break;

            case EditorMode.spawn:
                snakeFake.snake.transform.position = currentSelectedTile;
                snakeFake.snake.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
                snakeFake.UpdateSprites();
                break;

            case EditorMode.trigger:
                noFoodFake.gameObject.transform.position = currentSelectedTile;
                noFoodFake.UpdateSprite(noFoodFake.CanBePlaced());
                break;
        }

        currentTileFrame.gameObject.transform.position = currentSelectedTile;

        Debug.Log("Tile: " + currentTileId);

    }

    private void GetTileIDs()
    {
        if (tileInfo != null)
        {
            foreach (StaticTile_JSON tile in tileInfo.static_tiles)
            {
                tileIDs.Add(tile.id);
            }
            foreach (AnimatedTile_JSON tile in tileInfo.animated_tiles)
            {
                tileIDs.Add(tile.id);
            }
        }
        currentTileId = tileIDs[0];
    }

    private void Awake()
    {
        Application.targetFrameRate = 30;

        prefabList.Add("BaseSprite", Resources.Load<Object>("Prefabs/Game/BaseSprite"));
        prefabList.Add("BaseTile", Resources.Load<Object>("Prefabs/Game/BaseTile"));

        tileInfo = JsonUtility.FromJson<TileInfo>(tileInfoJson.text);
        GetTileIDs();
    }

    public int GetGroundTilesCount()
    {
        int count = 0;
        foreach(KeyValuePair<Vector3, Tile_Editor> tile in tiles) {
            if (tile.Value.type == TileType.ground || tile.Value.type == TileType.water) count++;
        }
        return count;
    }

    private void CreateGrassField()
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

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;

        CreateGrassField();
        snakeFake = new Snake_Editor(Vector3.zero, Heading.N, 3, this, prefabList["BaseSprite"]);
        snakeFake.snake.SetActive(false);
        snakeFake.snake.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        noFoodFake = new NoFood_Editor(Vector3.zero, this, prefabList["BaseSprite"]);
        noFoodFake.gameObject.SetActive(false);

        currentTileFrame = GameObject.Instantiate(prefabList["BaseSprite"]) as GameObject;
        currentTileFrame.gameObject.transform.position = currentSelectedTile;
        currentTileFrame.gameObject.SetSortingLayer("Top");
        currentTileFrame.gameObject.SetTextureOffset(new Vector2(0.4f, 0.2f));

        mousePrevPos = Input.mousePosition;
        textError.alpha = 0;
        UpdateTextMode();
        UpdateTextTile();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }
}
