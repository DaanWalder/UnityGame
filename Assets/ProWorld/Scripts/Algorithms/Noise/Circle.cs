using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    public class Circle : INoise, ISerializable
    {
        public enum Mode
        {
            Bell,
            Spherical,
            Linear,
        }

        public Circle()
        {
            
        }

        public float[,] Generate(int resolution, Vector2 center, Vector2 size, Mode mode)
        {
            var output = new float[resolution,resolution];

            var half = resolution/2f;
            var mid = new Vector2(half, half);
            mid.x += center.x*half;
            mid.y += center.y*half;

            var a = size.x*resolution;
            var b = size.y*resolution;

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var p1 = Mathf.Pow(a, 2)*Mathf.Pow(mid.y - y, 2);
                    var p2 = Mathf.Pow(b, 2)*Mathf.Pow(mid.x - x, 2);
                    var bot = Mathf.Sqrt(p1 + p2);
                    var eq = (a*b)/bot;

                    var xx = eq*(mid.x - x);
                    var yy = eq*(mid.y - y);

                    var dist1 = Mathf.Pow(mid.x - x, 2) + Mathf.Pow(mid.y - y, 2);
                    dist1 = Mathf.Sqrt(dist1);

                    var dist2 = Mathf.Pow(xx, 2) + Mathf.Pow(yy, 2);
                    dist2 = Mathf.Sqrt(dist2);

                    // Special case for middle
                    if (Math.Abs(mid.x - x) < float.Epsilon && Math.Abs(mid.y - y) < float.Epsilon)
                    {
                        dist1 = 0;
                        dist2 = 1;
                    }

                    var ratio = dist1/dist2;

                    switch (mode)
                    {
                        case Mode.Linear:
                            output[y, x] = Mathf.Clamp01(1 - ratio);
                            break;
                        case Mode.Spherical:
                            ratio = Mathf.Clamp01(ratio);
                            output[y, x] = Mathf.Max(Mathf.Cos(Mathf.PI/2*ratio), 0);
                            break;
                        case Mode.Bell:
                            const float micro = 1/(2*Mathf.PI);
                            var front = 1/((Mathf.Sqrt(micro)*Mathf.Sqrt(2*Mathf.PI)));
                            var back = -Mathf.Pow(ratio, 2)/(2*micro);

                            output[y, x] = front*Mathf.Exp(back);
                            break;
                    }
                }
            }

            return output;
        }

        public float Noise(float x, float y)
        {
            return Math.Abs((x + y)%2) < float.Epsilon ? 1f : 0f;
        }

// ReSharper disable UnusedParameter.Local
        public Circle(SerializationInfo info, StreamingContext context)
// ReSharper restore UnusedParameter.Local
        {
            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }
    }
}