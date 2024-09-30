using System.Collections;
using System.Collections.Generic;
using BakeAnything.Midi;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class MidiTest : MonoBehaviour
{
    [SerializeField]
    private TextAsset midiFile;

    private MidiSequencer sequencer;
    private PlayableDirector director;

    private void Start()
    {
        Application.runInBackground = true;
        director = GetComponent<PlayableDirector>();
        sequencer = new MidiSequencer(midiFile.bytes);
    }

    private void Update()
    {
        sequencer.Update(director.time);
    }
}
