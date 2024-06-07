using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BakeAnything.Tests
{
    public sealed class MidiFileReaderTest
    {
        [Test]
        public void TestMidiFormat()
        {
            const string filePath = "Packages/com.github.asus4.bake-anything/Tests/test.mid.bytes";
            Assert.IsTrue(File.Exists(filePath));

            var bytes = File.ReadAllBytes(filePath);
            var midi = Midi.MidiFileReader.Read(bytes);
            Assert.IsNotNull(midi);

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < midi.Tracks.Count; i++)
            {
                var track = midi.Tracks[i];
                sb.AppendLine($"Track {i}: Duration={track.Duration}");
                sb.AppendLine($"  Meta events: {track.MetaEvents.Count}");
                foreach (var e in track.MetaEvents)
                {
                    sb.AppendLine($"  {e}");
                }
                sb.AppendLine($"  Midi events: {track.Events.Count}");
                foreach (var e in track.Events)
                {
                    sb.AppendLine($"  {e}");
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}
