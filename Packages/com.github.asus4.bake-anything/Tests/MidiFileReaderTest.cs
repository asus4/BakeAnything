using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
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

            using var stream = File.OpenRead(filePath);

            Debug.Log($"load file at {filePath}");
        }
    }
}
