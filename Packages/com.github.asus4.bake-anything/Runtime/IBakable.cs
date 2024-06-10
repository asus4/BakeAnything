using System;
using UnityEngine;

namespace BakeAnything
{
    public interface IBakable
    {
        int Width { get; }
        int Height { get; }
        ReadOnlySpan<Color> Bake();
    }
}
