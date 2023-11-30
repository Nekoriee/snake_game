using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState {free, occupied, nofood, nul};
public enum FoodType {nofood, normal, burn, freeze, golden, ghost, drunk}
public enum TileType {ground, wall, water, ice, cliff, nul, portal};

public abstract class Tile
{
    public abstract void SetTileCurState(TileState state);
    public abstract void RevertTileCurState();
    public abstract TileState GetTileBaseState();
    public abstract TileState GetTileCurState();

    public abstract TileType GetTileType();

    public abstract string GetTileID();

    public abstract FoodType GetFoodType();
    public abstract void CreateFood(FoodType type);

    public abstract bool HasFood();
    public abstract void DeleteFood();
    public abstract IEnumerator PlayAnimation();

    public abstract Quaternion GetTileRotation();
}
