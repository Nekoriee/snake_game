using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState {free, occupied};
public enum FoodType {nofood, normal, burn, freeze, golden, ghost}
public enum TileType {ground, wall, water};

public abstract class Tile
{
    public abstract void SetTileState(TileState state);
    public abstract TileState GetTileState();

    public abstract TileType GetTileType();

    public abstract FoodType GetFoodType();
    public abstract void CreateFood(FoodType type);
    public abstract void DeleteFood();
}
