using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBody_Editor
{
    public GameObject gameObject;
    public string bodyType;

    public SnakeBody_Editor(string bodyType, Vector3 position, Transform parent, Heading heading, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = position;
        gameObject.transform.parent = parent;
        gameObject.SetSortingLayer("Snake");
        this.bodyType = bodyType;
        Vector3 euler = Vector3.zero;
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

    public void UpdateSprite(bool tileAvailable)
    {
        switch (bodyType)
        {
            case "head":
                if (tileAvailable) gameObject.SetTextureOffset(new Vector2(0f, 0.8f));
                else gameObject.SetTextureOffset(new Vector2(0.8f, 0.2f));
                break;
            case "body":
                if (tileAvailable) gameObject.SetTextureOffset(new Vector2(0f, 0.6f));
                else gameObject.SetTextureOffset(new Vector2(0.8f, 0f));
                break;
        }
        
    }
}
