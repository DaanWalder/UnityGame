using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProWorldSDK
{
    public static class Util
    {
        public static Texture2D White;

        public static void ApplyMapToTexture(ref Texture2D texture, float[,] color)
        {
            // Much more efficient to use SetPixels than loop through and SetPixel 
            if (color == null)
                throw new UnityException("Color data is empty");

            if (!texture)
                throw new UnityException("Texture doesn't exist");

            var width = color.GetLength(1);
            var height = color.GetLength(0);

            var colors = new Color[width*height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var c = color[y, x];
                    colors[y*width + x] = new Color(c, c, c);
                }
            }

            try
            {
                texture.SetPixels(colors);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void ApplyWaterToTexture(ref Texture2D texture, float[,] color, float level)
        {
            if (color == null)
                throw new UnityException("Color data is empty");

            if (!texture)
                throw new UnityException("Texture doesn't exist");

            var width = color.GetLength(1);
            var height = color.GetLength(0);

            var dif = 1 - level;

            var colors = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var c = color[y, x];
                    var col = c >= level
                                  ? new Color(0, -0.5f * ((c - level) / dif) + 1, 0) // level to 1, shift to 1 to 0.5
                                  : new Color(0, 0, ((c / level) + 1) / 2); // 0 to level, shift to 0.5 to 1

                    colors[y * width + x] = col;
                }
            }

            try
            {
                texture.SetPixels(colors);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void ApplyBoolMapToTexture(ref Texture2D texture, bool[,] data)
        {
            // Much more efficient to use SetPixels than loop through and SetPixel 
            if (data == null)
            {
                throw new UnityException("Data is empty");
            }

            var width = data.GetLength(1);
            var height = data.GetLength(0);
			
            var colors = new Color[width*height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    colors[y * width + x] = data[y, x] ? Color.white : Color.black;
                }
            }

            if (!texture || texture.width != width || texture.height != height)
            {
                texture = new Texture2D(width, height);
            }

            try
            {
                texture.SetPixels(colors);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        public static void ApplyBoolMapToTexture(ref Texture2D texture, bool[,] data, Color color)
        {
            // Much more efficient to use SetPixels than loop through and SetPixel 
            if (data == null)
            {
                throw new UnityException("Data is empty");
            }

            var width = data.GetLength(1);
            var height = data.GetLength(0);

            var colors = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    colors[y * width + x] = data[y, x] ? color : Color.black;
                }
            }

            if (!texture || texture.width != width || texture.height != height)
            {
                texture = new Texture2D(width, height);
            }

            try
            {
                texture.SetPixels(colors);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void ApplyIntMapToTexture(Texture2D texture, int[,] data)
        {
            // Much more efficient to use SetPixels than loop through and SetPixel 
            if (data == null)
                throw new UnityException("Data is empty");

            var width = data.GetLength(1);
            var height = data.GetLength(0);

            var colors = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var r = (data[y, x] & 0xff0000) >> 16;
                    var g = (data[y, x] & 0x00ff00) >> 8;
                    var b = (data[y, x] & 0x0000ff);
                    colors[y * width + x] = new Color(r, g, b);
                }
            }

            if (!texture)
            {
                texture = new Texture2D(width, height);
            }

            try
            {
                texture.SetPixels(colors);
                texture.Apply();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static Color[] DivideAreasHighlight(float[,] data, List<float> cutoffs, float waterLevel, int highlight)
        {
            var size = data.GetLength(0);
            var color = new Color[size * size];

            var lColor = new Color[cutoffs.Count];

            // Pregenerate colors for speed
            for (var index = 0; index < lColor.Length; index++)
            {
                var d = index / (float)cutoffs.Count;

                const int r = 0;
                var g = d * 0.75f + 0.25f;
                var b = d * 0.1875f + 0.0625f;

                lColor[index] = new Color(r, g, b);
            }

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    for (var i = 0; i < cutoffs.Count; i++)
                    {
                        if (data[y, x] <= cutoffs[i])
                        {
                            var c = lColor[i];

                            if (i == highlight)
                            {
                                c = Color.yellow;
                            }
                            else if (data[y, x] < waterLevel)
                            {
                                //c.g = c.g;
                                c.b = 1;
                            }

                            color[y * size + x] = c;

                            break;
                        }
                    }
                }
            }

            return color;
        }
        public static Color[] DivideAreas(float[,] data, List<float> cutoffs, float waterLevel)
        {
            var size = data.GetLength(0);
            var color = new Color[size * size];

            var lColor = new Color[cutoffs.Count];

            // Pregenerate colors for speed
            for (var index = 0; index < lColor.Length; index++)
            {
                var d = index / (float)cutoffs.Count;

                const int r = 0;
                var g = d * 0.75f + 0.25f;
                var b = d * 0.1875f + 0.0625f;

                lColor[index] = new Color(r, g, b);
            }

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    for (var i = 0; i < cutoffs.Count; i++)
                    {
                        if (data[y, x] <= cutoffs[i])
                        {
                            var c = lColor[i];

                            if (data[y, x] < waterLevel)
                            {
                                //c.g = c.g;
                                c.b = 1;
                            }

                            color[y * size + x] = c;

                            break;
                        }
                    }
                }
            }

            return color;
        }

        public static bool[,] GrayToBlackWhite(float[,] data)
        {
            var size = data.GetLength(0);

            var b = new bool[size,size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var d = data[y, x];

                    b[y, x] = d >= 0.5f;
                }
            }

            return b;
        }

        public static Texture2D BlackTexture(int size)
        {
            var texture = new Texture2D(size, size);
            var colors = new Color[size*size];

            for (var j = 0; j < size; j++)
            {
                for (var i = 0; i < size; i++)
                {
                    colors[j*size + i] = Color.black;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        /*public static Texture2D WhiteTexture(int size)
        {
            var texture = new Texture2D(size, size);
            var colors = new Color[size * size];

            for (var j = 0; j < size; j++)
            {
                for (var i = 0; i < size; i++)
                {
                    colors[j * size + i] = Color.white;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }*/

        public static T[,] SetArrayValue<T>(int size, T val)
        {
            var array = new T[size,size];

            for (var j = 0; j < size; j++)
            {
                for (var i = 0; i < size; i++)
                {
                    array[j, i] = val;
                }
            }

            return array;
        }

        // Based off work by http://www.codeproject.com/Articles/2122/Image-Processing-for-Dummies-with-C-and-GDI-Part-4
        public static T[,] ResizeArray<T>(T[,] oldArray, int size)
        {
            if (oldArray == null)
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                System.Reflection.MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                throw new UnityException("Array is null. Called from: " + methodBase.Name); // e.g.
            }

            //Check which method called another method
            var oldSizeX = oldArray.GetLength(1);
            var oldSizeY = oldArray.GetLength(0);

            var newArray = new T[size,size];

            // If old array has a length of zero, just return a new array of max size and default values
            if (oldSizeX == 0 || oldSizeY == 0)
                return newArray;

            var xFactor = oldSizeX/(float) size;
            var yFactor = oldSizeY/(float) size;

            for (var x = 0; x < size; ++x)
                for (var y = 0; y < size; ++y)
                {
                        newArray[y, x] = oldArray[(int) Math.Floor(y*yFactor), (int) Math.Floor(x*xFactor)];
                }

            return newArray;
        }

        public static T[] ResizeArray<T>(T[] oldArray, int size)
        {
            if (oldArray == null)
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                System.Reflection.MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                throw new UnityException("Array is null. Called from: " + methodBase.Name); // e.g.
            }

            if (oldArray.Length == 0)
                return new T[size*size];

            var dim = Mathf.Sqrt(oldArray.Length);
            if (Math.Abs(oldArray.Length%dim) > float.Epsilon)
            {
                throw new UnityException("Array isn't square");
            }
            var oldSize = (int) dim;

            //if (size > oldSize) return oldArray; // no need to scale up

            var newArray = new T[size*size];
            var nFactor = oldSize/(float) size;

            for (var x = 0; x < size; ++x)
                for (var y = 0; y < size; ++y)
                {
                    newArray[y*size + x] = oldArray[(int) Math.Floor(y*nFactor)*oldSize + (int) Math.Floor(x*nFactor)];
                }

            return newArray;
        }

        public static T[] MultiToArray<T>(T[,] oldArray)
        {
            var width = oldArray.GetLength(1);
            var height = oldArray.GetLength(0);

            var output = new T[width*height];

            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    output[j*width + i] = oldArray[j, i];
                }
            }

            return output;
        }

        public static T[,] ArrayToMulti<T>(T[] oldArray, int width, int height)
        {
            var output = new T[width,height];

            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    output[j, i] = oldArray[j*width + i];
                }
            }

            return output;
        }

        public static bool PointInMask(Vector2 point, float[,] mask)
        {
            var x = Mathf.RoundToInt(point.x);
            var y = Mathf.RoundToInt(point.y);
            var width = mask.GetLength(1);

            var last = false;
            var count = 0;

            for (var xx = 0; xx < width; xx++)
            {
                if (xx > x)
                    break;

                var m = mask[y, xx] > 0.5f;

                if (m != last)
                {
                    last = m;
                    count++;
                }
            }

            return count%2 != 0;
        }

        public static float[,] ConvertToGrayscale(Texture2D texture)
        {
            if (!texture) return new float[1,1];

            var width = texture.width;
            var height = texture.height;

            var colors = texture.GetPixels();

            var gray = new float[height,width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    gray[y, x] = colors[y*width + x].grayscale;
                }
            }

            return gray;
        }

        public static bool[,] ConvertToBlackWhite(Texture2D texture)
        {
            if (!texture) return new bool[1,1];

            var width = texture.width;
            var height = texture.height;

            var colors = texture.GetPixels();

            var bw = new bool[height,width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var gray = colors[y*width + x].grayscale;

                    bw[y, x] = gray >= 0.5f;
                }
            }

            return bw;
        }

        public static bool[,] AndArrays(bool[,] array1, bool[,] array2)
        {
            if (array1.GetLength(0) != array2.GetLength(0) || array1.GetLength(1) != array2.GetLength(1))
                throw new UnityException("Array sizes don't match");

            var width = array1.GetLength(1);
            var height = array1.GetLength(0);

            var array0 = new bool[height,width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    array0[y, x] = array1[y, x] && array2[y, x];
                }
            }

            return array0;
        }

        public static bool[,] FloatToBoolArray(float[,] data, float cutoff = 0.5f)
        {
            var width = data.GetLength(1);
            var height = data.GetLength(0);

            var b = new bool[height,width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    b[y, x] = data[y, x] >= cutoff;
                }
            }

            return b;
        }

        public static float[,] BoolToFloat(bool[,] data)
        {
            var width = data.GetLength(1);
            var height = data.GetLength(0);

            var f = new float[height,width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    f[y, x] = data[y, x] ? 1f : 0f;
                }
            }

            return f;
        }

        public static int UpperPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;

        }
        public static bool[,] LayerToBool(int[,] layerData, int layer)
        {
            var h = layerData.GetLength(0);
            var w = layerData.GetLength(1);

            var output = new bool[h,w];

            for(var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    output[y, x] = layerData[y, x] == layer;
                }
            }

            return output;
        }

        public static int RoundToLog(int val)
        {
            var sign = val < 0 ? -1 : 1;
            try
            {
                val = Mathf.Abs(val);
            }
            catch (OverflowException)
            {
                return val;
            }

            if (val < 100) return sign * val;          

            var pow = (int)Mathf.Log10(val) - 1;
            var closest = Mathf.Pow(10, pow);

            return sign * ((int)Mathf.Round(val / closest)) * (int)closest;
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}