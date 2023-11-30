using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SnakeState {normal, burn, freeze, gold, ghost, drunk }

public class Snake
{
    public GameObject snake;
    private GameDirector gameDirector;
    private SnakeState state = SnakeState.normal;
    private List<SnakeBody> bodyList = new List<SnakeBody>();
    private float speed;
    private float speedMultiplier = 1;
    private float currentSpeed;
    private string curModifier = "none";

    public Snake(Vector3 position, Heading heading, int size, float speed, GameDirector gameDirector)
    {
        snake = new GameObject("Snake");
        bodyList.Add(new Head(position, heading, snake.transform, gameDirector.GetPrefab("BaseSprite")));
        
        for (int i = 1; i < size; i++)
        {
            Vector3 bodyPos = Vector3.zero;
            switch (heading)
            {
                case Heading.N:
                    bodyPos = new Vector3(position.x, position.y - i, 0);
                    break;
                case Heading.S:
                    bodyPos = new Vector3(position.x, position.y + i, 0);
                    break;
                case Heading.W:
                    bodyPos = new Vector3(position.x + i, position.y, 0);
                    break;
                case Heading.E:
                    bodyPos = new Vector3(position.x - i, position.y, 0);
                    break;
            }
            bodyList.Add(new Body(bodyPos, snake.transform, gameDirector.GetPrefab("BaseSprite")));
        }
        this.speed = speed;
        this.currentSpeed = speed * speedMultiplier;
        this.gameDirector = gameDirector;
        UpdateSprites();
    }

    private void UpdateSprites()
    {
        foreach(SnakeBody bodyPart in bodyList)
        {
            if (gameDirector.GetTileType(bodyPart.GetPosition()) == TileType.water) bodyPart.UpdateSprite(state, true);
            else bodyPart.UpdateSprite(state, false);
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        currentSpeed = speed * speedMultiplier;
    }

    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }

    public float GetSpeed()
    {
        return currentSpeed;
    }

    public Heading GetHeading()
    {
        return bodyList[0].GetHeading();
    }

    public void SetHeading(Heading heading)
    {
        if (bodyList[0].GetHeading() != heading)
        {
            bodyList[0].SetHeading(heading);
            gameDirector.audioController.PlaySound("Sound_Snake_Turn");
        }
    }

    public SnakeState GetState()
    {
        return state;
    }

    public void SetModifier(string modifier)
    {
        curModifier = modifier;
    }

    public string GetModifier()
    {
        return curModifier;
    }

    public void SetState(SnakeState state)
    {
        this.state = state;
        switch (state)
        {
            case SnakeState.burn:
                currentSpeed = speed * speedMultiplier * 2;
                break;
            case SnakeState.freeze:
                currentSpeed = speed * speedMultiplier / 2;
                break;
            default:
                currentSpeed = speed * speedMultiplier;
                break;
        }
    }

    public Vector3 GetHeadPos()
    {
        return bodyList[0].GetPosition();
    }

    public void OccupyTiles()
    {
        foreach (SnakeBody bodyPart in bodyList)
        {
            gameDirector.OccupyTile(bodyPart.GetPosition());
        }
    }

    private void Grow()
    {
        if (bodyList.Count + 1 >= gameDirector.GetTileCount_H() * gameDirector.GetTileCount_V())
        {
            gameDirector.StopGame();
        }
        else bodyList.Add(new Body(bodyList[bodyList.Count - 1].GetPosition(), snake.transform, gameDirector.GetPrefab("BaseSprite")));
    }

