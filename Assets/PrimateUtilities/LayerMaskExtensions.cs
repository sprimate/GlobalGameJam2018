using System.Collections.Generic;
using UnityEngine;

public static class LayerMaskExtensions
{
    public static bool HasLayer(this LayerMask layerMask, int layer)
    {
        if (layerMask == (layerMask | (1 << layer)))
        {
            return true;
        }

        return false;
    }

    public static List<int> GetLayers(this LayerMask layerMask)
    {
        var layers = new List<int>();

        for (int i = 0; i < 32; i++) //is 32 the max number of layers still?
        {
            if (layerMask == (layerMask | (1 << i)))
            {
                layers.Add(i);
            }
        }

        return layers;
    }

    public static int AddToLayer(this LayerMask layerMask, int originalLayer)
    {
        foreach (var layerToAdd in layerMask.GetLayers())
        {
            originalLayer |= (1 << layerToAdd);
        }
        return originalLayer;
    }

    public static int RemoveFromLayer(this LayerMask layerMask, int originalLayer)
    {
        foreach (var layerToRemove in layerMask.GetLayers())
        {
            originalLayer &= ~(1 << layerToRemove);
        }
        return originalLayer;
    }
}