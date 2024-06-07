using UnityEditor;
using UnityEngine;

namespace BakeAnything
{
    [CustomEditor(typeof(BakableTrack), true)]
    public class BakableTrackEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Bake"))
            {
                var bakable = target as IBakable;
                if (bakable != null)
                {
                    BakeHelper.Bake(target.name, bakable);
                }
            }
        }
    }
}