    public void EatFood(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.normal:
                SetState(SnakeState.normal);
                gameDirector.audioController.PlaySound("Sound_Apple_Normal");
                gameDirector.audioController.SetMusicPitchFade(1f, 0.5f);
                break;
            case FoodType.burn:
                SetState(SnakeState.burn);
                gameDirector.audioController.PlaySound("Sound_Apple_Burn");
                gameDirector.audioController.SetMusicPitchFade(1.1f, 0.5f);
                break;
            case FoodType.freeze:
                SetState(SnakeState.freeze);
                gameDirector.audioController.PlaySound("Sound_Apple_Freeze");
                gameDirector.audioController.SetMusicPitchFade(0.9f, 0.5f);
                break;
            case FoodType.drunk:
                SetState(SnakeState.drunk);
                gameDirector.audioController.PlaySound("Sound_Apple_Drunk");
                gameDirector.audioController.SetMusicPitchSine(0.1f);
                break;
            case FoodType.golden:
                SetState(SnakeState.gold);
                gameDirector.audioController.PlaySound("Sound_Apple_Gold");
                gameDirector.audioController.SetMusicPitchFade(1f, 0.5f);
                break;
            case FoodType.ghost:
                SetState(SnakeState.ghost);
                gameDirector.audioController.PlaySound("Sound_Apple_Ghost");
                gameDirector.audioController.SetMusicPitchFade(1f, 0.5f);
                break;
            default:
                break;
        }
        UpdateSprites();
    }

    private Vector3 GetNextPos(Vector3 posCurrent, Heading heading)
    {
        Vector3 posNew = posCurrent;
        switch (heading)
        {
            case Heading.N:
                posNew = new Vector3(posCurrent.x, posCurrent.y + 1, 0);
                break;

            case Heading.S:
                posNew = new Vector3(posCurrent.x, posCurrent.y - 1, 0);
                break;

            case Heading.W:
                posNew = new Vector3(posCurrent.x - 1, posCurrent.y, 0);
                break;

            case Heading.E:
                posNew = new Vector3(posCurrent.x + 1, posCurrent.y, 0);
                break;
        }
        return posNew;
    }

    public bool CanGoThoughCliff(Vector3 tilePos, Heading heading)
    {
        Quaternion qHeading = Extensions.HeadingToRotation(heading);
        TileType tileType = gameDirector.GetTileType(tilePos);
        if (gameDirector.IsTileOccupied(tilePos)) return false;
        switch (tileType)
        {
            case TileType.wall:
            case TileType.portal:
                return false;
            case TileType.cliff:
                return
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).x * 10f) * 0.1f * -1
            == Mathf.RoundToInt(qHeading.x * 10f) * 0.1f * -1
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).y * 10f) * 0.1f * -1
            == Mathf.RoundToInt(qHeading.y * 10f) * 0.1f * -1
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).z * 10f) * 0.1f * -1
            == Mathf.RoundToInt(qHeading.z * 10f) * 0.1f * -1
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).w * 10f) * 0.1f * -1
            == Mathf.RoundToInt(qHeading.w * 10f) * 0.1f * -1
            ||
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).x * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.x * 10f) * 0.1f
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).y * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.y * 10f) * 0.1f
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).z * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.z * 10f) * 0.1f
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).w * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.w * 10f) * 0.1f
            ||
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).x * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.x * 10f) * 0.1f * -1
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).y * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.y * 10f) * 0.1f * -1
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).z * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.z * 10f) * 0.1f * -1
            &&
            Mathf.RoundToInt(gameDirector.GetTileRotation(tilePos).w * 10f) * 0.1f
            == Mathf.RoundToInt(qHeading.w * 10f) * 0.1f * -1;
            default:
                return true;
        };
    }

    private bool IsTileValid(Vector3 tilePos)
    {
        return !(tilePos.y > Mathf.Floor(gameDirector.GetTileCount_V() / 2)
            || tilePos.y < -1 * Mathf.Floor(gameDirector.GetTileCount_V() / 2)
            || tilePos.x < -1 * Mathf.Floor(gameDirector.GetTileCount_H() / 2)
            || tilePos.x > Mathf.Floor(gameDirector.GetTileCount_H() / 2)
            || gameDirector.GetTileCurState(tilePos) == TileState.occupied
            || gameDirector.IsTileAWall(tilePos)
            || !CanGoThoughCliff(tilePos, GetHeading())
            );
    }

    public bool Move()
    {
        Vector3 headPos = bodyList[0].GetPosition(); ;
        Vector3 headPosNew = GetNextPos(headPos, bodyList[0].GetHeading());

        if ((gameDirector.IsTileAWall(headPosNew) == false &&
            gameDirector.IsTileOccupied(headPosNew) == false &&
            gameDirector.GetTileType(headPosNew) != TileType.cliff &&
            gameDirector.GetTileType(headPosNew) != TileType.portal)
            || state == SnakeState.ghost
            || gameDirector.GetTileType(headPosNew) == TileType.portal
            || CanGoThoughCliff(headPosNew, GetHeading())
            )
        {
            if (gameDirector.GetTileType(headPosNew) == TileType.portal)
            {
                Vector3 headPosAfterPortal = gameDirector.GetRandomPortal(headPosNew);
                if (headPosAfterPortal != headPosNew)
                {
                    headPosNew = GetNextPos(headPosAfterPortal, bodyList[0].GetHeading());
                    gameDirector.audioController.PlaySound("Sound_Portal");
                }
            }
            if (!IsTileValid(headPosNew) && state != SnakeState.ghost) return false;
            if (state == SnakeState.ghost)
            {
                switch (bodyList[0].GetHeading())
                {
                    case Heading.N:
                        if (headPosNew.y > Mathf.Floor(gameDirector.GetTileCount_V() / 2))
                            headPosNew.y = -1 * Mathf.Floor(gameDirector.GetTileCount_V() / 2);
                        break;
                    case Heading.S:
                        if (headPosNew.y < -1 * Mathf.Floor(gameDirector.GetTileCount_V() / 2))
                            headPosNew.y = Mathf.Floor(gameDirector.GetTileCount_V() / 2);
                        break;
                    case Heading.W:
                        if (headPosNew.x < -1 * Mathf.Floor(gameDirector.GetTileCount_H() / 2))
                            headPosNew.x = Mathf.Floor(gameDirector.GetTileCount_H() / 2);
                        break;
                    case Heading.E:
                        if (headPosNew.x > Mathf.Floor(gameDirector.GetTileCount_H() / 2))
                            headPosNew.x = -1 * Mathf.Floor(gameDirector.GetTileCount_H() / 2);
                        break;
                }
            }

            bool foodEaten = false;
            FoodType foodType = gameDirector.GetFoodType(headPosNew);
            if (foodType != FoodType.nofood)
            {
                EatFood(foodType);
                gameDirector.DeleteAllFood();
                if (curModifier != "hungry" && curModifier != "spaghetti") Grow();
                foodEaten = true;
            }

            if (curModifier == "spaghetti") Grow();

            for (int i = bodyList.Count - 1; i > 0; i--)
            {
                bodyList[i].SetPosition(bodyList[i - 1].GetPosition());
            }
            
            bodyList[0].SetPosition(headPosNew);

            gameDirector.UpdateTiles();
            UpdateSprites();
            if (foodEaten) gameDirector.CreateFood();
            if (gameDirector.GetTileType(headPosNew) == TileType.ice && state != SnakeState.ghost)
            {
                gameDirector.audioController.PlaySound("Sound_Snake_Move_Ice");
            }
            else if (gameDirector.GetTileType(headPosNew) == TileType.cliff && state != SnakeState.ghost)
            {
                gameDirector.audioController.PlaySound("Sound_Snake_Fall");
            }
            else if (gameDirector.GetTileType(headPosNew) == TileType.water && state != SnakeState.ghost)
            {
                gameDirector.audioController.PlaySound("Sound_Snake_Move_Water");
            }
            else
            {
                gameDirector.audioController.PlaySound("Sound_Snake_Move");
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
