using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private TMPro.TextMeshProUGUI modeHint;
    [SerializeField] private TMPro.TextMeshProUGUI textError;

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

    NoFood_Editor noFoodFake;

    int availableNoFoodTiles;

    public void SaveMap(string name)
    {
        if (snakeReal != null)
        {
            textError.alpha = 0f;
            spawn.x = snakeReal.snake.transform.position.x;
            spawn.y = snakeReal.snake.transform.position.y;
            spawn.rotation = snakeReal.snake.transform.rotation.eulerAngles.z;

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

            mapInfo.map_name = name;
            mapInfo.spawn = spawn;
            mapInfo.tiles = tilesToSave;

            string json = JsonUtility.ToJson(mapInfo);
            string pathLevelFolder = Application.dataPath + "/Resources/Levels";
            
            string pathLevel = StandaloneFileBrowser.SaveFilePanel("Save .wld file", pathLevelFolder, "MyLevel", "wld");
            if (pathLevel != "")
            {
                System.IO.File.Delete(pathLevel);
                System.IO.File.WriteAllText(pathLevel, json);
            }
        }
        else textError.alpha = 0.52f;
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
                        if (currentTileId != "grass" && currentTileId != "sand" && currentTileId != "water") rotation = currentRotation;
                        else rotation = 90f * Random.Range(0, 5);

                        Destroy(tiles[currentSelectedTile].gameObject);
                        tiles.Remove(currentSelectedTile);

                        tiles.Add(currentSelectedTile, new Tile_Editor(currentSelectedTile, currentTileId, new Vector3(rotation, 270f, 90f), transform, this, prefabList["BaseTile"]));

                        availableNoFoodTiles = GetGroundTilesCount() / 2;
                    }
                    break;

                case EditorMode.spawn:
                    if (snakeFake.CanBePlaced())
                    {
                        if (snakeReal == null)
                        {
                            snakeReal = new Snake_Editor(Vector3.zero, Heading.N, 3, this, prefabList["BaseSprite"]);
                        }
                        snakeReal.snake.transform.position = snakeFake.snake.transform.position;
                        snakeReal.snake.transform.rotation = snakeFake.snake.transform.rotation;
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

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentTileId = "grass";
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentTileId = "sand";
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentTileId = "water";
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentTileId = "g2s_s";
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentTileId = "g2s_ic";
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentTileId = "g2s_oc";
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentTileId = "s2w_s";
        if (Input.GetKeyDown(KeyCode.Alpha8)) currentTileId = "s2w_ic";
        if (Input.GetKeyDown(KeyCode.Alpha9)) currentTileId = "s2w_oc";
        if (Input.GetKeyDown(KeyCode.Alpha0)) currentTileId = "grass_rock";
        if (Input.GetKeyDown(KeyCode.Minus)) currentTileId = "sand_rock";

        if (Input.GetKeyDown(KeyCode.Z)) currentTileId = "spawn";
        if (Input.GetKeyDown(KeyCode.Z)) currentTileId = "nofood";

        if (Input.GetKeyDown(KeyCode.S)) SaveMap("MyLevel");

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
                    textHint.text = "M - change mode | mouse/arrows - move | alt/rmb - rotate | space/lmb - place | 1-0,minus - tiles";
                    modeHint.text = "mode: tile";
                    break;
                case EditorMode.spawn:
                    snakeFake.snake.SetActive(true);
                    noFoodFake.gameObject.SetActive(false);
                    textHint.text = "M - change mode | mouse/arrows - move | alt/rmb - rotate | space/lmb - place";
                    modeHint.text = "mode: spawn";
                    break;
                case EditorMode.trigger:
                    snakeFake.snake.SetActive(false);
                    noFoodFake.gameObject.SetActive(true);
                    textHint.text = "M - change mode | mouse/arrows - move | space/lmb - place | delete - delete";
                    modeHint.text = "mode: nofood";
                    break;
            }
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
        
        
    }

    private void Awake()
    {
        Application.targetFrameRate = 30;

        prefabList.Add("BaseSprite", Resources.Load<Object>("Prefabs/Game/BaseSprite"));
        prefabList.Add("BaseTile", Resources.Load<Object>("Prefabs/Game/BaseTile"));

        string path = Application.dataPath + "/Resources/tileInfo.json";
        System.IO.StreamReader reader = new System.IO.StreamReader(path);
        tileInfo = JsonUtility.FromJson<TileInfo>(reader.ReadToEnd());
        reader.Close();
    }

    public int GetGroundTilesCount()
    {
        int count = 0;
        foreach(KeyValuePair<Vector3, Tile_Editor> tile in tiles) {
            if (tile.Value.type == TileType.ground || tile.Value.type == TileType.water) count++;
        }
        return count;
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

        snakeFake = new Snake_Editor(Vector3.zero, Heading.N, 3, this, prefabList["BaseSprite"]);
        snakeFake.snake.SetActive(false);
        snakeFake.snake.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        noFoodFake = new NoFood_Editor(Vector3.zero, this, prefabList["BaseSprite"]);
        noFoodFake.gameObject.SetActive(false);

        mousePrevPos = Input.mousePosition;
        textError.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }
}
