using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTile : Tile
{
    private TileState state = TileState.free;
    private TileType type = TileType.ground;
    private FoodType foodType = FoodType.nofood;
    private GameDirector gameDirector;
    private GameObject gameObject;
    private GameObject foodObject;
    private Object foodPrefab;

    public DefaultTile(Vector3 pos, Transform parent, GameDirector gameDirector, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = pos;
        gameObject.transform.rotation = Quaternion.Euler(90 * Random.Range(0, 5), 270, 90);
        gameObject.transform.parent = parent;
        this.gameDirector = gameDirector;
        foodPrefab = gameDirector.GetPrefab("BaseSprite");
    }

    public override void SetTileState(TileState state)
    {
        this.state = state;
    }

    public override TileState GetTileState()
    {
        return state;
    }

    public override TileType GetTileType()
    {
        return type;
    }

    public override void CreateFood(FoodType type)
    {
        foodObject = GameObject.Instantiate(foodPrefab) as GameObject;
        foodObject.transform.position = gameObject.transform.position;
        foodObject.SetSortingLayer("Item");
        switch (type)
        {
            case FoodType.normal:
                foodObject.SetTextureOffset(new Vector2(0, 0.4f));
                SetFoodType(FoodType.normal);
                break;
            case FoodType.burn:
                foodObject.SetTextureOffset(new Vector2(0.4f, 0.4f));
                SetFoodType(FoodType.burn);
                break;
            case FoodType.freeze:
                foodObject.SetTextureOffset(new Vector2(0.2f, 0.4f));
                SetFoodType(FoodType.freeze);
                break;
            case FoodType.golden:
                foodObject.SetTextureOffset(new Vector2(0.6f, 0.4f));
                SetFoodType(FoodType.golden);
                break;
            case FoodType.ghost:
                foodObject.SetTextureOffset(new Vector2(0.8f, 0.4f));
                SetFoodType(FoodType.ghost);
                break;
            default:
                break;
        }

    }

    public override void DeleteFood()
    {
        if (foodObject != null) Transform.Destroy(foodObject);
        foodType = FoodType.nofood;
    }

    public override FoodType GetFoodType()
    {
        return foodType;
    }

    private void SetFoodType(FoodType type)
    {
        foodType = type;
    }

}
