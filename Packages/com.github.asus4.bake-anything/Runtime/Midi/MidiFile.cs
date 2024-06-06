using System.Collections.ObjectModel;

namespace BakeAnything.Midi
{
    public enum MidiFileFormat : uint
    {
        SingleTrack = 0,
        MultiTrack = 1,
        MultiSong = 2
    }

    public class MidiFile
    {
        public MidiFileFormat Format { get; internal set; }
        public ReadOnlyCollection<MidiTrack> Tracks { get; internal set; }

        public MidiFile()
        {

        }

        public override string ToString()
        {
            return $"MidiFile: Tracks={Tracks.Count}";
        }
    }

    public class MidiTrack
    {
        public double Tempo;
        public uint Duration;
        public uint TicksPerQuarterNote = 96;
        public ReadOnlyCollection<MidiEvent> Events;
    }
}
