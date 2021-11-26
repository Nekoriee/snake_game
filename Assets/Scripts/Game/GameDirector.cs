using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirector : MonoBehaviour
{

    [SerializeField] private int cntTiles_H = 19;
    [SerializeField] private int cntTiles_V = 17;
    [SerializeField] private int snakeSize = 3;
    [SerializeField] private Heading snakeHeading = Heading.E;

    const float snakeSpeed = 5f;

    private Dictionary<Vector3, Tile> playField = new Dictionary<Vector3, Tile>();
    private Snake snake;
    private Heading snakeHeadingBeforeMove;

    private Dictionary<string, Object> prefabList = new Dictionary<string, Object>();

    private void Create_DefaultField()
    {
        for (int i = 0; i < cntTiles_V; i++)
        {
            for (int j = 0; j < cntTiles_H; j++)
            {
                Vector3 pos = new Vector3(j - cntTiles_H / 2, i - cntTiles_V / 2, 0);
                playField.Add(pos , new DefaultTile(pos, transform, this));
            }
        }

        snake = new Snake(new Vector3(0, 0, 0), snakeHeading, snakeSize, snakeSpeed, this);
    }

    private void StopGame()
    {
        Debug.LogError("Can't move here");
    }

   IEnumerator Game()
    {
        CreateFood();
        while(true)
        {
            snakeHeadingBeforeMove = snake.GetHeading();
            yield return new WaitForSeconds(1 / snake.GetSpeed());
            if (!snake.Move()) StopGame();
            yield return null;
        }
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (snakeHeadingBeforeMove != Heading.S) snake.SetHeading(Heading.N);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (snakeHeadingBeforeMove != Heading.W) snake.SetHeading(Heading.E);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (snakeHeadingBeforeMove != Heading.N) snake.SetHeading(Heading.S);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (snakeHeadingBeforeMove != Heading.E) snake.SetHeading(Heading.W);
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

        float random = Random.Range(1, System.Enum.GetValues(typeof(FoodType)).Length);
        playField[foodPos].CreateFood((FoodType)Mathf.Round(random));
    }

    public void DeleteFood(Vector3 tilePos)
    {
        if (playField.ContainsKey(tilePos))
        {
            playField[tilePos].DeleteFood();
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
        prefabList.Add("BaseSprite", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/BaseSprite.prefab", typeof(Object)));
        prefabList.Add("Apple_Normal", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Items/Apple_Normal.prefab", typeof(Object)));
        prefabList.Add("Apple_Burn", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Items/Apple_Burn.prefab", typeof(Object)));
        prefabList.Add("Apple_Freeze", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Items/Apple_Freeze.prefab", typeof(Object)));
        prefabList.Add("Apple_Ghost", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Items/Apple_Ghost.prefab", typeof(Object)));
        prefabList.Add("Apple_Golden", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Items/Apple_Golden.prefab", typeof(Object)));
        prefabList.Add("Snake_Normal_Head", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Snake/Parts/Head.prefab", typeof(Object)));
        prefabList.Add("Snake_Normal_Body", UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Game/Snake/Parts/Body.prefab", typeof(Object)));
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
