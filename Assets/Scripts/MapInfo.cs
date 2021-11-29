using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
[System.Serializable]
public class Tile_MapInfo
{
    public string tileID;
    public int x;
    public int y;
    public int rotation;
    public string state;
}

[System.Serializable]
public class Spawn_MapInfo
{
    public float x;
    public float y;
    public float rotation;
}

[System.Serializable]
public class MapInfo
{
    public string map_name;
    public List<Tile_MapInfo> tiles;
    public Spawn_MapInfo spawn;
}


