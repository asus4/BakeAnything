using System;
using System.Linq;
using UnityEngine;

namespace BakeAnything
{
    [CreateAssetMenu(
        fileName = "MultiBakableTrack",
        menuName = "ScriptableObject/Bake Anything/Multi Bakable Track"
    )]
    public sealed class MultiBakableTrack : BakableTrack
    {
        [field: SerializeField]
        public BakableTrack[] Tracks { get; private set; }

        public override int Frames => Tracks.Max(track => track.Frames);
        public override int Channels => Tracks.Sum(track => track.Channels);

        public override ReadOnlySpan<Color> Bake()
        {
            throw new NotImplementedException();
        }

        private void OnValidate()
        {
            // Ensure all tracks have the same frame rate.
            foreach (var track in Tracks)
            {
                if (track.FrameRate != FrameRate)
                {
                    track.FrameRate = FrameRate;
                }
            }
        }
    }
}
