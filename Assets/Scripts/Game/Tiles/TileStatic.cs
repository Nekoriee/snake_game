using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStatic : Tile
{
    private TileState state = TileState.free;
    private TileState curState = TileState.free;
    private TileType type = TileType.nul;
    private FoodType foodType = FoodType.nofood;
    private string tileID;
    private GameDirector gameDirector;
    private GameObject gameObject;
    private GameObject foodObject;
    private Object foodPrefab;

    public TileStatic(Vector3 pos, string tileId, Quaternion rotation, Transform parent, TileState state, GameDirector gameDirector, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rotation;
        gameObject.transform.parent = parent;
        this.gameDirector = gameDirector;
        foodPrefab = gameDirector.GetPrefab("BaseSprite");
        this.tileID = tileId;
        this.state = state;
        TileType typeOut;

        foreach (StaticTile_JSON tile in gameDirector.tileInfo.static_tiles)
        {
            if (tile.id == tileId)
            {
                gameObject.SetTextureOffset(new Vector2(tile.x, tile.y));
                if (!System.Enum.TryParse<TileType>(tile.type, out typeOut))
                {
                    type = TileType.ice;
                }
                else
                {
                    type = typeOut;
                }
                break;
            }
        }
    }

    public override void SetTileCurState(TileState newState)
    {
        curState = newState;
    }

    public override void RevertTileCurState()
    {
        curState = state;
    }

    public override TileState GetTileCurState()
    {
        return curState;
    }

    public override TileState GetTileBaseState()
    {
        return state;
    }

    public override TileType GetTileType()
    {
        return type;
    }

    public override string GetTileID()
    {
        return tileID;
    }

    public override void CreateFood(FoodType type)
    {
        foodObject = GameObject.Instantiate(foodPrefab) as GameObject;
        foodObject.transform.position = gameObject.transform.position;
        foodObject.SetSortingLayer("Item");
        switch (type)
        {
            case FoodType.drunk:
                foodObject.SetTextureOffset(new Vector2(0.4f, 0));
                SetFoodType(FoodType.drunk);
                break;
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

    public override bool HasFood()
    {
        if (foodObject == null) return false;
        else return true;
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

    public override IEnumerator PlayAnimation()
    {
        yield return null;
    }

    public override Quaternion GetTileRotation()
    {
        return gameObject.transform.rotation;
    }
}
