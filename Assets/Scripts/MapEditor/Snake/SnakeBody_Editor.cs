using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBody_Editor
{
    public GameObject gameObject;
    public string bodyType;

    public SnakeBody_Editor(string bodyType, Vector3 position, Transform parent, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = position;
        gameObject.transform.parent = parent;
        gameObject.SetSortingLayer("Snake");
        this.bodyType = bodyType;
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
