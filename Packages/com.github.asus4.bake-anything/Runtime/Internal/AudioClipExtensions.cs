using System;
using System.Buffers;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Assertions;

namespace BakeAnything.Internal
{
    internal static class AudioClipExtensions
    {
        public static void GetMonoData(this AudioClip clip, float[] data)
        {
            if (data.Length < clip.samples)
            {
                throw new ArgumentException("The length of samples must be greater than or equal to clip.samples.");
            }

            if (clip.channels == 1)
            {
                clip.GetData(data, 0);
            }
            else
            {
                var interleaved = ArrayPool<float>.Shared.Rent(clip.samples * clip.channels);
                clip.GetData(interleaved, 0);
                var interleavedSpan = interleaved.AsSpan(0, clip.samples * clip.channels);
                var dataSpan = data.AsSpan(0, clip.samples);
                Assert.AreEqual(interleavedSpan.Length, dataSpan.Length * clip.channels);

                MergeInterleavedChannels(interleavedSpan, dataSpan, clip.channels);
                ArrayPool<float>.Shared.Return(interleaved);
            }
        }

        [BurstCompile]
        static void MergeInterleavedChannels(Span<float> interleavedIn, Span<float> mergedOut, int channels)
        {
            for (int i = 0; i < mergedOut.Length; i++)
            {
                float sum = 0;
                for (int j = 0; j < channels; j++)
                {
                    sum += interleavedIn[i * channels + j];
                }
                mergedOut[i] = sum / channels;
            }
        }
    }
}
