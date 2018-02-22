using System;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class PerlinNoise : INoise
    {
        public float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}