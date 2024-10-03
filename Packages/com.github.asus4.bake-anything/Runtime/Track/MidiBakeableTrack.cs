using System;
using UnityEngine;

namespace BakeAnything
{
    [CreateAssetMenu(
        fileName = "MidiBakeableTrack",
        menuName = "ScriptableObject/Bake Anything/Midi Track")]
    public class MidiBakeableTrack : BakeableTrack
    {
        [SerializeField]
        private TextAsset midiFile;

        public override int Frames => 0;
        public override int Channels => 0;

        internal override void BakeChannel(Span<float> buffer, int channel)
        {
            throw new NotImplementedException();
        }
    }
}
