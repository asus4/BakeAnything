using System;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// An options for baking textures.
    /// </summary>
    [Serializable]
    public class BakeOptions
    {
        [field: SerializeField]
        public TextureFormat Format { get; set; }
        [field: SerializeField]
        public TextureWrapMode WrapMode { get; set; }
        [field: SerializeField]
        public FilterMode FilterMode { get; set; }

        public static readonly BakeOptions Default = new()
        {
            Format = TextureFormat.RGBAHalf,
            WrapMode = TextureWrapMode.Repeat,
            FilterMode = FilterMode.Point,
        };
    }
}
