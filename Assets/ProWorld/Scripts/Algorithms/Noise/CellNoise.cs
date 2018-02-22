/* 
	Copyright 1994, 2002 by Steven Worley
	This software may be modified and redistributed without restriction
	provided this comment header remains intact in the source code.
	This code is provided with no warrantee, express or implied, for
	any purpose.
	
	A detailed description and application examples can be found in the
	1996 SIGGRAPH paper "A Cellular Texture Basis Function" and
	especially in the 2002 book "Texturing and Modeling, a Procedural
	Approach, 3rd edition." There is also extra information on the web
	site http://www.worley.com/cellular.html .
	
	If you do find interesting uses for this tool, and especially if
	you enhance it, please drop me an email at steve@worley.com.
	
	An implementation of the key cellular texturing basis
	function. This function is hardwired to return an average F_1 value
	of 1.0. It returns the <n> most closest feature point distances
	F_1, F_2, .. F_n the vector delta to those points, and a 32 bit
	seed for each of the feature points.  This function is not
	difficult to extend to compute alternative information such as
	higher order F values, to use the Manhattan distance metric, or
	other fun perversions.
	
	<at>    The input sample location. 
	<max_order>  Smaller values compute faster. < 5, read the book to extend it.
	<F>     The output values of F_1, F_2, ..F[n] in F[0], F[1], F[n-1]
	<delta> The output vector difference between the sample point and the n-th
			closest feature point. Thus, the feature point's location is the
			hit point minus this value. The DERIVATIVE of F is the unit
			normalized version of this vector.
	<ID>    The output 32 bit ID number which labels the feature point. This
			is useful for domain partitions, especially for coloring flagstone
			patterns.
			
	This implementation is tuned for speed in a way that any order > 5
	will likely have discontinuous artifacts in its computation of F5+.
	This can be fixed by increasing the internal points-per-cube
	density in the source code, at the expense of slower
	computation. The book lists the details of this tuning.
*/

/*
    * Edited by: Carl-Johan Rosén, Linköping University
    * Date: 2006-02-23
    * Contact: cj dot rosen at gmail dot com
    * 
    * This is the main class for generating cell noise. It uses the data 
    * structure CellDataStruct also in this package. The class implements 
    * one, two and three dimensional cell noise.
    */

/*
    * Edited by: Timothy Raines
    * Date: 2012-2013
    *  
    * Converted to c#
    * Changed structure to fit ProWorld
    * Added new distance methods
    * Built in various types
    */

