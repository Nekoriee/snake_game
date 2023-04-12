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

    const float snakeSpeed = 5f;
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

    private int GetLevelID(string levelPath)
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
            levelID = GetLevelID(prefsLevelPath);
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

    //private void Create_DebugField()
    //{
    //    for (int i = 0; i < cntTiles_V; i++)
    //    {
    //        for (int j = 0; j < cntTiles_H; j++)
    //        {
    //            Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);
    //            if (j < cntTiles_H/2) playField.Add(pos, new TileStatic(pos, "sand", new Vector3(90f * Random.Range(0, 5), 270f, 90f), transform, this, prefabList["BaseTile"]));
    //            else if (j == (int)(cntTiles_H / 2)) playField.Add(pos, new TileAnimated(pos, "s2w_s", new Vector3(180, 270f, 90f), transform, this, prefabList["BaseTile"]));
    //            else playField.Add(pos, new TileAnimated(pos, "water", new Vector3(90f * Random.Range(0, 5), 270f, 90f), transform, this, prefabList["BaseTile"]));
    //            StartCoroutine(playField[pos].PlayAnimation());
    //        }
    //    }

    //    snake = new Snake(new Vector3(0, 0, 0), snakeHeading, snakeSize, snakeSpeed, this);

    //    snake.SetSpeedMultiplier(1f);
    //}

    //private void Create_WaterField()
    //{
    //    for (int i = 0; i < cntTiles_V; i++)
    //    {
    //        for (int j = 0; j < cntTiles_H; j++)
    //        {
    //            Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);

    //            playField.Add(pos, new TileAnimated(pos, "water", new Vector3(90f * Random.Range(0, 5), 270f, 90f), transform, this, prefabList["BaseTile"]));
    //            StartCoroutine(playField[pos].PlayAnimation());
    //        }
    //    }

    //    snake = new Snake(new Vector3(0, 0, 0), snakeHeading, snakeSize, snakeSpeed, this);

    //    snake.SetSpeedMultiplier(1f);
    //}

    private void Create_Default()
    {
        for (int i = 0; i < cntTiles_V; i++)
        {
            for (int j = 0; j < cntTiles_H; j++)
            {
                Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);
                playField.Add(pos, new TileStatic(pos, "grass", Quaternion.Euler(90f * Random.Range(0, 5), 270f, 90f), transform, TileState.free, this, prefabList["BaseTile"]));
            }
        }

        snake = new Snake(new Vector3(0, 0, 0), snakeHeading, snakeSize, snakeSpeed, this);

        snake.SetSpeedMultiplier(prefsSpeed);
        snake.SetModifier(prefsModifier);
    }

    public void ChangeGameState(GameState state)
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
            PlayerPrefs.SetInt(levelID.ToString() + prefsSpeed.ToString() + prefsModifier, currentScore);
        }
        ChangeGameState(GameState.gameover);
        audioController.StopMusic();
        ui.StartGameOver();
    }

    private void MoveSnake()
    {
        if (!snake.Move())
        {
            if (snake.GetState() != SnakeState.gold)
            {
                StopGame();
            }
            else
            {
                Vector3 headPos = snake.GetHeadPos();
                bool gameOver = true;
                for (int i = (int)headPos.x - 1; i <= (int)headPos.x + 1; i++)
                {
                    for (int j = (int)headPos.y - 1; j <= (int)headPos.y + 1; j++)
                    {
                        Vector3 tile = new Vector3(i, j, 0);
                        if (!IsTileAWall(tile) && !IsTileOccupied(tile))
                        {
                            gameOver = false;
                            break;
                        }
                    }
                    if (!gameOver) break;
                }
                if (gameOver)
                {
                    audioController.PlaySound("Sound_Menu_Gameover");
                    StopGame();
                }
            }
        }
        else snakeHeadingBeforeMove = snake.GetHeading();
    }

   IEnumerator Game()
    {
        CreateFood();
        if (PlayerPrefs.HasKey(levelID.ToString() + prefsSpeed.ToString() + prefsModifier) && 
            PlayerPrefs.GetInt(levelID.ToString() + prefsSpeed.ToString() + prefsModifier) > 0)
        {
            goalScore = PlayerPrefs.GetInt(levelID.ToString() + prefsSpeed.ToString() + prefsModifier);
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
        while (gameState != GameState.gameover && prefsModifier != "control")
        {
            yield return new WaitForSeconds(1 / snake.GetSpeed());
            MoveSnake();
        }
        controlLastMove = Time.time;
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
            ChangeGameState(GameState.paused);
            audioController.SetMusicVolumeFade(0.25f, 0.5f);
            ui.StartPause();
        }
        else
        {
            Time.timeScale = 1;
            audioController.SetMusicVolumeFade(1f, 0.5f);
            ChangeGameState(GameState.game);
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
            if (snakeHeadingBeforeMove != Heading.S)
            {
                snake.SetHeading(Heading.N);
                if (prefsModifier == "control") MoveSnake();
                controlLastMove = Time.time;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (snakeHeadingBeforeMove != Heading.W)
            {
                snake.SetHeading(Heading.E);
                if (prefsModifier == "control") MoveSnake();
                controlLastMove = Time.time;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (snakeHeadingBeforeMove != Heading.N)
            {
                snake.SetHeading(Heading.S);
                if (prefsModifier == "control") MoveSnake();
                controlLastMove = Time.time;
            }
                
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (snakeHeadingBeforeMove != Heading.E)
            {
                snake.SetHeading(Heading.W);
                if (prefsModifier == "control") MoveSnake();
                controlLastMove = Time.time;
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
            playField[tilePos].SetTileState(TileState.occupied);
        }
    }

    public void UpdateTiles()
    {
        foreach (Tile tile in playField.Values)
        {
            tile.RevertTileState();
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

    public TileState GetTileState(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetTileState();
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
        //Debug.Log(tilePos.x + " " + tilePos.y + " " + GetTileState(tilePos).ToString());
        if (GetTileState(tilePos) == TileState.nofood) return true;
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
                if (!IsTileOccupied(pos) && !IsTileAWall(pos) && !IsTileNoFood(pos))
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
            if (playField[tilePos].GetTileCurState() == TileState.free && playField[tilePos].GetTileState() == TileState.free &&
                playField[tilePos].GetTileType() != TileType.wall && playField[tilePos].GetTileType() != TileType.nul) return true;
        }
        return false;
    }

    public void CreateFood()
    {
        List<Vector3> availableTiles = GetAvailableTiles();
        if (availableTiles.Count > 0)
        {
            Vector3 foodPos = availableTiles[Random.Range(0, availableTiles.Count)];

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
            else playField[foodPos].CreateFood(FoodType.normal);
        }
        else StopGame();
    }

    public void CreateFood_old()
    {
        Vector3 foodPos;
        do
        {
            foodPos = new Vector3(Mathf.Round(Random.Range(-9, 10)), Mathf.Round(Random.Range(-8, 9)), 0);
        } while (!CanCreateFood(foodPos));

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
        else playField[foodPos].CreateFood(FoodType.normal);
    }

    public void DeleteFood(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            playField[tilePos].DeleteFood();
            AddScore(1);
        }
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
        baseChance.Add("burn", 0.45f); // 15%
        baseChance.Add("freeze", 0.3f); // 15%
        baseChance.Add("gold", 0.15f); // 10%
        baseChance.Add("ghost", 0.05f); // 5%
        // standard apple: 55% 

        switch (prefsModifier)
        {
            case "classic":
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                // standard apple: 100%
                break;
            case "chill":
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", 1f); // 100%
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                break;
            case "chili_pepper":
                foodChance.Add("burn", 1f); // 100%
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", -1f);
                foodChance.Add("ghost", -1f);
                break;
            case "gold":
                foodChance.Add("burn", -1f);
                foodChance.Add("freeze", -1f);
                foodChance.Add("gold", 1f); // 100%
                foodChance.Add("ghost", -1f);
                break;
            case "spaghetti":
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
    }

    private void Update()
    {
        CheckInput();
        CheckControlGameover();
    }
}
