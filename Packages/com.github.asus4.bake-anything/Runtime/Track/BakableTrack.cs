using System;
using System.Buffers;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BakeAnything
{
    /// <summary>
    /// A time-based data that can be baked into a texture.
    /// (e.g. Animation, MIDI, Audio, etc.)
    /// </summary>
    public abstract class BakableTrack : AnythingBakable
    {
        [field: SerializeField, Min(1), Tooltip("Frames / Second")]
        public int Fps { get; internal set; } = 60;

        [field: SerializeField]
        public int MaxWidth { get; internal set; } = 8192;

        public override int Width => Frames;
        public override int Height => Mathf.CeilToInt(Channels / 4f); // RGBA

        public abstract int Frames { get; }
        public abstract int Channels { get; }


        private Color[] pixelBuffer;
        private Span<Color> PixelBuffer
        {
            get
            {
                int length = Width * Height;
                if (pixelBuffer == null || pixelBuffer.Length != length)
                {
                    pixelBuffer = new Color[length];
                }
                return pixelBuffer;
            }
        }


        public unsafe override ReadOnlySpan<Color> Bake()
        {
            // Clear the buffer
            PixelBuffer.Fill(new Color(0, 0, 0, 0));

            // Bake all channels
            var pool = ArrayPool<float>.Shared;
            int frameLength = Frames;
            int channelLength = Channels;
            var bufferArr = pool.Rent(frameLength);
            var buffer = bufferArr.AsSpan(0, frameLength);

            for (int ch = 0; ch < channelLength; ch++)
            {
                NotifyProgress((float)ch / channelLength, $"Baking channel {ch + 1} / {channelLength}...");

                buffer.Fill(0);
                BakeChannel(buffer, ch);

                fixed (float* inPtr = buffer)
                fixed (Color* outPrt = PixelBuffer)
                {
                    CopyToChannel(inPtr, outPrt, frameLength, ch);
                }
            }

            ClearProgress();

            pool.Return(bufferArr);

            return PixelBuffer;
        }

        protected abstract void BakeChannel(Span<float> buffer, int channel);

        [BurstCompile]
        private unsafe static void CopyToChannel(
            float* input,
            Color* output,
            int length,
            int channel)
        {
            for (int i = 0; i < length; i++)
            {
                Color c = output[i];
                c[channel] = input[i];
                output[i] = c;
            }
        }

        [Conditional("UNITY_EDITOR")]
        protected static void NotifyProgress(float progress, string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            EditorUtility.DisplayProgressBar("Bake Anything", message, progress);
#endif // UNITY_EDITOR
        }

        [Conditional("UNITY_EDITOR")]
        protected static void ClearProgress()
        {
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif // UNITY_EDITOR
        }
    }
}
