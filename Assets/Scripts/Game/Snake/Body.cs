using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : SnakeBody
{
    private GameObject gameObject;

    public Body(Vector3 position, Transform parent, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = position;
        gameObject.transform.parent = parent;
        gameObject.SetSortingLayer("Snake");
    }

    public override void SetPosition(Vector3 position)
    {
        gameObject.transform.localPosition = position;
    }

    public override Vector3 GetPosition()
    {
        return gameObject.transform.localPosition;
    }

    public override void SetHeading(Heading heading)
    {
        return;
    }

    public override Heading GetHeading()
    {
        return Heading.N;
    }

    public override void UpdateSprite(SnakeState state)
    {
        switch (state)
        {
            case SnakeState.normal:
                gameObject.SetTextureOffset(new Vector2(0, 0.6f));
                break;
            case SnakeState.burn:
                gameObject.SetTextureOffset(new Vector2(0.4f, 0.6f));
                break;
            case SnakeState.freeze:
                gameObject.SetTextureOffset(new Vector2(0.2f, 0.6f));
                break;
            case SnakeState.gold:
                gameObject.SetTextureOffset(new Vector2(0.6f, 0.6f));
                break;
            case SnakeState.ghost:
                gameObject.SetTextureOffset(new Vector2(0.8f, 0.6f));
                break;
        }
    }
}
