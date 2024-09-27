using UnityEditor;
using UnityEngine;

namespace BakeAnything
{
    /// <summary>
    /// Adds a "Bake" button to the end of the inspector.
    /// </summary>
    [CustomEditor(typeof(AnythingBakable), true)]
    public class AnythingBakableEditor : Editor
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
            // replace path to .asset
            path = $"{path[..path.LastIndexOf('.')]}-baked.asset";
            BakeHelper.Bake(bakable, path);
        }
    }
}
