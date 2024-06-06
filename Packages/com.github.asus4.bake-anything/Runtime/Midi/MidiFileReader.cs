using System;
using System.Collections.Generic;
using System.Text;

namespace BakeAnything.Midi
{
    /// <summary>
    /// SMF (Standard MIDI File) reader.
    /// https://www.ccarh.org/courses/253/handout/smf/
    /// </summary>
    public class MidiFileReader
    {
        private static readonly byte[] MThd = Encoding.ASCII.GetBytes("MThd");
        private static readonly byte[] MTrk = Encoding.ASCII.GetBytes("MTrk");

        public static MidiFile Read(byte[] data)
        {
            var reader = new MidiDataStreamReader(data);

            // Chunk type
            if (!reader.ReadBytes(4).SequenceEqual(MThd))
            {
                throw new FormatException("Can't find header chunk.");
            }
            // Chunk length
            if (reader.ReadBEUInt32() != 6u)
            {
                throw new FormatException("Length of header chunk must be 6.");
            }

            // Format
            MidiFileFormat format = (MidiFileFormat)reader.ReadBEUInt16();
            if (format == MidiFileFormat.MultiSong)
            {
                throw new NotSupportedException("Multi-song format is not supported.");
            }

            // Number of tracks
            uint trackCount = reader.ReadBEUInt16();
            UnityEngine.Debug.Log($"Track count: {trackCount}");

            // Ticks per quarter note
            var tpqn = reader.ReadBEUInt16();
            if ((tpqn & 0x8000u) != 0)
            {
                throw new FormatException("SMPTE time code is not supported.");
            }

            var track = new MidiTrack[trackCount];
            for (var i = 0; i < trackCount; i++)
            {
                UnityEngine.Debug.Log($"Reading track: {i}");
                track[i] = ReadTrack(reader, tpqn);
            }

            return new MidiFile
            {
                Format = format,
                Tracks = Array.AsReadOnly(track),
            };
        }

        private static MidiTrack ReadTrack(MidiDataStreamReader reader, uint tpqn)
        {
            // Chunk type
            if (!reader.ReadBytes(4).SequenceEqual(MTrk))
            {
                UnityEngine.Debug.Log("Can't find track chunk.");
                throw new FormatException("Can't find track chunk.");
            }

            UnityEngine.Debug.Log($"Start reading track: {reader.Position}");

            // Chunk length
            uint chunkEnd = reader.ReadBEUInt32();
            UnityEngine.Debug.Log($"Chunk end: {chunkEnd}");
            chunkEnd += reader.Position;

            var events = new List<MidiEvent>();
            uint ticks = 0u;
            byte status = 0;

            while (reader.Position < chunkEnd)
            {
                // Delta time
                ticks += reader.ReadMultiByteValue();

                // Status byte
                if ((reader.PeekByte() & 0x80u) != 0)
                {
                    status = reader.ReadByte();
                }

                switch (status)
                {
                    // TODO: Meta event
                    case 0xff:
                        reader.Advance(1);
                        reader.Advance(reader.ReadMultiByteValue());
                        break;
                    // TODO: SysEx event
                    case 0xf0:
                        // case 0xf7:
                        while (reader.ReadByte() != 0xf7) { }
                        break;
                    // MIDI event
                    default:
                        byte data1 = reader.ReadByte();
                        byte data2 = (status & 0xe0u) == 0xc0u ? (byte)0 : reader.ReadByte();
                        events.Add(new MidiEvent
                        {
                            Time = ticks,
                            Status = status,
                            Data1 = data1,
                            Data2 = data2,
                        });
                        break;
                }

            }

            // Quantize duration with bars.
            var bars = (ticks + tpqn * 4 - 1) / (tpqn * 4);

            return new MidiTrack()
            {
                Tempo = 120,
                Duration = bars * tpqn * 4,
                TicksPerQuarterNote = tpqn,
                Events = events.AsReadOnly(),
            };
        }
    }
}
