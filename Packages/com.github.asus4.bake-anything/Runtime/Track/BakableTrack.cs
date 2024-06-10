using System;
using UnityEngine;

namespace BakeAnything
{
    public abstract class BakableTrack : ScriptableObject, IBakable
    {
        [field: SerializeField]
        public int FrameRate { get; internal set; } = 60;

        public virtual int Frames { get; }
        public virtual int Channels { get; }

        public int Width => Frames;
        public int Height => Channels;
        public abstract ReadOnlySpan<Color> Bake();
    }
}
