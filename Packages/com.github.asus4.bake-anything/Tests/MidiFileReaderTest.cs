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

            Debug.Log($"midi: {midi}");
        }
    }
}
