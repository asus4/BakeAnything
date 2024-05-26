using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BakeAnything
{
    [CreateAssetMenu(fileName = "TimelineBaker", menuName = "ScriptableObject/Bake Anything/Timeline Baker")]
    public sealed class TimelineBaker : ScriptableObject
    {
        [field: SerializeField]
        private TimelineAsset timeline;
    }
}
