using System;
using System.Linq;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// Composite of multiple bakable tracks.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MultiBakableTrack",
        menuName = "ScriptableObject/Bake Anything/Multi Bakable Track"
    )]
    public sealed class MultiBakableTrack : BakeableTrack
    {
        [field: SerializeField]
        public BakeableTrack[] Tracks { get; private set; }

        public override int Frames => Tracks.Max(track => track.Frames);
        public override int Channels => Tracks.Sum(track => track.Channels);

        private void OnValidate()
        {
            // Ensure all tracks have the same settings.
            foreach (var track in Tracks)
            {
                track.Fps = Fps;
                track.MaxWidth = MaxWidth;
            }
        }

        internal override void BakeChannel(Span<float> buffer, int channel)
        {
            int offset = 0;
            foreach (var track in Tracks)
            {
                var count = track.Channels;
                if (channel < offset + count)
                {
                    track.BakeChannel(buffer, channel - offset);
                    return;
                }
                offset += count;
            }
        }
    }
}
