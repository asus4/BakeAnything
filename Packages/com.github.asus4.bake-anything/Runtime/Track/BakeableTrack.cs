using System;
using System.Buffers;
using System.Diagnostics;
using Unity.Burst;
using Unity.Mathematics;
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
    public abstract class BakeableTrack : AnythingBakeable
    {
        [field: SerializeField, Min(1), Tooltip("Frames / Second")]
        public int Fps { get; internal set; } = 60;

        [field: SerializeField]
        public int MaxWidth { get; internal set; } = 8192;

        public override int Width => math.min(Frames, MaxWidth);
        public override int Height
        {
            get
            {
                int widthWraps = (int)math.ceil((double)Frames / MaxWidth);
                int channelWraps = (int)math.ceil(Channels / 4.0); // RGBA
                return widthWraps * channelWraps;
            }
        }

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

            int frames = Frames;
            int channels = Channels;
            var bufferArr = ArrayPool<float>.Shared.Rent(frames);
            var buffer = bufferArr.AsSpan(0, frames);

            // Bake all channels
            for (int channel = 0; channel < channels; channel++)
            {
                NotifyProgress((float)channel / channels, $"Baking channel {channel + 1} / {channels}...");

                // Bake single channel
                buffer.Fill(0);
                BakeChannel(buffer, channel);
                CopyBufferToChannel(buffer, PixelBuffer, Width, channel);
            }

            ClearProgress();

            ArrayPool<float>.Shared.Return(bufferArr);

            return PixelBuffer;
        }

        protected abstract void BakeChannel(Span<float> buffer, int channel);

        static unsafe void CopyBufferToChannel(Span<float> buffer, Span<Color> pixelBuffer, int width, int channel)
        {
            Log($"buffer.length={buffer.Length}, width={width}");

            if (buffer.Length < width)
            {
                Log("single-line");
                // Just copy single strait line.
                fixed (float* inPtr = buffer)
                fixed (Color* outPrt = pixelBuffer)
                {
                    CopyToChannel(inPtr, outPrt, buffer.Length, channel);
                }
                return;
            }

            Log("multi-line");
            // else
            for (int i = 0; i < buffer.Length; i++)
            {
                Color c = pixelBuffer[i];
                c[channel] = buffer[i];
                pixelBuffer[i] = c;
            }
        }

        [BurstCompile]
        private unsafe static void CopyToChannel(float* input, Color* output, int length, int channel)
        {
            for (int i = 0; i < length; i++)
            {
                float* f = (float*)&output[i] + channel;
                *f = input[i];
            }
        }

        [Conditional("UNITY_EDITOR")]
        static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR")]
        protected static void NotifyProgress(float progress, string message)
        {
#if UNITY_EDITOR
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
