using System;
using UnityEngine;

namespace BakeAnything
{
    [CreateAssetMenu(fileName = "AudioBakableTrack", menuName = "ScriptableObject/Bake Anything/Audio Bakable Track")]
    public sealed class AudioBakableTrack : BakableTrack
    {
        public enum Mode
        {
            Loudness,
        }

        [SerializeField]
        private AudioClip clip;

        [SerializeField]
        private Mode mode;

        [SerializeField]
        private bool normalize = true;

        public override int Frames => Mathf.CeilToInt(clip.length * FrameRate);
        public override int Channels => mode switch
        {
            Mode.Loudness => 1,
            _ => 0,
        };

        public override ReadOnlySpan<Color> Bake()
        {
            float[] samples = GetSamples(clip);
            if (normalize)
            {
                Normalize(samples);
            }
            throw new NotImplementedException();
        }

        private static void Normalize(Span<float> samples)
        {
            float max = float.MinValue;
            for (int i = 0; i < samples.Length; i++)
            {
                max = Mathf.Max(max, Mathf.Abs(samples[i]));
            }
            if (max == 0)
            {
                return;
            }
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= max;
            }
        }

        private static float[] GetSamples(AudioClip clip)
        {
            int channels = clip.channels;
            var allChannels = new float[clip.samples * channels];
            clip.GetData(allChannels, 0);
            if (channels == 1)
            {
                return allChannels;
            }
            // Merge channels
            var samples = new float[clip.samples / channels];
            for (int i = 0; i < samples.Length; i++)
            {
                float sum = 0;
                for (int j = 0; j < channels; j++)
                {
                    sum += allChannels[i * channels + j];
                }
                samples[i] = sum / channels;
            }
            return samples;
        }
    }
}
