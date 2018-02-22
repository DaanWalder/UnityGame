using System;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class Grid : INoise
    {
        public Vector2 Frequency { get; set; }
        public Vector2 Cutoff { get; set; }


        public Grid()
        {
            Frequency = new Vector2(1,1);
            Cutoff = new Vector2(0.5f,0.5f);
        }

        public float Noise(float x, float y)
        {
            var cutx = Cutoff.x * Frequency.x;
            var leftx = Math.Abs(x % Frequency.x);
            if (x < 0) leftx = Frequency.x - leftx;

            var cuty = Cutoff.y * Frequency.y;
            var lefty = Math.Abs(y % Frequency.y);
            if (y < 0) lefty = Frequency.y - lefty;

            return leftx < cutx || lefty < cuty ? 1f : 0f;
        }
    }
}
