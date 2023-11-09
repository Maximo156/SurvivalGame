using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sound
{
    int maxHeight;
    int width;
    SoundSettings settings;
    float flattening;
    Vector3 offset;
    public Sound(int height, int width, float flattening, SoundSettings settings, Vector3 offset)
    {
        maxHeight = height;
        this.width = width;
        this.settings = settings;
        this.flattening = flattening;
        this.offset = offset;
    }

    public float GetHeight(Vector3 pos)
    {
        pos = pos + offset;
        float res = 0;
        float div = 0;

        float multiplier = 1;

        for(int i = 0; i < settings.octaves; i++)
        {
            var loc = pos;

            loc *= multiplier;

            res += Mathf.PerlinNoise(loc.x / settings.scale + 0.1f, loc.z / settings.scale + 0.6f) / (multiplier * 2);
            div += 1/ (multiplier * 2f);
            multiplier *= settings.change;
        }

        float distanceScale = Mathf.Pow(Mathf.Max(1 - (Vector3.Distance(pos, offset) / width * 2), 0), 1.5f);

        float baseHeight = res / div  * distanceScale;

        return Mathf.Pow(baseHeight, flattening) * maxHeight;
    }
}

[Serializable]
public class SoundSettings
{
    public int octaves;
    public float change;
    public int scale;
}
