using System;
using UnityEngine;

namespace BakeAnything
{
    [CreateAssetMenu(fileName = "AudioBakableTrack", menuName = "ScriptableObject/Bake Anything/Audio Bakable Track")]
    public sealed class AudioBakableTrack : BakableTrack
    {
        [field: SerializeField]
        public AudioClip Clip { get; private set; }

        public override int Frames => Clip.samples;
        public override int Channels => Clip.channels;

        public override ReadOnlySpan<float> Bake()
        {
            var samples = new float[Clip.samples * Clip.channels];
            Clip.GetData(samples, 0);
            return samples;
        }
    }
}
