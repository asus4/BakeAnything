using System;
using System.Buffers;
using System.Collections.Generic;
using BakeAnything.Internal;
using UnityEngine;
using UnityEngine.Assertions;

namespace BakeAnything
{
    /// <summary>
    /// Bakes analyzed audio data
    /// </summary>
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
            _ => throw new NotImplementedException(),
        };

        protected override void BakeChannel(Span<float> buffer, int channel)
        {
            var samples = new float[clip.samples];
            clip.GetMonoData(samples);

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
                    throw new NotImplementedException($"Unsupported mode: {mode}");
            };
        }

        private void BakeLoudness(float[] samples, Span<float> buffer)
        {
            var frameSamples = SplitIntoFrames(samples, SamplesPerFrame);
            Assert.AreEqual(buffer.Length, frameSamples.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (float)BurstCall.ComputeRMS(frameSamples[i]);
            }
            if (normalize)
            {
                BurstCall.NormalizeMinMax(buffer);
            }
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
    }
}
