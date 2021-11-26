using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void SetSortingLayer(this GameObject gameObject, string sortingLayer)
    {
        gameObject.GetComponent<Renderer>().sortingLayerName = sortingLayer;
    }

    public static void SetTextureOffset(this GameObject gameObject, Vector2 textureOffset)
    {
        gameObject.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", textureOffset);
    }
}
