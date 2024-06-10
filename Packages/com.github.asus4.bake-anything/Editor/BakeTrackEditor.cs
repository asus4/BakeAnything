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
                Bake();
            }
        }

        private void Bake()
        {
            if (target is not IBakable bakable)
            {
                throw new System.InvalidOperationException($"target is not IBakable");
            }
            string path = AssetDatabase.GetAssetPath(target);
            // replace path to .exr
            path = $"{path[..path.LastIndexOf('.')]}.exr";
            BakeHelper.Bake(bakable, path);
        }
    }
}
