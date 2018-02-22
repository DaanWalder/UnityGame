using System;
using UnityEngine;

namespace ProWorldSDK
{
    public class Smooth
    {
        public static float[,] SmoothGrayscale(float[,] img, int radius)
        {
            if (radius < 1)
                return img;

            var w = img.GetLength(1);
            var h = img.GetLength(0);

            if (radius > w || radius > h)
                throw new Exception("Radius is larger than dimensions. Don't do this");

            var output = new float[h, w];
            Array.Copy(img, output, img.Length);

            var a = new float[h, w];
            var vmin = new int[Math.Max(w, h)]; // We reuse this so we use the max size needed
            var vmax = new int[Math.Max(w, h)];

            var div = radius + radius + 1;

            for (var x = 0; x < w; x++)
            {
                vmin[x] = Math.Min(x + radius + 1, w - 1);
                vmax[x] = Math.Max(x - radius, 0);
            }

            for (var y = 0; y < h; y++)
            {
                var sum = 0f;
                for (var i = -radius; i <= radius; i++)
                {
                    // Restrict the blur so it doesn't go outside.
                    sum += output[y, Mathf.Clamp(i, 0, w-1)];
                }
                for (var x = 0; x < w; x++)
                {
                    a[y, x] = sum / div;

                    sum += output[y, vmin[x]] - output[y, vmax[x]];
                }
            }

            for (var y = 0; y < h; y++)
            {
                vmin[y] = Math.Min(y + radius + 1, h - 1);
                vmax[y] = Math.Max(y - radius, 0);
            }

            for (var x = 0; x < w; x++)
            {
                var sum = 0f;
                var yp = -radius;

                for (var i = -radius; i <= radius; i++)
                {
                    var yi = Math.Max(0, yp);
                    sum += a[yi, x];
                    yp += 1;
                }
                for (var y = 0; y < h; y++)
                {
                    output[y, x] = Mathf.Clamp01(sum / div);
                    if (x == 0)
                    {
                        vmin[y] = Math.Min(y + radius + 1, h - 1);
                        vmax[y] = Math.Max(y - radius, 0);
                    }

                    sum += a[vmin[y], x] - a[vmax[y], x];
                }
            }

            return output;
        }
        public static int[,] SmoothColor(int[,] img, int radius)
        {
            if (radius < 1)
                return img;

            var w = img.GetLength(1);
            var h = img.GetLength(0);

            if (radius > w || radius > h)
                throw new Exception("Radius is larger than dimensions. Don't do this");

            var output = new int[h, w];
            Array.Copy(img, output, img.Length);

            var r = new int[h, w];
            var g = new int[h, w];
            var b = new int[h, w];
            var vmin = new int[Mathf.Max(w, h)];
            var vmax = new int[Mathf.Max(w, h)];

            var div = radius + radius + 1;

            // Calculate min/max for each value for x
            for (var x = 0; x < w; x++)
            {
                vmin[x] = Mathf.Min(x + radius + 1, w - 1);
                vmax[x] = Mathf.Max(x - radius, 0);
            }
            // Blur in the x direction
            for (var y = 0; y < h; y++)
            {
                var sum = new int[3]; // r g b

                for (var i = -radius; i <= radius; i++)
                {
                    var p = output[y, Mathf.Clamp(i, 0, w - 1)];

                    sum[0] += (p & 0xff0000) >> 16;
                    sum[1] += (p & 0x00ff00) >> 8;
                    sum[2] += (p & 0x0000ff);
                }
                for (var x = 0; x < w; x++)
                {
                    r[y, x] = sum[0] / div;
                    g[y, x] = sum[1] / div;
                    b[y, x] = sum[2] / div;

                    var p1 = output[y, vmin[x]];
                    var p2 = output[y, vmax[x]];

                    sum[0] += ((p1 & 0xff0000) - (p2 & 0xff0000)) >> 16;
                    sum[1] += ((p1 & 0x00ff00) - (p2 & 0x00ff00)) >> 8;
                    sum[2] += (p1 & 0x0000ff) - (p2 & 0x0000ff);
                }
            }
            // Blur in the y direction
            for (var y = 0; y < h; y++)
            {
                vmin[y] = Math.Min(y + radius + 1, h - 1);
                vmax[y] = Math.Max(y - radius, 0);
            }
            for (var x = 0; x < w; x++)
            {
                var sum = new int[3];//
                var yp = -radius;

                for (var i = -radius; i <= radius; i++)
                {
                    var yi = Math.Max(0, yp);
                    sum[0] += r[yi, x];
                    sum[1] += g[yi, x];
                    sum[2] += b[yi, x];
                    yp += 1;
                }
                for (var y = 0; y < h; y++)
                {
                    output[y, x] = (sum[0] / div) << 16 | (sum[1] / div) << 8 | sum[2] / div;
                    sum[0] += r[vmin[y], x] - r[vmax[y], x];
                    sum[1] += g[vmin[y], x] - g[vmax[y], x];
                    sum[2] += b[vmin[y], x] - b[vmax[y], x];
                }
            }

            return output;
        }
    }
}