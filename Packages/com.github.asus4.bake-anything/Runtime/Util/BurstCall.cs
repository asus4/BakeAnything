using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace BakeAnything
{
    /// <summary>
    /// Unsafe methods that calls Burst functions.
    /// </summary>
    public static unsafe class BurstCall
    {
        [BurstCompile]
        private static float AbsMax([ReadOnly] float* arr, int length)
        {
            float max = float.MinValue;
            for (int i = 0; i < length; i++)
            {
                max = Math.Max(max, Math.Abs(arr[i]));
            }
            return max;
        }

        [BurstCompile]
        private static void Mul(float* arr, int length, float scalar)
        {
            for (int i = 0; i < length; i++)
            {
                arr[i] *= scalar;
            }
        }

        [BurstCompile]
        private static double SqrSum(float* arr, int length)
        {
            double sqrSum = 0.0;
            for (int i = 0; i < length; i++)
            {
                sqrSum += arr[i] * arr[i];
            }
            return sqrSum;
        }

        public static void NormalizeAudio(Span<float> arr)
        {
            fixed (float* pArr = arr)
            {
                float max = AbsMax(pArr, arr.Length);
                if (max == 0)
                {
                    return; // blank audio
                }
                float rMax = 1f / max;
                Mul(pArr, arr.Length, rMax);
            }
        }

        public static double ComputeRMS(Span<float> arr)
        {
            fixed (float* pArr = arr)
            {
                return Math.Sqrt(SqrSum(pArr, arr.Length) / arr.Length);
            }
        }
    }
}
