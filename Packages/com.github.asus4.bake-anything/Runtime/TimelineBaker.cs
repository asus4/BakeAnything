using UnityEngine;

namespace BakeAnything
{
    [CreateAssetMenu(fileName = "TimelineBaker", menuName = "ScriptableObject/Bake Anything/Timeline Baker")]
    public sealed class TimelineBaker : ScriptableObject
    {
        [field: SerializeField]
        public int FrameRate { get; private set; } = 60;

        [field: SerializeField]
        public BakableTrack[] Tracks { get; private set; }
    }
}
