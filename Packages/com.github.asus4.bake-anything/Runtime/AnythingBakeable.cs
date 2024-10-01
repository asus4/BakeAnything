using System;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// A ScriptableObject wrapper of IBakable.
    /// </summary>
    public abstract class AnythingBakeable : ScriptableObject, IBakeable
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract ReadOnlySpan<Color> Bake();

        [SerializeField]
        private BakeOptions bakeOptions = BakeOptions.Default;
        public BakeOptions BakeOptions => bakeOptions;
    }
}
