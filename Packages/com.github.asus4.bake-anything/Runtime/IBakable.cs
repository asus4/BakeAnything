using System;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// Core interface that can be baked into a texture.
    /// </summary>
    public interface IBakable
    {
        /// <summary>
        /// The width of the texture
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the texture
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Bakes data into color array.
        /// </summary>
        /// <returns>Color array</returns>
        ReadOnlySpan<Color> Bake();
    }
}
