using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Heading {N, S, W, E}

public class Head : SnakeBody
{

    private GameObject gameObject;
    private Heading heading;

    public Head(Vector3 position, Heading heading, Transform parent, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = position;
        this.heading = heading;
        RotateHead();
        gameObject.transform.parent = parent;
        gameObject.SetSortingLayer("Snake");
    }

    private void RotateHead()
    {
        switch (heading)
        {
            case Heading.N:
                gameObject.transform.rotation = Quaternion.Euler(90, 270, 90);
                break;
            case Heading.E:
                gameObject.transform.rotation = Quaternion.Euler(0, 270, 90);
                break;
            case Heading.S:
                gameObject.transform.rotation = Quaternion.Euler(-90, 270, 90);
                break;
            case Heading.W:
                gameObject.transform.rotation = Quaternion.Euler(180, 270, 90);
                break;
            default:
                break;
        }
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
        this.heading = heading;
        RotateHead();
    }

    public override Heading GetHeading()
    {
        return heading;
    }

    public override void UpdateSprite(SnakeState state, bool isSubmerged)
    {
        switch (state)
        {
            case SnakeState.normal:
                if (!isSubmerged) gameObject.SetTextureOffset(new Vector2(0, 0.8f));
                else gameObject.SetTextureOffset(new Vector2(0f, 0f));
                break;
            case SnakeState.burn:
                if (!isSubmerged) gameObject.SetTextureOffset(new Vector2(0.4f, 0.8f));
                else gameObject.SetTextureOffset(new Vector2(0f, 0f));
                break;
            case SnakeState.freeze:
                if (!isSubmerged) gameObject.SetTextureOffset(new Vector2(0.2f, 0.8f));
                else gameObject.SetTextureOffset(new Vector2(0f, 0f));
                break;
            case SnakeState.gold:
                if (!isSubmerged) gameObject.SetTextureOffset(new Vector2(0.6f, 0.8f));
                else gameObject.SetTextureOffset(new Vector2(0f, 0f));
                break;
            case SnakeState.ghost:
                gameObject.SetTextureOffset(new Vector2(0.8f, 0.8f));
                break;
        }
    }

}
