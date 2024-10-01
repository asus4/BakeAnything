using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace BakeAnything.Internal
{
    /// <summary>
    /// Unsafe methods that calls Burst functions.
    /// </summary>
    internal static unsafe class BurstCall
    {
        /// <summary>
        /// max(abs(arr[n]))
        /// </summary>
        [BurstCompile]
        internal static float AbsMax([ReadOnly] float* arr, int length)
        {
            float max = float.MinValue;
            for (int i = 0; i < length; i++)
            {
                max = math.max(max, math.abs(arr[i]));
            }
            return max;
        }

        /// <summary>
        /// arr[n] = arr[n] * scalar
        /// </summary>
        [BurstCompile]
        internal static void Mul(float* arr, int length, float scalar)
        {
            for (int i = 0; i < length; i++)
            {
                arr[i] *= scalar;
            }
        }

        /// <summary>
        /// sum(arr[n] * arr[n])
        /// </summary>
        [BurstCompile]
        internal static double SqrSum([ReadOnly] float* arr, int length)
        {
            double sqrSum = 0.0;
            for (int i = 0; i < length; i++)
            {
                sqrSum += arr[i] * arr[i];
            }
            return sqrSum;
        }

        /// <summary>
        /// min(arr[n])
        /// max(arr[n]) 
        /// </summary>
        [BurstCompile]
        internal static void MinMax(
            [ReadOnly] float* arr, int length,
            out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < length; i++)
            {
                min = math.min(min, arr[i]);
                max = math.max(max, arr[i]);
            }
        }

        /// <summary>
        /// arr[n] = (arr[n] + offset) * scale
        /// </summary>
        [BurstCompile]
        internal static void AddMul(float* arr, int length, float offset, float scale)
        {
            for (int i = 0; i < length; i++)
            {
                arr[i] = (arr[i] + offset) * scale;
            }
        }

        internal static void NormalizeMinMax(Span<float> arr)
        {
            fixed (float* pArr = arr)
            {
                MinMax(pArr, arr.Length, out float min, out float max);
                if (min == max)
                {
                    return; // blank audio
                }
                AddMul(pArr, arr.Length, offset: -min, scale: 1f / (max - min));
            }
        }

        internal static void NormalizeAudio(Span<float> arr)
        {
            fixed (float* pArr = arr)
            {
                float max = AbsMax(pArr, arr.Length);
                if (max == 0)
                {
                    return; // blank audio
                }
                float scale = 1f / max;
                Mul(pArr, arr.Length, scale);
            }
        }

        internal static double ComputeRMS(Span<float> arr)
        {
            fixed (float* pArr = arr)
            {
                return math.sqrt(SqrSum(pArr, arr.Length) / arr.Length);
            }
        }
    }
}
