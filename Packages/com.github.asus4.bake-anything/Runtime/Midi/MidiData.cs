using System;
using System.Collections.ObjectModel;
using System.Text;

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

        public MidiFile() { }
        public override string ToString() => $"MidiFile: Tracks={Tracks.Count}";
    }

    public class MidiTrack
    {
        public uint Duration;
        public uint TicksPerQuarterNote = 96;
        public ReadOnlyCollection<MidiEvent> Events;
        public ReadOnlyCollection<MetaEvent> MetaEvents;
    }

    public interface ITimeEvent
    {
        uint Time { get; }
    }

    [Serializable]
    public struct MidiEvent : ITimeEvent
    {
        public uint time;
        public byte status;
        public byte data1;
        public byte data2;

        public readonly uint Time => time;

        public readonly bool IsCC => (status & 0xb0) == 0xb0;
        public readonly bool IsNote => (status & 0xe0) == 0x80;
        public readonly bool IsNoteOn => (status & 0xf0) == 0x90;
        public readonly bool IsNoteOff => (status & 0xf0) == 0x80;

        public override readonly string ToString() => $"[{time}: {status:X}, {data1}, {data2}]";
    }

    public enum MetaEventType : byte
    {
        SequenceNumber = 0x00,
        TextEvent = 0x01,
        CopyrightNotice = 0x02,
        SequenceOrTrackName = 0x03,
        InstrumentName = 0x04,
        LyricText = 0x05,
        MarkerText = 0x06,
        CuePoint = 0x07,
        MidiChannelPrefixAssignment = 0x20,
        EndOfTrack = 0x2F,
        TempoSetting = 0x51,
        SMPTEOffset = 0x54,
        TimeSignature = 0x58,
        KeySignature = 0x59,
        SequencerSpecificEvent = 0x7F,
    }

    [Serializable]
    public class MetaEvent : ITimeEvent
    {
        public uint time;
        public byte type;
        public byte[] data;

        public uint Time => time;
        public MetaEventType EventType => (MetaEventType)type;

        public string DataAsText => Encoding.ASCII.GetString(data);
        public double DataAsTempo => EventType switch
        {
            MetaEventType.TempoSetting => 60000000.0 / ((data[0] << 16) | (data[1] << 8) | data[2]),
            _ => throw new InvalidOperationException("This meta event is not a tempo setting event."),
        };

        public override string ToString()
        {
            return EventType switch
            {
                MetaEventType.TextEvent
                or MetaEventType.CopyrightNotice
                or MetaEventType.SequenceOrTrackName
                or MetaEventType.InstrumentName
                or MetaEventType.LyricText
                or MetaEventType.MarkerText
                    => $"[{time}: {EventType} {DataAsText}]",
                MetaEventType.TempoSetting => $"[{time}: {EventType} Tempo={DataAsTempo}]",
                _ => $"[{time}: {EventType}, {data.Length} bytes]",
            };
        }
    }
}
