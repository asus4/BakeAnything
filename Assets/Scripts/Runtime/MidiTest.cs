using System.Collections;
using System.Collections.Generic;
using BakeAnything.Midi;
using UnityEngine;

public class MidiTest : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private TextAsset midiFile;

    private MidiSequencer sequencer;

    private void Start()
    {
        sequencer = new MidiSequencer(midiFile.bytes);
        audioSource.Play();
    }

    private void Update()
    {
        sequencer.Update(audioSource.time);
    }
}
