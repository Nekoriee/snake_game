using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState {free, occupied, nofood};
public enum FoodType {nofood, normal, burn, freeze, golden, ghost}
public enum TileType {ground, wall, water};

public enum TileID { grass, sand, water, // surfaces
    g2s_s, g2s_ic, g2s_oc, // grass to sand transitions
    s2w_s, s2w_ic, s2w_oc, // sand to water transitions
    grass_rock, // grass decorations
    sand_rock // sand decorations
}

public abstract class Tile
{
    public abstract void SetTileState(TileState state);
    public abstract TileState GetTileState();

    public abstract TileType GetTileType();

    public abstract FoodType GetFoodType();
    public abstract void CreateFood(FoodType type);
    public abstract void DeleteFood();
    public abstract IEnumerator PlayAnimation();
}
