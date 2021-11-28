using System.Collections;
using System.Collections.Generic;
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
    public AudioController audioController;

    const float snakeSpeed = 5f;
    private Dictionary<string, float> foodChance = new Dictionary<string, float>();

    private int currentScore = 0;
    private int goalScore = 10;
    private bool musicOn = true;

    private Dictionary<Vector3, Tile> playField = new Dictionary<Vector3, Tile>();
    private Snake snake;
    private Heading snakeHeadingBeforeMove;

    private GameState gameState = GameState.game;

    private Dictionary<string, Object> prefabList = new Dictionary<string, Object>();

    private void Create_DefaultField()
    {
        for (int i = 0; i < cntTiles_V; i++)
        {
            for (int j = 0; j < cntTiles_H; j++)
            {
                Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);
                playField.Add(pos , new DefaultTile(pos, transform, this, prefabList["BaseTile"]));
            }
        }

        snake = new Snake(new Vector3(0, 0, 0), snakeHeading, snakeSize, snakeSpeed, this);

        snake.SetSpeedMultiplier(1f);
    }

    public void ChangeGameState(GameState state)
    {
        gameState = state;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    private void StopGame()
    {
        ChangeGameState(GameState.gameover);
        audioController.StopMusic();
        ui.StartGameOver();
    }

   IEnumerator Game()
    {
        CreateFood();
        ui.UpdateCurrentScore(currentScore.ToString());
        ui.UpdateGoalScore(goalScore.ToString());
        if (musicOn == true) audioController.PlayMusic();
        while (gameState != GameState.gameover)
        {
            snakeHeadingBeforeMove = snake.GetHeading();
            yield return new WaitForSeconds(1 / snake.GetSpeed());
            if (!snake.Move() && snake.GetState() != SnakeState.gold )
            {
                audioController.PlaySound("Sound_Menu_Gameover");
                StopGame();
            }
                
            yield return null;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore(int score)
    {
        currentScore += score;
        ui.UpdateCurrentScore(currentScore.ToString());
    }

    public void SetGoalScore(int score)
    {
        ui.UpdateGoalScore(score.ToString());
    }

    private void CheckInputGame()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (snakeHeadingBeforeMove != Heading.S && snakeHeadingBeforeMove != Heading.N) snake.SetHeading(Heading.N);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (snakeHeadingBeforeMove != Heading.W && snakeHeadingBeforeMove != Heading.E) snake.SetHeading(Heading.E);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (snakeHeadingBeforeMove != Heading.N && snakeHeadingBeforeMove != Heading.S) snake.SetHeading(Heading.S);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (snakeHeadingBeforeMove != Heading.E && snakeHeadingBeforeMove != Heading.W) snake.SetHeading(Heading.W);
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Pause))
        {
            Time.timeScale = 0;
            ChangeGameState(GameState.paused);
            audioController.SetMusicVolume(0.25f);
            ui.StartPause();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            musicOn = !musicOn;
            if (musicOn == true) audioController.PlayMusic(); else audioController.StopMusic();
        }
    }

    private void CheckInputPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Pause))
        {
            Time.timeScale = 1;
            audioController.SetMusicVolume(1f);
            ChangeGameState(GameState.game);
            ui.StopPause();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            musicOn = !musicOn;
            if (musicOn == true) audioController.PlayMusic(); else audioController.StopMusic();
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
            musicOn = !musicOn;
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
            tile.SetTileState(TileState.free);
        }
        snake.OccupyTiles();
    }

    public bool IsTileOccupied(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            if (playField[tilePos].GetTileState() == TileState.occupied) return true;
            else return false;
        }
        else return true;
    }

    public bool IsTileAWall(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            if (playField[tilePos].GetTileType() == TileType.wall) return true;
            else return false;
        }
        else return true;
    }

    public FoodType GetFoodType(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            return playField[tilePos].GetFoodType();
        }
        else return FoodType.nofood;
    }

    private bool CanCreateFood(Vector3 tilePos) // can create if tile is ground and free
    {
        if (playField.ContainsKey(tilePos))
        {
            if (playField[tilePos].GetTileState() == TileState.free &&
                playField[tilePos].GetTileType() == TileType.ground) return true;
        }
        return false;
    }

    public void CreateFood()
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
        } else if (random > foodChance["ghost"] && random <= foodChance["gold"])
        {
            playField[foodPos].CreateFood(FoodType.golden);
        } else if (random > foodChance["gold"] && random <= foodChance["freeze"])
        {
            playField[foodPos].CreateFood(FoodType.freeze);
        } else if (random > foodChance["freeze"] && random <= foodChance["burn"])
        {
            playField[foodPos].CreateFood(FoodType.burn);
        } else playField[foodPos].CreateFood(FoodType.normal);

    }

    public void DeleteFood(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            playField[tilePos].DeleteFood();
            AddScore(1);
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

    private void Awake()
    {
        Application.targetFrameRate = 120;

        prefabList.Add("BaseSprite", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/BaseSprite.prefab", typeof(Object)));
        prefabList.Add("BaseTile", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/BaseTile.prefab", typeof(Object)));

        foodChance.Add("burn", 0.45f);
        foodChance.Add("freeze", 0.3f);
        foodChance.Add("gold", 0.15f);
        foodChance.Add("ghost", 0.05f);
    }

    void Start()
    {
        Create_DefaultField();
        StartCoroutine(Game());
    }

    private void Update()
    {
        CheckInput();
    }
}
