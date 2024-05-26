using UnityEngine;

namespace BakeAnything.Editor
{
    [System.Serializable]
    public sealed class AudioTrack
    {
        [field: SerializeField]
        public AudioClip AudioClip { get; set; }
    }
}
