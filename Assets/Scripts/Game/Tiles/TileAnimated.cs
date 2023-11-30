using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimated : Tile
{
    private TileState state = TileState.free;
    private TileState curState = TileState.free;
    private TileType type = TileType.ground;
    private FoodType foodType = FoodType.nofood;
    private string tileID;
    private GameDirector gameDirector;
    private GameObject gameObject;
    private GameObject foodObject;
    private Object foodPrefab;
    private List<Vector2> animFrames = new List<Vector2>();
    private float fps = 2f;
    private int currentFrame;

    public TileAnimated(Vector3 pos, string tileId, Quaternion rotation, Transform parent, TileState state, GameDirector gameDirector, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rotation;
        gameObject.transform.parent = parent;
        this.gameDirector = gameDirector;
        foodPrefab = gameDirector.GetPrefab("BaseSprite");
        this.tileID = tileId;
        this.state = state;

        foreach (AnimatedTile_JSON tile in gameDirector.tileInfo.animated_tiles)
        {
            if (tile.id == tileId)
            {
                foreach (AnimFrame_JSON animFrame in tile.anim_frames)
                {
                    animFrames.Add(new Vector2(animFrame.x, animFrame.y));
                }
                currentFrame = Random.Range(0, animFrames.Count);
                gameObject.SetTextureOffset(animFrames[0]);
                if (!System.Enum.TryParse<TileType>(tile.type, out type)) type = TileType.water;
                break;
            }
        }
    }

    public override IEnumerator PlayAnimation()
    {
        while(true)
        {
            currentFrame++;
            if (currentFrame >= animFrames.Count) currentFrame = 0;
            gameObject.SetTextureOffset(animFrames[currentFrame]);
            yield return new WaitForSeconds(1 / fps);
        }
    }

    public override void SetTileCurState(TileState state)
    {
        curState = state;
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
                if (this.type != TileType.water) foodObject.SetTextureOffset(new Vector2(0.4f, 0));
                else foodObject.SetTextureOffset(new Vector2(0, 0.2f));
                SetFoodType(FoodType.drunk);
                break;
            case FoodType.normal:
                if (this.type != TileType.water) foodObject.SetTextureOffset(new Vector2(0, 0.4f));
                else foodObject.SetTextureOffset(new Vector2(0, 0.2f));
                SetFoodType(FoodType.normal);
                break;
            case FoodType.burn:
                if (this.type != TileType.water) foodObject.SetTextureOffset(new Vector2(0.4f, 0.4f));
                else foodObject.SetTextureOffset(new Vector2(0, 0.2f));
                SetFoodType(FoodType.burn);
                break;
            case FoodType.freeze:
                if (this.type != TileType.water) foodObject.SetTextureOffset(new Vector2(0.2f, 0.4f));
                else foodObject.SetTextureOffset(new Vector2(0, 0.2f));
                SetFoodType(FoodType.freeze);
                break;
            case FoodType.golden:
                if (this.type != TileType.water) foodObject.SetTextureOffset(new Vector2(0.6f, 0.4f));
                else foodObject.SetTextureOffset(new Vector2(0, 0.2f));
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

    public override Quaternion GetTileRotation()
    {
        return gameObject.transform.rotation;
    }
}

