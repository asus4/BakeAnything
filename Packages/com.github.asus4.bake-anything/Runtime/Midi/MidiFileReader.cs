using System;
using System.Collections.Generic;
using System.Text;

namespace BakeAnything.Midi
{
    /// <summary>
    /// SMF (Standard MIDI File) reader.
    /// Originally developed by Keijiro Takahashi (Unlicense)
    /// https://github.com/keijiro/MidiAnimationTrack/blob/master/LICENSE
    ///
    /// References for SMF:
    /// https://www.ccarh.org/courses/253/handout/smf/
    /// http://midi.teragonaudio.com/tech/midifile.htm
    /// </summary>
    public static class MidiFileReader
    {
        private static byte[] MThd => Encoding.ASCII.GetBytes("MThd");
        private static byte[] MTrk => Encoding.ASCII.GetBytes("MTrk");

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

            // Ticks per quarter note
            var tpqn = reader.ReadBEUInt16();
            if ((tpqn & 0x8000u) != 0)
            {
                throw new FormatException("SMPTE time code is not supported.");
            }

            var track = new MidiTrack[trackCount];
            for (var i = 0; i < trackCount; i++)
            {
                track[i] = ReadTrack(reader, tpqn);
            }

            return new MidiFile
            {
                Format = format,
                TicksPerQuarterNote = tpqn,
                Tracks = Array.AsReadOnly(track),
            };
        }

        private static MidiTrack ReadTrack(MidiDataStreamReader reader, uint tpqn)
        {
            // Chunk type
            if (!reader.ReadBytes(4).SequenceEqual(MTrk))
            {
                throw new FormatException("Can't find track chunk.");
            }

            // Chunk length
            uint chunkEnd = reader.ReadBEUInt32();
            chunkEnd += reader.Position;

            List<MidiEvent> midiEvents = new();
            List<MetaEvent> metaEvents = new();

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
                    case 0xff:
                        // Meta event
                        // meta_event = 0xFF + <meta_type> + <v_length> + <event_data_bytes>
                        {
                            byte type = reader.ReadByte();
                            uint length = reader.ReadMultiByteValue();
                            byte[] data = reader.ReadBytes((int)length).ToArray();
                            metaEvents.Add(new MetaEvent
                            {
                                time = ticks,
                                type = type,
                                data = data,
                            });
                        }
                        break;
                    case 0xf0:
                    case 0xf7:
                        // TODO: SysEx event
                        while (reader.ReadByte() != 0xf7) { }
                        break;
                    default:
                        // MIDI event
                        {
                            byte data1 = reader.ReadByte();
                            byte data2 = (status & 0xe0u) == 0xc0u ? (byte)0 : reader.ReadByte();
                            midiEvents.Add(new MidiEvent
                            {
                                time = ticks,
                                status = status,
                                data1 = data1,
                                data2 = data2,
                            });
                        }
                        break;
                }
            }

            // Quantize duration with bars.
            uint bars = (ticks + tpqn * 4 - 1) / (tpqn * 4);

            return new MidiTrack()
            {
                Duration = bars * tpqn * 4,
                TicksPerQuarterNote = tpqn,
                Events = midiEvents.AsReadOnly(),
                MetaEvents = metaEvents.AsReadOnly(),
            };
        }
    }
}
