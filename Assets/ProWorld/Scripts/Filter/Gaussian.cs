using System;
using UnityEngine;

namespace ProWorldSDK
{
    public class Gaussian
    {
        public static float[,] Calculate1DSampleKernel(float deviation, int size)
        {
            var ret = new float[size,1];
            //var sum = 0f;
            var half = size/2;
            for (int i = 0; i < size; i++)
            {
                ret[i, 0] = 1/(Mathf.Sqrt(2*Mathf.PI)*deviation)*
                            Mathf.Exp(-(i - half)*(i - half)/(2*deviation*deviation));
                //sum += ret[i, 0];
            }
            return ret;
        }

        public static float[,] Calculate1DSampleKernel(float deviation)
        {
            var size = Mathf.CeilToInt(deviation*3)*2 + 1;
            return Calculate1DSampleKernel(deviation, size);
        }

        public static float[,] CalculateNormalized1DSampleKernel(float deviation)
        {
            return NormalizeMatrix(Calculate1DSampleKernel(deviation));
        }

        public static float[,] NormalizeMatrix(float[,] matrix)
        {
            var ret = new float[matrix.GetLength(0),matrix.GetLength(1)];
            float sum = 0;
            for (var i = 0; i < ret.GetLength(0); i++)
            {
                for (var j = 0; j < ret.GetLength(1); j++)
                    sum += matrix[i, j];
            }
            if (Math.Abs(sum - 0) > Mathf.Epsilon)
            {
                for (int i = 0; i < ret.GetLength(0); i++)
                {
                    for (int j = 0; j < ret.GetLength(1); j++)
                        ret[i, j] = matrix[i, j]/sum;
                }
            }
            return ret;
        }

        public static float[,] GaussianConvolution(float[,] matrix, float deviation)
        {
            var kernel = CalculateNormalized1DSampleKernel(deviation);
            var res1 = new float[matrix.GetLength(0),matrix.GetLength(1)];
            var res2 = new float[matrix.GetLength(0),matrix.GetLength(1)];
            //x-direction
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                    res1[i, j] = ProcessPoint(matrix, i, j, kernel, 0);
            }
            //y-direction
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                    res2[i, j] = ProcessPoint(res1, i, j, kernel, 1);
            }
            return res2;
        }

        private static float ProcessPoint(float[,] matrix, int x, int y, float[,] kernel, int direction)
        {
            float res = 0;
            int half = kernel.GetLength(0)/2;
            for (int i = 0; i < kernel.GetLength(0); i++)
            {
                int cox = direction == 0 ? x + i - half : x;
                int coy = direction == 1 ? y + i - half : y;
                if (cox >= 0 && cox < matrix.GetLength(0) && coy >= 0 && coy < matrix.GetLength(1))
                {
                    res += matrix[cox, coy]*kernel[i, 0];
                }
            }
            return res;
        }
    }
}