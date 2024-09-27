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

        public override int Frames => (int)Math.Ceiling(clip.samples / clip.channels / SamplesPerFrame);
        public override int Channels => mode switch
        {
            Mode.Loudness => 1,
            _ => 0,
        };

        protected override void Bake(Span<Color> pixels)
        {
            float[] samples = GetSamples(clip);
            if (normalize)
            {
                BurstCall.NormalizeAudio(samples);
            }
            switch (mode)
            {
                case Mode.Loudness:
                    BakeLoudness(samples, pixels);
                    return;
                default:
                    throw new NotSupportedException($"Unsupported mode: {mode}");
            };
        }

        private ReadOnlySpan<Color> BakeLoudness(float[] samples, Span<Color> buffer)
        {
            var frameSamples = SplitIntoFrames(samples, SamplesPerFrame);
            Assert.AreEqual(buffer.Length, frameSamples.Count);

            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < buffer.Length; i++)
            {
                double rms = BurstCall.ComputeRMS(frameSamples[i]);
                min = Math.Min(min, rms);
                max = Math.Max(max, rms);
                buffer[i] = new Color((float)rms, 0, 0, 1);
            }

            Debug.Log($"Loudness: min={min}, max={max}");
            return buffer;
        }

        private static List<ArraySegment<float>> SplitIntoFrames(float[] samples, double samplesPerFrame)
        {
            int frameCount = (int)Math.Ceiling(samples.Length / samplesPerFrame);
            var frames = new List<ArraySegment<float>>(frameCount);
            for (int i = 0; i < frameCount; i++)
            {
                int start = (int)(i * samplesPerFrame);
                int end = (int)Math.Min(samples.Length, (i + 1) * samplesPerFrame);
                frames.Add(new(samples, start, end - start));
            }
            return frames;
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
            // Merge interleaved channels
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
