using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BakeAnything
{
    [CreateAssetMenu(
        fileName = "AudioBakableTrack",
        menuName = "ScriptableObject/Bake Anything/Audio Bakable Track"
    )]
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

        private double SamplesPerFrame => (double)clip.frequency / Fps;

        public override int Frames => (int)Math.Ceiling(clip.samples / SamplesPerFrame);
        public override int Channels => mode switch
        {
            Mode.Loudness => 1,
            _ => 0,
        };

        protected override void BakeChannel(Span<float> buffer, int channel)
        {
            float[] samples = GetMonoSamples(clip);
            if (normalize)
            {
                BurstCall.NormalizeAudio(samples);
            }
            switch (mode)
            {
                case Mode.Loudness:
                    BakeLoudness(samples, buffer);
                    return;
                default:
                    throw new NotSupportedException($"Unsupported mode: {mode}");
            };
        }

        private void BakeLoudness(float[] samples, Span<float> buffer)
        {
            var frameSamples = SplitIntoFrames(samples, SamplesPerFrame);
            Assert.AreEqual(buffer.Length, frameSamples.Length);

            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (float)BurstCall.ComputeRMS(frameSamples[i]);
            }
            BurstCall.NormalizeMinMax(buffer);

            Debug.Log($"Loudness: min={min}, max={max}");
        }

        private static ArraySegment<float>[] SplitIntoFrames(float[] samples, double samplesPerFrame)
        {
            int frameCount = (int)Math.Ceiling(samples.Length / samplesPerFrame);
            var frames = new ArraySegment<float>[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                int start = (int)(i * samplesPerFrame);
                int end = (int)Math.Min(samples.Length, (i + 1) * samplesPerFrame);
                frames[i] = new(samples, start, end - start);
            }
            return frames;
        }

        private static float[] GetMonoSamples(AudioClip clip)
        {
            int channels = clip.channels;
            var interleaved = new float[clip.samples * channels];
            clip.GetData(interleaved, 0);
            if (channels == 1)
            {
                return interleaved;
            }

            // Merge interleaved channels
            var samples = new float[clip.samples];
            BurstCall.MergeInterleavedChannels(interleaved, samples, channels);
            return samples;
        }
    }
}
