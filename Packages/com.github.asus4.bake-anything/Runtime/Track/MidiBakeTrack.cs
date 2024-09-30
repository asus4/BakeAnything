using System;
using UnityEngine;

namespace BakeAnything
{
    [CreateAssetMenu(fileName = "MidiBakableTrack", menuName = "ScriptableObject/Bake Anything/Midi Bakable Track")]
    public class MidiBakableTrack : BakableTrack
    {
        [SerializeField]
        private TextAsset midiFile;

        public override int Frames => 0;
        public override int Channels => 0;

        protected override void BakeChannel(Span<float> buffer, int channel)
        {
            throw new NotImplementedException();
        }
    }
}
