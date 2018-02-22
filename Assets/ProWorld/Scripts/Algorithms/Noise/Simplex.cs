// Simplex noise based off work by Stefan Gustavson which was placed in public domain
// http://webstaff.itn.liu.se/~stegu/simplexnoise/SimplexNoise.java

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Random = System.Random;

namespace ProWorldSDK
{
    [Serializable]
    public class SimplexNoise : INoise, ISerializable
    {
        public int Seed { get; private set; }
        public OutputMorph Morph = OutputMorph.Shift;

        // Simplex noise in 2D, 3D and 4D
        private static readonly Grad[] Grad3 = {
                                        new Grad(1, 1, 0), new Grad(-1, 1, 0), new Grad(1, -1, 0), new Grad(-1, -1, 0),
                                        new Grad(1, 0, 1), new Grad(-1, 0, 1), new Grad(1, 0, -1), new Grad(-1, 0, -1),
                                        new Grad(0, 1, 1), new Grad(0, -1, 1), new Grad(0, 1, -1), new Grad(0, -1, -1)
                                      };

        private static readonly Grad[] Grad4 = {new Grad(0,1,1,1),new Grad(0,1,1,-1),new Grad(0,1,-1,1),new Grad(0,1,-1,-1),
                   new Grad(0,-1,1,1),new Grad(0,-1,1,-1),new Grad(0,-1,-1,1),new Grad(0,-1,-1,-1),
                   new Grad(1,0,1,1),new Grad(1,0,1,-1),new Grad(1,0,-1,1),new Grad(1,0,-1,-1),
                   new Grad(-1,0,1,1),new Grad(-1,0,1,-1),new Grad(-1,0,-1,1),new Grad(-1,0,-1,-1),
                   new Grad(1,1,0,1),new Grad(1,1,0,-1),new Grad(1,-1,0,1),new Grad(1,-1,0,-1),
                   new Grad(-1,1,0,1),new Grad(-1,1,0,-1),new Grad(-1,-1,0,1),new Grad(-1,-1,0,-1),
                   new Grad(1,1,1,0),new Grad(1,1,-1,0),new Grad(1,-1,1,0),new Grad(1,-1,-1,0),
                   new Grad(-1,1,1,0),new Grad(-1,1,-1,0),new Grad(-1,-1,1,0),new Grad(-1,-1,-1,0)};

        // Skewing and unskewing factors for 2, 3, and 4 dimensions
        private static readonly float F2 = 0.5f * (float)(Math.Sqrt(3.0f) - 1.0f);
        private static readonly float G2 = (3.0f - (float)Math.Sqrt(3.0f)) / 6.0f;
        private const float F3 = 1.0f/3.0f;
        private const float G3 = 1.0f/6.0f;
        private static readonly float F4 = (float)(Math.Sqrt(5.0f) - 1.0f) / 4.0f;
        private static readonly float G4 = (5.0f - (float)Math.Sqrt(5.0f)) / 20.0f;

        private short[] _p = {151,160,137,91,90,15,
          131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
          190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
          88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
          77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
          102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
          135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
          5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
          223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
          129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
          251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
          49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
          138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180};

        // To remove the need for index wrapping, float the permutation table length
        private readonly short[] _perm = new short[512];
        private readonly short[] _permMod12 = new short[512];
        public SimplexNoise()
            : this(0)
        {
            
        }

        public SimplexNoise(int seed)
        {
            SetSeed(seed);
        }

        public void SetSeed(int seed)
        {
            Seed = seed;
            Setup();
        }

        private void Setup()
        {
            _p = GenerateRandomList(256, Seed).ToArray();

            for (var i = 0; i < 512; i++)
            {
                _perm[i] = _p[i & 255];
                _permMod12[i] = (short)(_perm[i] % 12);
            }
        }

        // This method is a *lot* faster than using (int)Math.floor(x)
        private static int Fastfloor(float x)
        {
            var xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }

        private static float Dot(Grad g, float x, float y)
        {
            return g.X * x + g.Y * y;
        }

