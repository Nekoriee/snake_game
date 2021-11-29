using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StaticTile_JSON
{
    public string id;
    public float x;
    public float y;
    public string type;
}

[System.Serializable]
public class AnimFrame_JSON
{
    public float x;
    public float y;
}

[System.Serializable]
public class AnimatedTile_JSON
{
    public string id;
    public string type;
    public List<AnimFrame_JSON> anim_frames;
}

[System.Serializable]
public class TileInfo
{
    public List<StaticTile_JSON> static_tiles;
    public List<AnimatedTile_JSON> animated_tiles;
}
