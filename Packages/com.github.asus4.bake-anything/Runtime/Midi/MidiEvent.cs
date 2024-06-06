namespace BakeAnything.Midi
{
    [System.Serializable]
    public struct MidiEvent
    {
        public uint Time;
        public byte Status;
        public byte Data1;
        public byte Data2;

        public readonly bool IsCC => (Status & 0xb0) == 0xb0;
        public readonly bool IsNote => (Status & 0xe0) == 0x80;
        public readonly bool IsNoteOn => (Status & 0xf0) == 0x90;
        public readonly bool IsNoteOff => (Status & 0xf0) == 0x80;

        public override readonly string ToString()
            => $"[{Time}: {Status:X}, {Data1}, {Data2}]";
    }
}
