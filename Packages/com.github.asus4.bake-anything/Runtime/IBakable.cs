using System;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// Something that can be baked into a texture.
    /// </summary>
    public interface IBakable
    {
        int Width { get; }
        int Height { get; }
        ReadOnlySpan<Color> Bake();
    }

    /// <summary>
    /// A ScriptableObject wrapper of IBakable.
    /// </summary>
    public abstract class AnythingBakable : ScriptableObject, IBakable
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract ReadOnlySpan<Color> Bake();
    }

    /// <summary>
    /// A time-based track that can be baked into a texture.
    /// </summary>
    public abstract class BakableTrack : AnythingBakable
    {
        [field: SerializeField]
        public int FrameRate { get; internal set; } = 60;

        public override int Width => Channels;
        public override int Height => Frames;

        public abstract int Frames { get; }
        public abstract int Channels { get; }
    }
}
