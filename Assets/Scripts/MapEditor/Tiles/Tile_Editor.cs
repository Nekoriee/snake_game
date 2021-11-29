using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile_Editor
{
    public string tileId = "";
    public TileState state = TileState.free;
    public TileType type = TileType.ground;
    public EditorDirector editorDirector;
    public GameObject gameObject;
    public Quaternion rotation; 

    public Tile_Editor(Vector3 pos, string tileId, Vector3 rotation, Transform parent, EditorDirector editorDirector, Object prefab)
    {
        gameObject = GameObject.Instantiate(prefab) as GameObject;
        gameObject.transform.position = pos;
        this.rotation = Quaternion.Euler(rotation);
        gameObject.transform.rotation = Quaternion.Euler(rotation);
        gameObject.transform.parent = parent;
        this.editorDirector = editorDirector;

        foreach (StaticTile_JSON tile in editorDirector.tileInfo.static_tiles)
        {
            if (tile.id == tileId)
            {
                this.tileId = tileId;
                gameObject.SetTextureOffset(new Vector2(tile.x, tile.y));
                if (!System.Enum.TryParse<TileType>(tile.type, out type)) type = TileType.ground;
                break;
            }
        }

        foreach (AnimatedTile_JSON tile in editorDirector.tileInfo.animated_tiles)
        {
            if (tile.id == tileId)
            {
                this.tileId = tileId;
                gameObject.SetTextureOffset(new Vector2(tile.anim_frames[0].x, tile.anim_frames[0].y));
                if (!System.Enum.TryParse<TileType>(tile.type, out type)) type = TileType.ground;
                break;
            }
        }

        if (tileId == "")
        {
            this.tileId = "grass";
            type = TileType.ground;
            gameObject.SetTextureOffset(new Vector2(0, 0.75f));
        }
    }

    public void SetTileState(TileState state)
    {
        this.state = state;
    }

    public TileState GetTileState()
    {
        return state;
    }

    public TileType GetTileType()
    {
        return type;
    }

    public void SelectTile(string tileId, float rotation)
    {
        foreach (StaticTile_JSON tile in editorDirector.tileInfo.static_tiles)
        {
            if (tile.id == tileId)
            {
                gameObject.SetTextureOffset(new Vector2(tile.x, tile.y));
                break;
            }
        }

        foreach (AnimatedTile_JSON tile in editorDirector.tileInfo.animated_tiles)
        {
            if (tile.id == tileId)
            {
                gameObject.SetTextureOffset(new Vector2(tile.anim_frames[0].x, tile.anim_frames[0].y));
                break;
            }
        }
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rotation, 270f, 90f));
    }

    public void UnselectTile()
    {
        foreach (StaticTile_JSON tile in editorDirector.tileInfo.static_tiles)
        {
            if (tile.id == tileId)
            {
                gameObject.SetTextureOffset(new Vector2(tile.x, tile.y));
                break;
            }
        }

        foreach (AnimatedTile_JSON tile in editorDirector.tileInfo.animated_tiles)
        {
            if (tile.id == tileId)
            {
                gameObject.SetTextureOffset(new Vector2(tile.anim_frames[0].x, tile.anim_frames[0].y));
                break;
            }
        }
        gameObject.transform.rotation = rotation;
    }
}