        private static float Dot(Grad g, float x, float y, float z)
        {
            return g.X * x + g.Y * y + g.Z * z;
        }

        private static float Dot(Grad g, float x, float y, float z, float w)
        {
            return g.X * x + g.Y * y + g.Z * z + g.W * w;
        }

        // 2D simplex noise
        public float Noise(float x, float y)
        {
            float n0, n1, n2; // Noise contributions from the three corners
            // Skew the input space to determine which simplex cell we're in
            float s = (x + y) * F2; // Hairy factor for 2D
            int i = Fastfloor(x + s);
            int j = Fastfloor(y + s);
            float t = (i + j) * G2;
            float x00 = i - t; // Unskew the cell origin back to (x,y) space
            float y00 = j - t;
            float x0 = x - x00; // The x,y distances from the cell origin
            float y0 = y - y00;
            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0)
            {
                i1 = 1;
                j1 = 0;
            } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else
            {
                i1 = 0;
                j1 = 1;
            } // upper triangle, YX order: (0,0)->(0,1)->(1,1)
            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6
            float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
            float y2 = y0 - 1.0f + 2.0f * G2;
            // Work out the hashed gradient indices of the three simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int gi0 = _permMod12[ii + _perm[jj]];
            int gi1 = _permMod12[ii + i1 + _perm[jj + j1]];
            int gi2 = _permMod12[ii + 1 + _perm[jj + 1]];
            // Calculate the contribution from the three corners
            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(Grad3[gi0], x0, y0); // (x,y) of grad3 used for 2D gradient
            }
            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(Grad3[gi1], x1, y1);
            }
            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(Grad3[gi2], x2, y2);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the interval [-1,1].
            var con = 70.0f * (n0 + n1 + n2);

            switch (Morph)
            {
                default:
                    return Mathf.Clamp01(con * 0.5f + 0.5f);
                case OutputMorph.Clamp:
                    return Mathf.Clamp01(con);
                case OutputMorph.Abs:
                    return Mathf.Abs(con);
            }
        }
        // 3D simplex noise
        public float Noise(float x, float y, float z)
        {
            float n0, n1, n2, n3; // Noise contributions from the four corners
            // Skew the input space to determine which simplex cell we're in
            float s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
            int i = Fastfloor(x + s);
            int j = Fastfloor(y + s);
            int k = Fastfloor(z + s);
            float t = (i + j + k) * G3;
            float x00 = i - t; // Unskew the cell origin back to (x,y,z) space
            float y00 = j - t;
            float z00 = k - t;
            float x0 = x - x00; // The x,y,z distances from the cell origin
            float y0 = y - y00;
            float z0 = z - z00;
            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords
            if (x0 >= y0)
            {
                if (y0 >= z0)
                {
                    i1 = 1;
                    j1 = 0;
                    k1 = 0;
                    i2 = 1;
                    j2 = 1;
                    k2 = 0;
                } // X Y Z order
                else if (x0 >= z0)
                {
                    i1 = 1;
                    j1 = 0;
                    k1 = 0;
                    i2 = 1;
                    j2 = 0;
                    k2 = 1;
                } // X Z Y order
                else
                {
                    i1 = 0;
                    j1 = 0;
                    k1 = 1;
                    i2 = 1;
                    j2 = 0;
                    k2 = 1;
                } // Z X Y order
            }
            else
            {
                // x0<y0
                if (y0 < z0)
                {
                    i1 = 0;
                    j1 = 0;
                    k1 = 1;
                    i2 = 0;
                    j2 = 1;
                    k2 = 1;
                } // Z Y X order
                else if (x0 < z0)
                {
                    i1 = 0;
                    j1 = 1;
                    k1 = 0;
                    i2 = 0;
                    j2 = 1;
                    k2 = 1;
                } // Y Z X order
                else
                {
                    i1 = 0;
                    j1 = 1;
                    k1 = 0;
                    i2 = 1;
                    j2 = 1;
                    k2 = 0;
                } // Y X Z order
            }
            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.
            float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;
            float x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
            float y2 = y0 - j2 + 2.0f * G3;
            float z2 = z0 - k2 + 2.0f * G3;
            float x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
            float y3 = y0 - 1.0f + 3.0f * G3;
            float z3 = z0 - 1.0f + 3.0f * G3;
            // Work out the hashed gradient indices of the four simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int gi0 = _permMod12[ii + _perm[jj + _perm[kk]]];
            int gi1 = _permMod12[ii + i1 + _perm[jj + j1 + _perm[kk + k1]]];
            int gi2 = _permMod12[ii + i2 + _perm[jj + j2 + _perm[kk + k2]]];
            int gi3 = _permMod12[ii + 1 + _perm[jj + 1 + _perm[kk + 1]]];
            // Calculate the contribution from the four corners
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(Grad3[gi0], x0, y0, z0);
            }
            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(Grad3[gi1], x1, y1, z1);
            }
            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(Grad3[gi2], x2, y2, z2);
            }
            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Dot(Grad3[gi3], x3, y3, z3);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to stay just inside [-1,1]
            return 32.0f * (n0 + n1 + n2 + n3);
        }
        // 4D simplex noise, better simplex rank ordering method 2012-03-09
        public float Noise(float x, float y, float z, float w)
        {

            float n0, n1, n2, n3, n4; // Noise contributions from the five corners
            // Skew the (x,y,z,w) space to determine which cell of 24 simplices we're in
            float s = (x + y + z + w) * F4; // Factor for 4D skewing
            int i = Fastfloor(x + s);
            int j = Fastfloor(y + s);
            int k = Fastfloor(z + s);
            int l = Fastfloor(w + s);
            float t = (i + j + k + l) * G4; // Factor for 4D unskewing
            float x00 = i - t; // Unskew the cell origin back to (x,y,z,w) space
            float y00 = j - t;
            float z00 = k - t;
            float w00 = l - t;
            float x0 = x - x00; // The x,y,z,w distances from the cell origin
            float y0 = y - y00;
            float z0 = z - z00;
            float w0 = w - w00;
            // For the 4D case, the simplex is a 4D shape I won't even try to describe.
            // To find out which of the 24 possible simplices we're in, we need to
            // determine the magnitude ordering of x0, y0, z0 and w0.
            // Six pair-wise comparisons are performed between each possible pair
            // of the four coordinates, and the results are used to rank the numbers.
            int rankx = 0;
            int ranky = 0;
            int rankz = 0;
            int rankw = 0;
            if (x0 > y0) rankx++;
            else ranky++;
            if (x0 > z0) rankx++;
            else rankz++;
            if (x0 > w0) rankx++;
            else rankw++;
            if (y0 > z0) ranky++;
            else rankz++;
            if (y0 > w0) ranky++;
            else rankw++;
            if (z0 > w0) rankz++;
            else rankw++;
            // simplex[c] is a 4-vector with the numbers 0, 1, 2 and 3 in some order.
            // Many values of c will never occur, since e.g. x>y>z>w makes x<z, y<w and x<w
            // impossible. Only the 24 indices which have non-zero entries make any sense.
            // We use a thresholding to set the coordinates in turn from the largest magnitude.
            // Rank 3 denotes the largest coordinate.
            int i1 = rankx >= 3 ? 1 : 0;
            int j1 = ranky >= 3 ? 1 : 0;
            int k1 = rankz >= 3 ? 1 : 0;
            int l1 = rankw >= 3 ? 1 : 0;
            // Rank 2 denotes the second largest coordinate.
            int i2 = rankx >= 2 ? 1 : 0;
            int j2 = ranky >= 2 ? 1 : 0;
            int k2 = rankz >= 2 ? 1 : 0;
            int l2 = rankw >= 2 ? 1 : 0;
            // Rank 1 denotes the second smallest coordinate.
            int i3 = rankx >= 1 ? 1 : 0;
            int j3 = ranky >= 1 ? 1 : 0;
            int k3 = rankz >= 1 ? 1 : 0;
            int l3 = rankw >= 1 ? 1 : 0;
            // The fifth corner has all coordinate offsets = 1, so no need to compute that.
            float x1 = x0 - i1 + G4; // Offsets for second corner in (x,y,z,w) coords
            float y1 = y0 - j1 + G4;
            float z1 = z0 - k1 + G4;
            float w1 = w0 - l1 + G4;
            float x2 = x0 - i2 + 2.0f * G4; // Offsets for third corner in (x,y,z,w) coords
            float y2 = y0 - j2 + 2.0f * G4;
            float z2 = z0 - k2 + 2.0f * G4;
            float w2 = w0 - l2 + 2.0f * G4;
            float x3 = x0 - i3 + 3.0f * G4; // Offsets for fourth corner in (x,y,z,w) coords
            float y3 = y0 - j3 + 3.0f * G4;
            float z3 = z0 - k3 + 3.0f * G4;
            float w3 = w0 - l3 + 3.0f * G4;
            float x4 = x0 - 1.0f + 4.0f * G4; // Offsets for last corner in (x,y,z,w) coords
            float y4 = y0 - 1.0f + 4.0f * G4;
            float z4 = z0 - 1.0f + 4.0f * G4;
            float w4 = w0 - 1.0f + 4.0f * G4;
            // Work out the hashed gradient indices of the five simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int ll = l & 255;
            int gi0 = _perm[ii + _perm[jj + _perm[kk + _perm[ll]]]] % 32;
            int gi1 = _perm[ii + i1 + _perm[jj + j1 + _perm[kk + k1 + _perm[ll + l1]]]] % 32;
            int gi2 = _perm[ii + i2 + _perm[jj + j2 + _perm[kk + k2 + _perm[ll + l2]]]] % 32;
            int gi3 = _perm[ii + i3 + _perm[jj + j3 + _perm[kk + k3 + _perm[ll + l3]]]] % 32;
            int gi4 = _perm[ii + 1 + _perm[jj + 1 + _perm[kk + 1 + _perm[ll + 1]]]] % 32;
            // Calculate the contribution from the five corners
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(Grad4[gi0], x0, y0, z0, w0);
            }
            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(Grad4[gi1], x1, y1, z1, w1);
            }
            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(Grad4[gi2], x2, y2, z2, w2);
            }
            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
            if (t3 < 0) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Dot(Grad4[gi3], x3, y3, z3, w3);
            }
            float t4 = 0.6f - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
            if (t4 < 0) n4 = 0.0f;
            else
            {
                t4 *= t4;
                n4 = t4 * t4 * Dot(Grad4[gi4], x4, y4, z4, w4);
            }
            // Sum up and scale the result to cover the range [-1,1]
            return 27.0f * (n0 + n1 + n2 + n3 + n4);
        }

        public static short[] GenerateRandomList(short max, int seed = 0)
        {
            var list = new short[max];
            for (short i = 0; i < max; i++) list[i] = i;

            var random = RandomizeList(list.ToList(), seed).ToArray();

            return random;
        }

        public static List<T> RandomizeList<T>(List<T> inputList, int seed)
        {
            var rand = new Random(seed);
            var randomList = new List<T>();

            while (inputList.Count > 0)
            {
                var randomIndex = rand.Next(0, inputList.Count - 1); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList;
        }

        // Inner class to speed upp gradient computations
        // (array access is a lot slower than member access)
        private class Grad
        {
            public readonly float X, Y, Z, W;

            public Grad(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Grad(float x, float y, float z, float w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
        }

        public SimplexNoise(SerializationInfo info, StreamingContext context)
        {
            SetSeed(info.GetInt32("Seed"));
            Morph = (OutputMorph)info.GetValue("Morph", typeof(OutputMorph));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Seed", Seed);
            info.AddValue("Morph", Morph);
        }
    }
}