using System;

namespace BakeAnything
{
    public interface IBakable
    {
        int Width { get; }
        int Height { get; }
        ReadOnlySpan<float> Bake();
    }
}
