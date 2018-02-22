using System;
using UnityEngine;

namespace ProWorldSDK
{
	public static class Combination
	{
        public enum Morph
        {
            Shift,
            Clamp
        }

        public static float[,] Add(Morph morph, float constant, params float[][,] a)
        {
            var count = a.Length;

            if (count < 1) throw new UnityException("No input");

            var resolution = a[0].GetLength(0);

            var c = new float[resolution, resolution];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var total = constant;
// ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var v in a)
// ReSharper restore LoopCanBeConvertedToQuery
                    {
                        total += v[y, x];
                    }

                    switch (morph)
                    {
                        case Morph.Shift:
                            c[y, x] = total / count;
                            break;
                        case Morph.Clamp:
                            c[y, x] = Math.Min(total, 1);
                            break;
                    }
                }
            }

            return c;
        }

        public static float[,] Sub(Morph morph, float constant, params float[][,] a)
        {
            var count = a.Length;

            if (count < 1) throw new UnityException("No input");

            var resolution = a[0].GetLength(0);

            var c = new float[resolution, resolution];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var total = a[0][y,x];
                    for (int index = 1; index < a.Length; index++)
                    {
                        total -= a[index][y, x];
                    }
                    total -= constant;

                    switch (morph)
                    {
                        case Morph.Shift:
                            c[y, x] = (total / count) + (1 - (1f / count)); // v / n + [1 - (1/n)]
                            break;
                        case Morph.Clamp:
                            c[y, x] = Math.Max(total, 0);
                            break;
                    }
                }
            }

            return c;
        }

        public static float[,] Mul(float constant, params float[][,] a)
        {
            var count = a.Length;

            if (count < 1) throw new UnityException("No input");

            var resolution = a[0].GetLength(0);

            var c = new float[resolution, resolution];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var total = constant;
// ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var v in a)
// ReSharper restore LoopCanBeConvertedToQuery
                    {
                        total *= v[y, x];
                    }

                    c[y, x] = total;
                }
            }

            return c;
        }

        public static float[,] Div(float constant, params float[][,] a)
        {
            var count = a.Length;

            if (count < 1) throw new UnityException("No input");

            var resolution = a[0].GetLength(0);

            var c = new float[resolution, resolution];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var total = a[0][y, x];
                    for (int index = 1; index < a.Length; index++)
                    {
                        total /= a[index][y, x];
                    }
                    total /= constant;

                    c[y, x] = Math.Min(total, 1);
                }
            }

            return c;

        }
	}
}
