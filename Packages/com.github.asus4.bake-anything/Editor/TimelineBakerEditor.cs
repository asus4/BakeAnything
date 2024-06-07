using UnityEditor;
using UnityEngine;

namespace BakeAnything
{
    [CustomEditor(typeof(TimelineBaker), true)]
    public class TimelineBakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Bake"))
            {
                var timelineBaker = target as TimelineBaker;
                if (timelineBaker != null)
                {
                    Debug.Log($"Bake {timelineBaker.name}");
                }
            }
        }
    }
}
