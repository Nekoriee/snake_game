using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoFood_Editor
{
    public GameObject gameObject;
    EditorDirector editorDirector;
    public NoFood_Editor(Vector3 position, EditorDirector editorDirector, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = position;
        gameObject.SetSortingLayer("Menu");
        this.editorDirector = editorDirector;
        UpdateSprite(true);
    }

    public bool CanBePlaced()
    {
        bool canBePlaced = true;
        Vector3 pos = gameObject.transform.position;
        Vector3 normPos = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0);
        TileType type = editorDirector.tiles[normPos].type;
        TileState state = editorDirector.tiles[normPos].state;
        if (type == TileType.wall || type == TileType.nul || state == TileState.nofood) canBePlaced = false;
        return canBePlaced;
    }

    public void UpdateSprite(bool isTileAvailable)
    {
        if (isTileAvailable) gameObject.SetTextureOffset(new Vector2(0.6f, 0.2f));
        else gameObject.SetTextureOffset(new Vector2(0.6f, 0));
    }
}
