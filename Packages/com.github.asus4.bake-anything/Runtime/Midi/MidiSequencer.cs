using System.Linq;
using UnityEngine;

namespace BakeAnything.Midi
{
    public sealed class MidiSequencer
    {
        private readonly MidiFile midi;
        private readonly uint ticksPerQuarterNote = 480;
        private uint previousTicks;
        private double tempo = 120; // default: 120bpm

        public MidiSequencer(MidiFile midi)
        {
            this.midi = midi;
            ticksPerQuarterNote = midi.TicksPerQuarterNote;

            // Find the first tempo meta event
            MetaEvent tempoEvent = midi.Tracks
                .SelectMany(t => t.MetaEvents)
                .Where(e => e.EventType == MetaEventType.TempoSetting)
                .FirstOrDefault();
            tempo = tempoEvent != null
                ? tempoEvent.DataAsTempo
                : 120; // default: 120bpm
        }

        public MidiSequencer(byte[] bytes) : this(MidiFileReader.Read(bytes))
        {
        }

        public void Update(double time)
        {
            uint ticks = SecondToTicks(time);
            if (ticks < previousTicks)
            {
                // Reset the sequencer
                previousTicks = 0;
            }

            foreach (var track in midi.Tracks)
            {
                foreach (var e in track.Events)
                {
                    if (e.Time >= previousTicks && e.Time < ticks)
                    {
                        // Process the event
                        Debug.Log($"Event: {e}");
                    }
                }
            }

            previousTicks = ticks;
        }

        private uint SecondToTicks(double time)
        {
            return (uint)(time * tempo / 60 * ticksPerQuarterNote);
        }

        private double TicksToSecond(uint ticks)
        {
            return ticks * 60.0 / (tempo * ticksPerQuarterNote);
        }
    }
}
