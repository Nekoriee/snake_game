using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake_Editor
{
    public GameObject snake;
    private EditorDirector editorDirector;
    public List<SnakeBody_Editor> bodyList = new List<SnakeBody_Editor>();

    public Snake_Editor(Vector3 position, Heading heading, int size, EditorDirector editorDirector, Object prefab)
    {
        snake = new GameObject("Snake");
        bodyList.Add(new SnakeBody_Editor("head", position, snake.transform, prefab));

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
            bodyList.Add(new SnakeBody_Editor("body", bodyPos, snake.transform, prefab));
        }
        this.editorDirector = editorDirector;
        UpdateSprites();

    }

    public bool CanBePlaced()
    {
        bool canBePlaced = true;
        foreach(SnakeBody_Editor bodyPart in bodyList)
        {
            Vector3 bodyPos = bodyPart.gameObject.transform.position;
            bodyPos = new Vector3(Mathf.Round(bodyPos.x), Mathf.Round(bodyPos.y), 0);
            if (editorDirector.tiles.ContainsKey(bodyPos))
            {
                TileType tileType = editorDirector.tiles[bodyPos].GetTileType();
                if (tileType == TileType.wall || tileType == TileType.nul)
                {
                    canBePlaced = false;
                    break;
                }
            }
            else
            {
                canBePlaced = false;
                break;
            }
        }
        return canBePlaced;
    }

    public void UpdateSprites()
    {
        foreach (SnakeBody_Editor bodyPart in bodyList)
        {
            bodyPart.UpdateSprite(CanBePlaced());
        }
    }
}
