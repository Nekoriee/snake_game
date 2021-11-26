using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SnakeState {normal, burn, freeze, gold, ghost }

public class Snake
{
    private GameObject snake;
    private GameDirector gameDirector;
    private SnakeState state = SnakeState.normal;
    private List<SnakeBody> bodyList = new List<SnakeBody>();
    private float speed;
    private float speedMultiplier = 1;
    private float currentSpeed;

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
            bodyPart.UpdateSprite(state);
        }
       
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeed = speed;
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
        bodyList[0].SetHeading(heading);
    }

    public SnakeState GetState()
    {
        return state;
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

    public void OccupyTiles()
    {
        foreach (SnakeBody bodyPart in bodyList)
        {
            gameDirector.OccupyTile(bodyPart.GetPosition());
        }
    }

    private void Grow()
    {
        bodyList.Add(new Body(bodyList[bodyList.Count - 1].GetPosition(), snake.transform, gameDirector.GetPrefab("BaseSprite")));
        UpdateSprites();
    }

    public bool Move()
    {
        Vector3 headPos = bodyList[0].GetPosition(); ;
        Vector3 headPosNew = headPos;

        switch (bodyList[0].GetHeading())
        {
            case Heading.N:
                headPosNew = new Vector3(headPos.x, headPos.y + 1, 0);
                break;

            case Heading.S:
                headPosNew = new Vector3(headPos.x, headPos.y - 1, 0);
                break;
                

            case Heading.W:
                headPosNew = new Vector3(headPos.x - 1, headPos.y, 0);
                break;

            case Heading.E:
                headPosNew = new Vector3(headPos.x + 1, headPos.y, 0);
                break;
        }

        if ((gameDirector.IsTileAWall(headPosNew) == false &&
            gameDirector.IsTileOccupied(headPosNew) == false) || state == SnakeState.ghost)
        {

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
                switch (foodType)
                {
                    case FoodType.normal:
                        SetState(SnakeState.normal);
                        break;
                    case FoodType.burn:
                        SetState(SnakeState.burn);
                        break;
                    case FoodType.freeze:
                        SetState(SnakeState.freeze);
                        break;
                    case FoodType.golden:
                        SetState(SnakeState.gold);
                        break;
                    case FoodType.ghost:
                        SetState(SnakeState.ghost);
                        break;
                    default:
                        break;
                }
                gameDirector.DeleteFood(headPosNew);
                Grow();
                foodEaten = true;
            }

            for (int i = bodyList.Count - 1; i > 0; i--)
            {
                bodyList[i].SetPosition(bodyList[i - 1].GetPosition());
            }
            
            bodyList[0].SetPosition(headPosNew);

            gameDirector.UpdateTiles();
            if (foodEaten) gameDirector.CreateFood();

            return true;
        }
        else return false;
    }
}
