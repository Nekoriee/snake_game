using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState {game, paused, gameover }

public class GameDirector : MonoBehaviour
{

    [SerializeField] private int cntTiles_H = 19;
    [SerializeField] private int cntTiles_V = 17;
    [SerializeField] private int snakeSize = 3;
    [SerializeField] private Heading snakeHeading = Heading.E;
    [SerializeField] private UIController ui;
    [SerializeField] private TextAsset tileInfoJson;
    [SerializeField] private TextAsset defaultLevelJson;
    public AudioController audioController;

    const float snakeSpeed = 3.5f;
    private Dictionary<string, float> foodChance = new Dictionary<string, float>();

    private int currentScore = 0;
    private int goalScore = 0;
    private bool musicOn = true;

    private Dictionary<Vector3, Tile> playField = new Dictionary<Vector3, Tile>();
    private Snake snake;
    private Heading snakeHeadingBeforeMove;

    private GameState gameState = GameState.game;

    private Dictionary<string, Object> prefabList = new Dictionary<string, Object>();
    public TileInfo tileInfo;

    private int levelID;
    private string prefsLevelPath;
    private float prefsSpeed = 1f;
    private string prefsModifier = "none";

    private float controlLastMove;

    private void SetSnakeState()
    {
        switch (prefsModifier)
        {
            case "chill":
                snake.EatFood(FoodType.freeze);
                break;
            case "chili_pepper":
                snake.EatFood(FoodType.burn);
                break;
            case "gold":
                snake.EatFood(FoodType.golden);
                break;
            case "drunk":
                snake.EatFood(FoodType.drunk);
                break;
            default:
                break;
        }
    }

    public Quaternion GetTileRotation(Vector3 vTile)
    {
        return playField[vTile].GetTileRotation();
    }

    private int GetLevelIDFromPath(string levelPath)
    {
        System.IO.StreamReader reader = new System.IO.StreamReader(levelPath);
        string levelString = reader.ReadToEnd();
        int id = 0;
        foreach (char symbol in levelString)
        {
            id += (int)char.GetNumericValue(symbol);
        }
        return id;
    }

    private int GetLevelIDFromJson(string levelString)
    {
        int id = 0;
        foreach (char symbol in levelString)
        {
            id += (int)char.GetNumericValue(symbol);
        }
        return id;
    }

    public bool IsTileAnimated(string tileId)
    {
        bool isAnimated = false;
        foreach(AnimatedTile_JSON tile in tileInfo.animated_tiles)
        {
            if (tileId == tile.id)
            {
                isAnimated = true;
                break;
            }
                
        }
        return isAnimated;
    }

