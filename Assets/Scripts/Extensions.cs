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

    public static Quaternion HeadingToRotation(Heading heading)
    {
        switch (heading)
        {
            case Heading.N:
                return Quaternion.Euler(90, 270, 90);
            case Heading.E:
                return Quaternion.Euler(0, 270, 90);
            case Heading.S:
                return Quaternion.Euler(-90, 270, 90);
            case Heading.W:
                return Quaternion.Euler(180, 270, 90);
            default:
                break;
        }
        return Quaternion.Euler(-1,-1,-1);
    }
}
