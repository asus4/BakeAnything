using System;
using UnityEngine;

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

        public override ReadOnlySpan<Color> Bake()
        {
            // Clear the buffer
            PixelBuffer.Fill(new Color(0, 0, 0, 0));
            Bake(PixelBuffer);
            return PixelBuffer;
        }

        protected abstract void Bake(Span<Color> pixels);
    }
}
