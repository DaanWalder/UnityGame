using System;

namespace ProWorldSDK
{
    [Serializable]
    public class Square : INoise
    {
        public float Frequency { get; set; }
        public float Cutoff { get; set; }

        public Square()
        {
            Frequency = 1f;
            Cutoff = 0.5f;
        }

        public float Noise(float x, float y)
        {
            // STRIPES
            //var left = (x + y)%Frequency;
            //return left < Cutoff ? 1f : 0f;
            //if (x < 0) x = 1 - x;

            var cut = Cutoff * Frequency;

            var leftx = Math.Abs(x%Frequency);
            if (x < 0) leftx = Frequency - leftx;

            var lefty = Math.Abs(y % Frequency);
            if (y < 0) lefty = Frequency - lefty;

            return leftx < cut || lefty < cut ? 0f : 1f;
        }
    }
}
