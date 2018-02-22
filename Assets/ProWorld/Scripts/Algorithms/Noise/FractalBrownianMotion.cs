using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class FractalBrownianMotion : INoise, ISerializable
    {
        public INoise NoiseFunction { get; set; }

        public float H = 2;
        public int Octaves = 8;
        public float Gain = 0.5f;
        public float Lacunarity = 2.0f;

        public FractalBrownianMotion()
        {
            
        }

        public float Noise(float x, float y)
        {
            var total = 0.0f;
            var frequency = 1.0f / H;
            var amplitude = Gain;

            for (var k = 0; k < Octaves; ++k)
            {
                total += NoiseFunction.Noise(x * frequency, y * frequency) * amplitude;
                frequency *= Lacunarity;
                amplitude *= Gain;
            }

            total = Mathf.Clamp01(total);

            return total;
        }

        public FractalBrownianMotion(SerializationInfo info, StreamingContext context)
        {
            H = (float) info.GetValue("H", typeof (float));
            Octaves = info.GetInt32("Octaves");
            Gain = (float)info.GetValue("Gain", typeof(float));
            Lacunarity = (float)info.GetValue("Lacunarity", typeof(float));
            NoiseFunction = (INoise) info.GetValue("Noise", typeof (INoise));
            //Morph = (OutputMorph)info.GetValue("Morph", typeof(OutputMorph));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("H", H);
            info.AddValue("Octaves", Octaves);
            info.AddValue("Gain", Gain);
            info.AddValue("Lacunarity", Lacunarity);
            info.AddValue("Noise", NoiseFunction);
            //info.AddValue("Morph", Morph);
        }
    }
}