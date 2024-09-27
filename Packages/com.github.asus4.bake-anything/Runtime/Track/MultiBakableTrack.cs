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

        private void OnValidate()
        {
            // Ensure all tracks have the same settings.
            foreach (var track in Tracks)
            {
                track.Fps = Fps;
                track.MaxWidth = MaxWidth;
            }
        }

        protected override void Bake(Span<Color> pixels)
        {
            throw new NotImplementedException();
        }
    }
}
