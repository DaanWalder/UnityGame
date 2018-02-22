using System;
using UnityEngine;

namespace ProWorldSDK
{
	public static class Modifier
	{
        public enum CutoffHandle
        {
            Zero,
            Cutoff,
            One,
        }

        public static float[,] Invert(float[,] a)
        {
            var y = a.GetLength(0);
            var x = a.GetLength(1);

            var b = new float[y,x];

            for (var j = 0; j < y; j++)
            {
                for (var i = 0; i < x; i++)
                {
                    b[j, i] = 1 - a[j, i];
                }
            }

            return b;
        }

        public static float[,] Cutoff(float[,] a, float min, float max, CutoffHandle minHandle, CutoffHandle maxHandle)
        {
            var y = a.GetLength(0);
            var x = a.GetLength(1);

            var b = new float[y, x];

            for (var j = 0; j < y; j++)
            {
                for (var i = 0; i < x; i++)
                {
                    if (a[j, i] >= max)
                    {
                        switch (maxHandle)
                        {
                            case CutoffHandle.Cutoff:
                                b[j, i] = max;
                            break;
                            case CutoffHandle.One:
                                b[j, i] = 1;
                            break;
                            case CutoffHandle.Zero:
                                b[j, i] = 0;
                            break;

                        }
                    }
                    else if (a[j, i] <= min)
                    {
                        switch (minHandle)
                        {
                            case CutoffHandle.Cutoff:
                                b[j, i] = min;
                            break;
                            case CutoffHandle.One:
                                b[j, i] = 1;
                            break;
                            case CutoffHandle.Zero:
                                b[j, i] = 0;
                            break;

                        }
                    }
                    else
                    {
                        b[j, i] = a[j, i];
                    }
                }
            }

            return b;
        }

        public static float[,] Stretch(float[,] a, float from, float to, float min, float max)
        {
            var y = a.GetLength(0);
            var x = a.GetLength(1);

            var b = new float[y, x];

            var dif = max - min;
            var newDif = to - from;

            for (var j = 0; j < y; j++)
            {
                for (var i = 0; i < x; i++)
                {
                    b[j, i] = Mathf.Clamp01((a[j, i] - min) / dif) * newDif + from;
                }
            }

            return b;
        }

        public static float[,] Levels(float[,] a, float level)
        {
            var y = a.GetLength(0);
            var x = a.GetLength(1);

            var b = new float[y, x];

            for (var j = 0; j < y; j++)
            {
                for (var i = 0; i < x; i++)
                {
                    var v = a[j, i]/level;

                    b[j, i] = (float)Math.Round(v, MidpointRounding.AwayFromZero) * level;
                }
            }

            return b;
        }
	}


}