    private void Create_PlayField()
    {
        MapInfo mapInfo = JsonUtility.FromJson<MapInfo>(defaultLevelJson.text);
        levelID = GetLevelIDFromJson(defaultLevelJson.text);
        if (prefsLevelPath != "" && File.Exists(prefsLevelPath))
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(prefsLevelPath);
            levelID = GetLevelIDFromPath(prefsLevelPath);
            mapInfo = JsonUtility.FromJson<MapInfo>(reader.ReadToEnd());
            if (mapInfo == null)
            {
                mapInfo = JsonUtility.FromJson<MapInfo>(defaultLevelJson.text);
                levelID = GetLevelIDFromJson(defaultLevelJson.text);
                PlayerPrefs.SetString("LevelPath", "");
            }
        }
        else PlayerPrefs.SetString("LevelPath", "");
        foreach (Tile_MapInfo tile in mapInfo.tiles)
        {
            Vector3 pos = new Vector3(tile.x, tile.y, 0);

            TileState state;
            if (!System.Enum.TryParse<TileState>(tile.state, out state)) state = TileState.free;

            if (IsTileAnimated(tile.tileID))
            {
                if (!playField.ContainsKey(pos))
                {
                    playField.Add(pos, new TileAnimated(pos, tile.tileID, tile.rotation, transform, state, this, prefabList["BaseTile"]));
                    StartCoroutine(playField[pos].PlayAnimation());
                }
            }
            else
            {
                if (!playField.ContainsKey(pos)) playField.Add(pos, new TileStatic(pos, tile.tileID, tile.rotation, transform, state, this, prefabList["BaseTile"]));
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
        snake = new Snake(new Vector3(mapInfo.spawn.x, mapInfo.spawn.y, 0), heading, 3, snakeSpeed, this);
        snake.SetSpeedMultiplier(prefsSpeed);
        snake.SetModifier(prefsModifier);
    }

    public List<Vector3> GetPortals(Vector3 posPortal)
    {
        string portalID = GetTileID(posPortal);
        List<Vector3> availablePortals = new List<Vector3>();
        for (int i = -9; i < 10; i++)
        {
            for (int j = -8; j < 9; j++)
            {
                Vector3 pos = new Vector3(i, j, 0);
                if (GetTileType(pos) == TileType.portal 
                    && pos != posPortal
                    && GetTileID(pos) == portalID)
                {
                    availablePortals.Add(new Vector3(i, j, 0));
                }
            }
        }
        return availablePortals;
    }

    public Vector3 GetRandomPortal(Vector3 portalEnter)
    {
        Vector3 portalPos = portalEnter;
        List<Vector3> portals = GetPortals(portalEnter);
        if (portals.Count > 0)
        {
            portalEnter = portals[Random.Range(0, portals.Count)];
        }
        return portalEnter;
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void StopGame()
    {
        audioController.PlaySound("Sound_Menu_Gameover");
        if (currentScore >= goalScore)
        {
            PlayerPrefs.SetInt(levelID.ToString() + prefsModifier, currentScore);
        }
        SetGameState(GameState.gameover);
        audioController.StopMusic();
        ui.StartGameOver();
    }

    private bool MoveSnake()
    {
        // if snake can't move
        if (!snake.Move())
        {
            if (snake.GetState() != SnakeState.gold 
                || GetTileType(snake.GetHeadPos()) == TileType.ice
                || GetTileType(snake.GetHeadPos()) == TileType.cliff) StopGame();
            else
            {
                Vector3 headPos = snake.GetHeadPos();
                // the code below is used to check if there's at least one free tile around the snake's head
                // while the snake is under the golden apple effect so the player won't stuck there forever
                Vector3[] tilesAround = new Vector3[4];
                tilesAround[0] = new Vector3((int)headPos.x - 1, headPos.y, 0);
                tilesAround[1] = new Vector3((int)headPos.x + 1, headPos.y, 0);
                tilesAround[2] = new Vector3(headPos.x, (int)headPos.y - 1, 0);
                tilesAround[3] = new Vector3(headPos.x, (int)headPos.y + 1, 0);
                //  left
                if (!(!IsTileAWall(tilesAround[0]) && !IsTileOccupied(tilesAround[0]) 
                    && GetTileType(tilesAround[0]) != TileType.portal
                    && snake.CanGoThoughCliff(tilesAround[0], Heading.W)
                    // right
                    || !IsTileAWall(tilesAround[1]) && !IsTileOccupied(tilesAround[1]) 
                    && GetTileType(tilesAround[1]) != TileType.portal
                    && snake.CanGoThoughCliff(tilesAround[1], Heading.E)
                    // down
                    || !IsTileAWall(tilesAround[2]) && !IsTileOccupied(tilesAround[2]) 
                    && GetTileType(tilesAround[2]) != TileType.portal
                    && snake.CanGoThoughCliff(tilesAround[2], Heading.S)
                    // up
                    || !IsTileAWall(tilesAround[3]) && !IsTileOccupied(tilesAround[3]) 
                    && GetTileType(tilesAround[3]) != TileType.portal
                    && snake.CanGoThoughCliff(tilesAround[3], Heading.N)
                    ))
                {
                    audioController.PlaySound("Sound_Menu_Gameover");
                    StopGame();
                }
            }
            return false;
        }
        else
        {
            snakeHeadingBeforeMove = snake.GetHeading();
            return true;
        }
    }

    // main gameplay cycle unless "Control" modifier is active
   IEnumerator Game()
    {
        CreateFood();
        if (PlayerPrefs.HasKey(levelID.ToString() + prefsModifier) && 
            PlayerPrefs.GetInt(levelID.ToString() + prefsModifier) > 0)
        {
            goalScore = PlayerPrefs.GetInt(levelID.ToString() + prefsModifier);
        }
        ui.UpdateCurrentScore(currentScore.ToString());
        ui.UpdateGoalScore(goalScore.ToString());
        ui.UpdateModifierText(prefsModifier);
        ui.UpdateSpeedText(prefsSpeed);
        if (PlayerPrefs.HasKey("Music"))
        {
            musicOn = (PlayerPrefs.GetInt("Music") > 0) ? true : false;
        }
        ui.UpdateMusicText(musicOn);
        audioController.SetMusicPitch(1f);
        if (musicOn == true) audioController.PlayMusic();
        snakeHeadingBeforeMove = snake.GetHeading();
        controlLastMove = Time.time;
        SetSnakeState();
        while (gameState != GameState.gameover)
        {
            yield return new WaitForSeconds(1 / snake.GetSpeed());
            
            if (prefsModifier != "control" 
                || GetTileType(snake.GetHeadPos()) == TileType.ice
                || GetTileType(snake.GetHeadPos()) == TileType.cliff)
            {
                if (MoveSnake()) controlLastMove = Time.time;
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void AddScore(int score)
    {
        currentScore += score;
        ui.UpdateCurrentScore(currentScore.ToString());
        if (currentScore > goalScore)
        {
            ui.UpdateGoalScore(currentScore.ToString());
        }
    }

    public void SetGoalScore(int score)
    {
        ui.UpdateGoalScore(score.ToString());
    }

    private void Pause(bool bPause)
    {
        if (bPause)
        {
            Time.timeScale = 0;
            SetGameState(GameState.paused);
            audioController.SetMusicVolumeFade(0.25f, 0.5f);
            ui.StartPause();
        }
        else
        {
            Time.timeScale = 1;
            audioController.SetMusicVolumeFade(1f, 0.5f);
            SetGameState(GameState.game);
            ui.StopPause();
        }
    }

    private void ChangeMusicState()
    {
        musicOn = !musicOn;
        PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);
        if (musicOn == true && GetGameState() != GameState.gameover) audioController.PlayMusic(); else audioController.StopMusic();
        ui.UpdateMusicText(musicOn);
    }

    private void CheckInputGame()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (GetTileType(snake.GetHeadPos()) != TileType.ice
                && GetTileType(snake.GetHeadPos()) != TileType.cliff
                || snake.GetState() == SnakeState.ghost
                )
            {
                if (snake.GetState() != SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.S)
                {
                    snake.SetHeading(Heading.N);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
                else if (snake.GetState() == SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.N) 
                    {
                    snake.SetHeading(Heading.S);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (GetTileType(snake.GetHeadPos()) != TileType.ice
                && GetTileType(snake.GetHeadPos()) != TileType.cliff
                || snake.GetState() == SnakeState.ghost
                )
            {
                if (snake.GetState() != SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.W)
                {
                    snake.SetHeading(Heading.E);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
                else if (snake.GetState() == SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.E)
                {
                    snake.SetHeading(Heading.W);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (GetTileType(snake.GetHeadPos()) != TileType.ice
                && GetTileType(snake.GetHeadPos()) != TileType.cliff
                || snake.GetState() == SnakeState.ghost
                )
            {
                if (snake.GetState() != SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.N)
                {
                    snake.SetHeading(Heading.S);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
                else if (snake.GetState() == SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.S)
                {
                    snake.SetHeading(Heading.N);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (GetTileType(snake.GetHeadPos()) != TileType.ice
                && GetTileType(snake.GetHeadPos()) != TileType.cliff
                || snake.GetState() == SnakeState.ghost
                )
            {
                if (snake.GetState() != SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.E)
                {
                    snake.SetHeading(Heading.W);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
                else if (snake.GetState() == SnakeState.drunk
                    && snakeHeadingBeforeMove != Heading.W)
                {
                    snake.SetHeading(Heading.E);
                    if (prefsModifier == "control") MoveSnake();
                    controlLastMove = Time.time;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Pause))
        {
            Pause(true);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeMusicState();
        }
        
    }

    private void CheckInputPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Pause))
        {
            Pause(false);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeMusicState();
        }
    }

    private void CheckInputGameover()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeMusicState();
        }
    }

    private void CheckInput()
    {
        switch (gameState)
        {
            case GameState.game:
                CheckInputGame();
                break;
            case GameState.paused:
                CheckInputPause();
                break;
            case GameState.gameover:
                CheckInputGameover();
                break;
            default:
                break;
        }

    }

    public void OccupyTile(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            playField[tilePos].SetTileCurState(TileState.occupied);
        }
    }

    public void UpdateTiles()
    {
        foreach (Tile tile in playField.Values)
        {
            tile.RevertTileCurState();
        }
        snake.OccupyTiles();
    }

    public TileState GetTileCurState(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetTileCurState();
        }
        else return TileState.nul;
    }

    public TileState GetTileBaseState(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetTileBaseState();
        }
        else return TileState.nul;
    }

    public bool IsTileOccupied(Vector3 tilePos)
    {
        if (GetTileCurState(tilePos) == TileState.occupied) return true;
        else return false;
    }

    public bool IsTileNoFood(Vector3 tilePos)
    {
        if (GetTileBaseState(tilePos) == TileState.nofood) return true;
        else return false;
    }

    public TileType GetTileType(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetTileType();
        }
        else return TileType.nul;
    }

    public string GetTileID(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetTileID();
        }
        else return "";
    }

    public bool IsTileAWall(Vector3 tilePos)
    {
        TileType type = GetTileType(tilePos);
        if (type == TileType.wall || type == TileType.nul) return true;
        else return false;
    }

    public FoodType GetFoodType(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetFoodType();
        }
        else return FoodType.nofood;
    }

    private List<Vector3> GetAvailableTiles()
    {
        List<Vector3> availableTiles = new List<Vector3>();
        for (int i = -9; i < 10; i++)
        {
            for (int j = -8; j < 9; j++)
            {
                Vector3 pos = new Vector3(i, j, 0);
                if (!IsTileOccupied(pos) 
                    && !IsTileAWall(pos) 
                    && !IsTileNoFood(pos) 
                    && GetTileType(pos) != TileType.cliff
                    && GetTileType(pos) != TileType.portal)
                {
                    availableTiles.Add(new Vector3(i, j, 0));
                }
            }
        }
        return availableTiles;
    }

    private bool CanCreateFood(Vector3 tilePos) // can create if tile is ground and free
    {
        if (playField.ContainsKey(tilePos))
        {
            if (playField[tilePos].GetTileCurState() == TileState.free && playField[tilePos].GetTileBaseState() == TileState.free &&
                playField[tilePos].GetTileType() != TileType.wall && playField[tilePos].GetTileType() != TileType.nul) return true;
        }
        return false;
    }

    public void CreateFood()
    {
        List<Vector3> availableTiles = GetAvailableTiles();
        if (prefsModifier == "double" && availableTiles.Count > 1
            || prefsModifier != "double" && availableTiles.Count > 0)
        {
            int count = 1;
            if (prefsModifier == "double")
            {
                count = 2;
            }
            while (count > 0)
            {
                Vector3 foodPos = availableTiles[Random.Range(0, availableTiles.Count)];
                while (playField[foodPos].HasFood())
                {
                    foodPos = availableTiles[Random.Range(0, availableTiles.Count)];
                }

                float random = Random.Range(0f, 1f);
                if (random <= foodChance["ghost"])
                {
                    playField[foodPos].CreateFood(FoodType.ghost);
                }
                else if (random > foodChance["ghost"] && random <= foodChance["gold"])
                {
                    playField[foodPos].CreateFood(FoodType.golden);
                }
                else if (random > foodChance["gold"] && random <= foodChance["freeze"])
                {
                    playField[foodPos].CreateFood(FoodType.freeze);
                }
                else if (random > foodChance["freeze"] && random <= foodChance["burn"])
                {
                    playField[foodPos].CreateFood(FoodType.burn);
                }
                else if (random > foodChance["burn"] && random <= foodChance["drunk"])
                {
                    playField[foodPos].CreateFood(FoodType.drunk);
                }
                else playField[foodPos].CreateFood(FoodType.normal);
                count -= 1;
            }
            
        }
        else StopGame();
    }

    public void DeleteFood(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            playField[tilePos].DeleteFood();
            AddScore(1);
        }
    }

    public void DeleteAllFood()
    {
        foreach(KeyValuePair<Vector3, Tile> tile in playField)
        {
            tile.Value.DeleteFood();
        }
        AddScore(1);
    }

    private void CheckControlGameover()
    {
        if (prefsModifier == "control" && gameState == GameState.game)
        {
            if (Time.time - controlLastMove > (2f - prefsSpeed))
            {
                ui.UpdateControlTimer("");
                StopGame();
            }
            else
            {
                float timer = (2f - prefsSpeed) - (Time.time - controlLastMove);
                if (timer >= 0f) ui.UpdateControlTimer(timer.ToString("F2"));
                else ui.UpdateControlTimer("0.00");
            }
        }
    }

    public Object GetPrefab(string prefabKey)
    {
        return prefabList[prefabKey];
    }

    public int GetTileCount_H()
    {
        return cntTiles_H;
    }

    public int GetTileCount_V()
    {
        return cntTiles_V;
    }

    public void GetPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("LevelPath"))
        {
            prefsLevelPath = PlayerPrefs.GetString("LevelPath");
        }
        if (PlayerPrefs.HasKey("Speed") && PlayerPrefs.GetFloat("Speed") > 0f)
        {
            prefsSpeed = PlayerPrefs.GetFloat("Speed");
        }
        if (PlayerPrefs.HasKey("Modifier"))
        {
            prefsModifier = PlayerPrefs.GetString("Modifier");
        }
    }

    public void SetFoodChances()
    {
        Dictionary<string, float> baseChance = new Dictionary<string, float>();
        baseChance.Add("drunk", 0.55f); // 10%
        baseChance.Add("burn", 0.45f); // 15%
        baseChance.Add("freeze", 0.3f); // 15%
        baseChance.Add("gold", 0.15f); // 10%
        baseChance.Add("ghost", 0.05f); // 5%
        // standard apple: 45% 

        switch (prefsModifier)
        {
            case "classic":
                foodChance.Add("drunk", -1f);
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                // standard apple: 100%
                break;
            case "chill":
                foodChance.Add("drunk", -1f);
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", 1f); // 100%
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                break;
            case "chili_pepper":
                foodChance.Add("drunk", -1f);
                foodChance.Add("burn", 1f); // 100%
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                break;
            case "gold":
                foodChance.Add("drunk", -1f);
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", 1f); // 100%
                foodChance.Add("ghost", -1f);
                break;
            case "drunk":
                foodChance.Add("drunk", 1f); // 100%
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                break;
            case "spaghetti":
                foodChance.Add("drunk", baseChance["drunk"] - baseChance["ghost"]);
                foodChance.Add("burn", baseChance["burn"] - baseChance["ghost"]);
                foodChance.Add("freeze", baseChance["freeze"] - baseChance["ghost"]);
                foodChance.Add("gold", baseChance["gold"] - baseChance["ghost"]);
                foodChance.Add("ghost", -1f);
                break;
            case "control":
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", baseChance["gold"]);
                foodChance.Add("ghost", baseChance["ghost"]);
                foodChance.Add("drunk", baseChance["drunk"]);
                break;
            default:
                foodChance = baseChance;
                break;
        }
    }

    private void Awake()
    {
        Application.targetFrameRate = 120;
        GetPlayerPrefs();

        audioController.SetMusicPitch(1f);
        
        prefabList.Add("BaseSprite", Resources.Load<Object>("Prefabs/Game/BaseSprite"));
        prefabList.Add("BaseTile", Resources.Load<Object>("Prefabs/Game/BaseTile"));

        SetFoodChances();

        tileInfo = JsonUtility.FromJson<TileInfo>(tileInfoJson.text);
    }

    void Start()
    {
        
        Create_PlayField();
        Pause(false);
        StartCoroutine(Game());
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckInput();
        CheckControlGameover();
        if (gameState == GameState.game) Cursor.visible = false;
        else Cursor.visible = true;
    }
}
