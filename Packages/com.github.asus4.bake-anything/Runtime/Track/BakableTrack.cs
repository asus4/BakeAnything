using System;
using UnityEngine;

namespace BakeAnything
{

    /// <summary>
    /// A time-based track that can be baked into a texture.
    /// </summary>
    public abstract class BakableTrack : AnythingBakable
    {
        [field: SerializeField, Min(1), Tooltip("Frames / Second")]
        public int Fps { get; internal set; } = 60;

        [field: SerializeField]
        public int MaxWidth { get; internal set; } = 4096;

        public override int Width => Frames;
        public override int Height => Mathf.CeilToInt(Channels / 4f); // RGBA

        public abstract int Frames { get; }
        public abstract int Channels { get; }


        private Color[] buffer;
        private Span<Color> PixelBuffer
        {
            get
            {
                int length = Width * Height;
                if (buffer == null || buffer.Length != length)
                {
                    buffer = new Color[length];
                }
                return buffer;
            }
        }

        public override ReadOnlySpan<Color> Bake()
        {
            PixelBuffer.Fill(new Color(0, 0, 0, 0));
            Bake(PixelBuffer);
            return PixelBuffer;
        }

        protected abstract void Bake(Span<Color> pixels);
    }
}
