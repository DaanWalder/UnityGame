using System;
namespace ProWorldSDK
{
    [Serializable]
    public class Erosion
    {
        static readonly int[] Permutations = new int[256];

        static Erosion()
        {
            var beginning = DateTime.Now;//these two are used to time our algorithm

            var random = new Random(beginning.Millisecond);

            //randomize the permutations table
            for (var i = 0; i < 256; ++i)
                Permutations[i] = i;

            for (var i = 0; i < 256; ++i)
            {
                var j = Permutations[i];
                var k = random.Next() & 255;
                Permutations[i] = Permutations[k];
                Permutations[k] = j;
            }
        }

        public float[,] ThermalErosion(float[,] mapBefore, int iterations)
        {
            var lowestX = 0;
            var lowestY = 0;

            var h = mapBefore.GetLength(0);
            var v = mapBefore.GetLength(1);

            float talus = 4.0f / h;

            var map = new float[h, v];
            Array.Copy(mapBefore, map, h * v);

            //for each iteration...
            for (var iterCount = 0; iterCount < iterations; ++iterCount)
            {
                //for each pixel...
                for (var x = 1; x < (h - 1); ++x)
                {
                    for (var y = 1; y < (v - 1); ++y)
                    {
                        var currentHeight = map[x, y];
                        var maxDif = -float.MaxValue;

                        for (var i = -1; i < 2; i += 2)
                        {
                            for (var j = -1; j < 2; j += 2)
                            {
                                var currentDifference = currentHeight - map[x + i, y + j];

                                if (currentDifference > maxDif)
                                {
                                    maxDif = currentDifference;

                                    lowestX = i;
                                    lowestY = j;
                                }
                            }
                        }

                        if (maxDif > talus)
                        {
                            var newHeight = currentHeight - maxDif / 2.0f;

                            map[x, y] = newHeight;
                            map[x + lowestX, y + lowestY] = newHeight;
                        }
                    }
                }
            }

            return map;
        }

        public float[,] HydraulicErosion(float[,] mapBefore, int iterations)
        {
            var lowestX = 0;
            var lowestY = 0;

            var h = mapBefore.GetLength(0);
            var v = mapBefore.GetLength(1);

            var waterMap = new float[h, v];
            const float rainAmount = 0.01f; //amount of rain dropped per pixel each iteration
            const float solubility = 0.01f; //how much sediment a unit of water will erode
            const float evaporation = 0.9f; //how much water evaporates from each pixel each iteration
            const float capacity = solubility; //how much sediment a unit of water can hold

            var map = new float[h, v];
            Array.Copy(mapBefore, map, h * v);

            //fill the water map with 0's
            for (var i = 0; i < h; ++i)
            {
                for (var j = 0; j < v; ++j)
                    waterMap[i, j] = 0.0f;
            }

            //for each iteration...
            for (var iterCount = 0; iterCount < iterations; ++iterCount)
            {
                //step 1: rain
                for (var x = 0; x < h; ++x)
                {
                    for (var y = 0; y < v; ++y)
                        waterMap[x, y] += rainAmount;
                }

                //step 2: erosion
                for (var x = 0; x < h; ++x)
                {
                    for (var y = 0; y < v; ++y)
                    {
                        map[x, y] -= waterMap[x, y] * solubility;
                    }
                }

                //step 3: movement
                for (var x = 1; x < (h - 1); ++x)
                {

                    for (var y = 1; y < (v - 1); ++y)
                    {
                        //find the lowest neighbor
                        var currentHeight = map[x, y] + waterMap[x, y];
                        var maxDif = -float.MaxValue; //temporary variables

                        for (var i = -1; i < 2; i += 1)
                        {
                            for (var j = -1; j < 2; j += 1)
                            {
                                var currentDifference = currentHeight - map[x + i, y + j] - waterMap[x + i, y + i];

                                if (currentDifference > maxDif)
                                {
                                    maxDif = currentDifference;

                                    lowestX = i;
                                    lowestY = j;
                                }
                            }
                        }

                        //now either do nothing, level off, or move all the water
                        if (maxDif > 0.0f)
                        {
                            //move it all...
                            if (waterMap[x, y] < maxDif)
                            {
                                waterMap[x + lowestX, y + lowestY] += waterMap[x, y];
                                waterMap[x, y] = 0.0f;
                            }
                            //level off...
                            else
                            {
                                waterMap[x + lowestX, y + lowestY] += maxDif / 2.0f;
                                waterMap[x, y] -= maxDif / 2.0f;
                            }
                        }
                    }
                }

                //step 4: evaporation / deposition
                for (var x = 0; x < h; ++x)
                {
                    for (var y = 0; y < v; ++y)
                    {
                        var waterLost = waterMap[x, y] * evaporation; //temporary variables

                        waterMap[x, y] -= waterLost;
                        map[x, y] += waterLost * capacity;
                    }
                }
            }

            return map;
        }

        public float[,] ImprovedErosion(float[,] mapBefore, int iterations)
        {
            //int x, y, i, j, iter_count;
            var lowestX = 0;
            var lowestY = 0;

            var h = mapBefore.GetLength(0);
            var v = mapBefore.GetLength(1);


            float talus = 12.0f / h;

            var map = new float[h, v];
            Array.Copy(mapBefore, map, h * v);

            //for each iteration...
            for (var iterCount = 0; iterCount < iterations; ++iterCount)
            {
                //for each pixel...
                for (var x = 1; x < (h - 1); ++x)
                {
                    for (var y = 1; y < (v - 1); ++y)
                    {
                        var currentHeight = map[x, y];
                        var maxDif = -float.MaxValue;

                        for (var i = -1; i < 2; i += 1)
                        {
                            for (var j = -1; j < 2; j += 1)
                            {
                                var currentDifference = currentHeight - map[x + i, y + j];

                                if (currentDifference > maxDif)
                                {
                                    maxDif = currentDifference;

                                    lowestX = i;
                                    lowestY = j;
                                }
                            }
                        }

                        if (maxDif > 0.0f && maxDif <= talus)
                        {
                            var newHeight = currentHeight - maxDif / 2.0f;

                            map[x, y] = newHeight;
                            map[x + lowestX, y + lowestY] = newHeight;
                        }
                    }
                }
            }

            return map;
        }
    }
}