using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public enum DistanceMethod
    {
        Euclidean,
        Manhattan,
        Chebyshev,
        Quadratic,
        Minkowski4,
        Minkowski5
    }

    [Serializable]
    public enum VoronoiType
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Difference21,
        Difference32,
    }

    [Serializable]
    public class CellNoise : INoise, ISerializable
    {

        /*
            * A hardwired lookup table to quickly determine how many feature points 
            * should be in each spatial cube. We use a table so we don't need to 
            * multiple slower tests. A random number indexed into this array will 
            * give an approximate Poisson distribution of mean density 2.5. Read 
            * the book for the long-winded explanation.
            */

        private readonly int[] _poissonCount = new[]
                                                   {
                                                       4, 3, 1, 1, 1, 2, 4, 2, 2, 2, 5, 1, 0, 2, 1, 2, 2, 0, 4, 3, 2, 1, 2,
                                                       1, 3, 2, 2, 4, 2, 2, 5, 1, 2, 3, 2, 2, 2, 2, 2, 3, 2, 4, 2, 5, 3, 2,
                                                       2, 2, 5, 3, 3, 5, 2, 1, 3, 3, 4, 4, 2, 3, 0, 4, 2, 2, 2, 1, 3, 2, 2,
                                                       2, 3, 3, 3, 1, 2, 0, 2, 1, 1, 2, 2, 2, 2, 5, 3, 2, 3, 2, 3, 2, 2, 1,
                                                       0, 2, 1, 1, 2, 1, 2, 2, 1, 3, 4, 2, 2, 2, 5, 4, 2, 4, 2, 2, 5, 4, 3,
                                                       2, 2, 5, 4, 3, 3, 3, 5, 2, 2, 2, 2, 2, 3, 1, 1, 5, 2, 1, 3, 3, 4, 3,
                                                       2, 4, 3, 3, 3, 4, 5, 1, 4, 2, 4, 3, 1, 2, 3, 5, 3, 2, 1, 3, 1, 3, 3,
                                                       3, 2, 3, 1, 5, 5, 4, 2, 2, 4, 1, 3, 4, 1, 5, 3, 3, 5, 3, 4, 3, 2, 2,
                                                       1, 1, 1, 1, 1, 2, 4, 5, 4, 5, 4, 2, 1, 5, 1, 1, 2, 3, 3, 3, 2, 5, 2,
                                                       3, 3, 2, 0, 2, 1, 1, 4, 2, 1, 3, 2, 1, 2, 2, 3, 2, 5, 5, 3, 4, 5, 5,
                                                       2, 4, 4, 5, 3, 2, 2, 2, 1, 4, 2, 3, 3, 4, 2, 5, 4, 2, 4, 2, 2, 2, 4,
                                                       5, 3, 2
                                                   };

        /*
        * This constant is manipulated to make sure that the mean value of 
        * F[0] is 1.0. This makes an easy natural 'scale' size of the cellular 
        * features.
        *
        * Its inverse is also kept, to improve speed.
        */
        private float _densityAdjustment;
        private float _densityAdjustmentInv;

        // Position in space/plane.
        private float[] _at;
        // Distance to closest points.
        private float[] _f;
        // Vector to closest points.
        private float[][] _delta; // TODO COULD WE USE
        // Uniqe id depending on the seed of the closest point.
        private long[] _id; // TODO COULD WE USE
        // Max number of 'closest' points.
        private int _maxOrder;

        // Dimensions.
        public int Dim { get; set; }
        // Distance measure type.
        public DistanceMethod DistType  { get; set; }
        // Distance measure type.
        public VoronoiType Type { get; set; }
        // Z - axis for 3D
        public float Z  { get; set; }

        public CellNoise()
        {
            Dim = 2;
            DistType = DistanceMethod.Euclidean;
            Type = VoronoiType.First;
        }

        public float Noise(float x, float y)
        {
            if (Dim > 3 || Dim < 2) return 0;

            // Set max order based off type
            switch (Type)
            {
                case VoronoiType.Difference21:
                    _maxOrder = 2;
                    break;
                case VoronoiType.Difference32:
                    _maxOrder = 3;
                    break;
                default:
                    _maxOrder = (int) Type + 1;
                    break;
            }

            _f = new float[_maxOrder];
            _delta = new float[_maxOrder][];
            for (var i = 0; i < _maxOrder; i++)
                _delta[i] = new float[Dim];
            _id = new long[_maxOrder];

            _at = new float[Dim];
            _at[0] = x;
            _at[1] = y;
            if (Dim > 2)
                _at[2] = Z;

            Run();

            switch (Type)
            {
                case VoronoiType.Difference21:
                    Run();
                    return _f[1] - _f[0];
                case VoronoiType.Difference32:
                    Run();
                    return _f[2] - _f[1];
                default:
                    Run();
                    return _f[(int)Type];
            }
        }

        private void Run()
        {
            if (Dim == 3)
            {
                /* Adjustment variable to make F[0] average at 1.0 when using 
                 * EUCLIDEAN distance in 3D.*/
                _densityAdjustment = 1f;// 0.398150f;
                _densityAdjustmentInv = 1.0f / _densityAdjustment;
                Noise3D();
            }
            else if (Dim == 2)
            {
                /* Adjustment variable to make F[0] average at 1.0 when using 
                 * EUCLIDEAN distance in 2D. */
                _densityAdjustment = 1f;// 0.294631f;
                _densityAdjustmentInv = 1.0f / _densityAdjustment;
                Noise2D();
            }
        }

        /*
        * Noise function for three dimensions. Coordinating the search on the 
        * above cube level. Deciding in which cubes to search.
        */

        private void Noise3D()
        {
            var newAt = new float[3];
            var intAt = new int[3];

            // initialize F
            for (var i = 0; i < _maxOrder; i++)
                _f[i] = 99999.9f;

            newAt[0] = _densityAdjustment*_at[0];
            newAt[1] = _densityAdjustment*_at[1];
            newAt[2] = _densityAdjustment*_at[2];

            intAt[0] = (int) Math.Floor(newAt[0]);
            intAt[1] = (int) Math.Floor(newAt[1]);
            intAt[2] = (int) Math.Floor(newAt[2]);

            /*
            * The center cube. It's very likely that the closest feature 
            * point will be found in this cube.
            */
            AddSamples(intAt[0], intAt[1], intAt[2], newAt);

            /*
            * We test if the cubes are even possible contributors by examining 
            * the combinations of the sum of the squared distances from the 
            * cube's lower or upper corners.
            */
            var x2 = newAt[0] - intAt[0];
            var y2 = newAt[1] - intAt[1];
            var z2 = newAt[2] - intAt[2];
            var mx2 = (1.0 - x2)*(1.0 - x2);
            var my2 = (1.0 - y2)*(1.0 - y2);
            var mz2 = (1.0 - z2)*(1.0 - z2);
            x2 *= x2;
            y2 *= y2;
            z2 *= z2;

            /*
            * The 6 facing neighbours of center cube. These are the closest 
            * and most likely to have a close feature point.
            */
            if (x2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1], intAt[2], newAt);
            if (y2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] - 1, intAt[2], newAt);
            if (z2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1], intAt[2] - 1, newAt);
            if (mx2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1], intAt[2], newAt);
            if (my2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] + 1, intAt[2], newAt);
            if (mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1], intAt[2] + 1, newAt);

            /*
            * The 12 edge cubes. These are next closest.
            */
            if (x2 + y2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] - 1, intAt[2], newAt);
            if (x2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1], intAt[2] - 1, newAt);
            if (y2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] - 1, intAt[2] - 1, newAt);
            if (mx2 + my2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] + 1, intAt[2], newAt);
            if (mx2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1], intAt[2] + 1, newAt);
            if (my2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] + 1, intAt[2] + 1, newAt);
            if (x2 + my2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] + 1, intAt[2], newAt);
            if (x2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1], intAt[2] + 1, newAt);
            if (y2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] - 1, intAt[2] + 1, newAt);
            if (mx2 + y2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] - 1, intAt[2], newAt);
            if (mx2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1], intAt[2] - 1, newAt);
            if (my2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] + 1, intAt[2] - 1, newAt);

            /*
            * The 8 corner cubes. 
            */
            if (x2 + y2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] - 1, intAt[2] - 1, newAt);
            if (x2 + y2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] - 1, intAt[2] + 1, newAt);
            if (x2 + my2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] + 1, intAt[2] - 1, newAt);
            if (x2 + my2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] + 1, intAt[2] + 1, newAt);
            if (mx2 + y2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] - 1, intAt[2] - 1, newAt);
            if (mx2 + y2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] - 1, intAt[2] + 1, newAt);
            if (mx2 + my2 + z2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] + 1, intAt[2] - 1, newAt);
            if (mx2 + my2 + mz2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] + 1, intAt[2] + 1, newAt);

            for (var i = 0; i < _maxOrder; i++)
            {
                _f[i] = (float)Math.Sqrt(_f[i])*_densityAdjustmentInv;
                _delta[i][0] *= _densityAdjustmentInv;
                _delta[i][1] *= _densityAdjustmentInv;
                _delta[i][2] *= _densityAdjustmentInv;
            }
        }




        /*
        * Noise function for two dimensions. Coordinating the search on the 
        * above square level. Deciding in which squares to search.
        */

        private void Noise2D()
        {
            var newAt = new float[2];
            var intAt = new int[2];

            // initialize F
            for (var i = 0; i < _maxOrder; i++)
                _f[i] = 99999.9f;

            newAt[0] = _densityAdjustment*_at[0];
            newAt[1] = _densityAdjustment*_at[1];

            intAt[0] = (int) Math.Floor(newAt[0]);
            intAt[1] = (int) Math.Floor(newAt[1]);

            /*
            * The center cube. It's very likely that the closest feature 
            * point will be found in this cube.
            */
            AddSamples(intAt[0], intAt[1], newAt);

            /*
            * We test if the cubes are even possible contributors by examining 
            * the combinations of the sum of the squared distances from the 
            * cube's lower or upper corners.
            */
            var x2 = newAt[0] - intAt[0];
            var y2 = newAt[1] - intAt[1];
            var mx2 = (1.0 - x2)*(1.0 - x2);
            var my2 = (1.0 - y2)*(1.0 - y2);
            x2 *= x2;
            y2 *= y2;

            /*
            * The 4 facing neighbours of center square. These are the closest 
            * and most likely to have a close feature point.
            */
            if (x2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1], newAt);
            if (y2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] - 1, newAt);
            if (mx2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1], newAt);
            if (my2 < _f[_maxOrder - 1]) AddSamples(intAt[0], intAt[1] + 1, newAt);

            /*
            * The 4 edge squares. These are next closest.
            */
            if (x2 + y2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] - 1, newAt);
            if (mx2 + my2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] + 1, newAt);
            if (x2 + my2 < _f[_maxOrder - 1]) AddSamples(intAt[0] - 1, intAt[1] + 1, newAt);
            if (mx2 + y2 < _f[_maxOrder - 1]) AddSamples(intAt[0] + 1, intAt[1] - 1, newAt);

            for (var i = 0; i < _maxOrder; i++)
            {
                _f[i] = (float)Math.Sqrt(_f[i])*_densityAdjustmentInv;
                _delta[i][0] *= _densityAdjustmentInv;
                _delta[i][1] *= _densityAdjustmentInv;
            }
        }

        /*
        * Generating the sample points in the grid
        * 3D
        */

        private void AddSamples(int xi, int yi, int zi, float[] at)
        {
            /*
            * Generating a random seed, based on the cube's ID number. The seed might be 
            * better if it were a nonlinear hash like Perlin uses for noise, but we do very 
            * well with this faster simple one.
            * Our LCG uses Knuth-approved constants for maximal periods.
            */
            long seed = (uint) (702395077*xi) + (uint) (915488749*yi) + (uint) (2120969693*zi);

            /* Number of feature points in this cube. */
            var count = _poissonCount[(int) (0xFF & (seed >> 24))];

            /* Churn the seed with good Knuth LCG. */
            seed = (uint) (1402024253*seed + 586950981);

            for (var j = 0; j < count; j++)
            {
                var thisID = seed;
                seed = (uint) (1402024253*seed + 586950981);

                /* Compute the 0..1 feature point location's xyz. */
                var fx = (seed + 0.5f)/4294967296.0f;
                seed = (uint) (1402024253*seed + 586950981);
                var fy = (seed + 0.5f)/4294967296.0f;
                seed = (uint) (1402024253*seed + 586950981);
                var fz = (seed + 0.5f)/4294967296.0f;
                seed = (uint) (1402024253*seed + 586950981);

                /* Delta from feature point to sample location. */
                float dx = xi + fx - at[0];
                float dy = yi + fy - at[1];
                float dz = zi + fz - at[2];

                /*
                * Distance computation
                */

                float d2;
                switch (DistType)
                {
                    case DistanceMethod.Manhattan:
                        d2 = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                        d2 *= d2;
                    break;
                    case DistanceMethod.Chebyshev:
                        d2 = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Abs(dz));
                        d2 *= d2;
                    break;
                    case DistanceMethod.Quadratic:
                        d2 = dx*dx + dy*dy + dz*dz + dx*dy + dx*dz + dy*dz;
                        d2 *= d2;
                    break;
                    case DistanceMethod.Minkowski4:
                        d2 = (float)Math.Pow(dx * dx * dx * dx + dy * dy * dy * dy + dz * dz * dz * dz, 0.25);
                        d2 *= d2;
                    break;
                    case DistanceMethod.Minkowski5:
                        const float p = 0.5f;
                        d2 = (float)Math.Pow(Math.Pow(Math.Abs(dx), p) + Math.Pow(Math.Abs(dy), p) + Math.Pow(Math.Abs(dz), Math.E), 1 / p);
                        d2 *= d2;
                    break;
                    default:  //DistanceMethod.Euclidean
                        d2 = dx * dx + dy * dy + dz * dz;
                    break;
                        /*
                    case DistanceMethod.Cityblock:
                        d2 = Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), Math.Abs(dz));
                        d2 *= d2;
                        break;
                    case DistanceMethod.Manhattan:
                        d2 = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                        d2 *= d2;
                        break;
                    case DistanceMethod.Quadratic:
                        d2 = dx*dx + dy*dy + dz*dz + dx*dy + dx*dz + dy*dz;
                        d2 *= d2;
                        break;
                    default:
                        d2 = dx*dx + dy*dy + dz*dz;
                        break;*/
                }


                /* Store points that are close enough to remember. */
                if (d2 < _f[_maxOrder - 1])
                {
                    var index = _maxOrder;
                    while (index > 0 && d2 < _f[index - 1])
                    {
                        index--;
                    }
                    for (var i = _maxOrder - 1; i-- > index;)
                    {
                        _f[i + 1] = _f[i];
                        _id[i + 1] = _id[i];
                        _delta[i + 1][0] = _delta[i][0];
                        _delta[i + 1][1] = _delta[i][1];
                        _delta[i + 1][2] = _delta[i][2];
                    }
                    _f[index] = d2;
                    _id[index] = thisID;
                    _delta[index][0] = dx;
                    _delta[index][1] = dy;
                    _delta[index][2] = dz;
                }
            }
        }



        /*
        * Generating the sample points in the grid
        * 2D
        */

        private void AddSamples(int xi, int yi, float[] at)
        {
            /*
            * Generating a random seed, based on the cube's ID number. The seed might be 
            * better if it were a nonlinear hash like Perlin uses for noise, but we do very 
            * well with this faster simple one.
            * Our LCG uses Knuth-approved constants for maximal periods.
            */
            long seed = ((uint) (702395077*xi) + (uint) (915488749*yi));

            /* Number of feature points in this cube. */
            var count = _poissonCount[(int) (0xFF & (seed >> 24))];

            /* Churn the seed with good Knuth LCG. */
            seed = (uint) (1402024253*seed + 586950981);

            /* Compute the 0..1 feature point location's xyz. */
            for (var j = 0; j < count; j++)
            {
                var thisID = seed;
                seed = (uint) (1402024253*seed + 586950981);
                var fx = (seed + 0.5f)/4294967296.0f;
                seed = (uint) (1402024253*seed + 586950981);
                var fy = (seed + 0.5f)/4294967296.0f;
                seed = (uint) (1402024253*seed + 586950981);

                /* Delta from feature point to sample location. */
                float dx = xi + fx - at[0];
                float dy = yi + fy - at[1];

                /*
                * Calculate distance.
                */
                float d2;
                switch (DistType)
                {
                    case DistanceMethod.Manhattan:
                        d2 = Math.Abs(dx) + Math.Abs(dy);
                        d2 *= d2;
                    break;
                    case DistanceMethod.Chebyshev:
                        d2 = Math.Max(Math.Abs(dx), Math.Abs(dy));
                        d2 *= d2;
                    break;
                    case DistanceMethod.Quadratic:
                        d2 = dx*dx + dy*dy + dx*dy;
                        d2 *= d2;
                    break;
                    case DistanceMethod.Minkowski4:
                        d2 = (float)Math.Pow(dx * dx * dx * dx + dy * dy * dy * dy, 0.25);
                        d2 *= d2;
                    break;
                    case DistanceMethod.Minkowski5:
                        const float p = 0.5f;
                        d2 = (float)Math.Pow(Math.Pow(Math.Abs(dx), p) + Math.Pow(Math.Abs(dy), p), 1 / p);
                        d2 *= d2;
                    break;
                    default: // DistanceMethod.Length
                        d2 = dx * dx + dy * dy;
                    break;

                    /*case DistanceMethod.Cityblock:
                        d2 = Math.Max(Math.Abs(dx), Math.Abs(dy));
                        d2 *= d2;
                        break;
                    case DistanceMethod.Manhattan:
                        d2 = Math.Abs(dx) + Math.Abs(dy);
                        d2 *= d2;
                        break;
                    case DistanceMethod.Quadratic:
                        d2 = dx*dx + dy*dy + dx*dy;
                        d2 *= d2;
                        break;
                    default: // Length2
                        d2 = dx*dx + dy*dy;
                        break;*/
                }

                /* Store points that are close enough to remember. */
                if (d2 < _f[_maxOrder - 1])
                {
                    var index = _maxOrder;
                    while (index > 0 && d2 < _f[index - 1])
                    {
                        index--;
                    }
                    int i;
                    for (i = _maxOrder - 1; i-- > index;)
                    {
                        _f[i + 1] = _f[i];
                        _id[i + 1] = _id[i];
                        _delta[i + 1][0] = _delta[i][0];
                        _delta[i + 1][1] = _delta[i][1];
                    }
                    _f[index] = d2;
                    _id[index] = thisID;
                    _delta[index][0] = dx;
                    _delta[index][1] = dy;
                }
            }
        }

        public CellNoise(SerializationInfo info, StreamingContext context)
        {
            Dim = info.GetInt32("Dim");
            DistType = (DistanceMethod)info.GetValue("DistType", typeof(DistanceMethod));
            Type = (VoronoiType)info.GetValue("Type", typeof(VoronoiType));
            Z = (float) info.GetValue("Z", typeof (float));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Dim", Dim);
            info.AddValue("DistType", DistType);
            info.AddValue("Type", Type);
            info.AddValue("Z", DistType);
        }
    }
}