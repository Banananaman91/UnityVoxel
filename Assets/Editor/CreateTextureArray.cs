using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateTextureArray : ScriptableWizard
    {
#pragma warning disable 0649
        [SerializeField] private Texture2D[] _textures;
#pragma warning restore 0649

        [MenuItem("Assets/Create/Texture Array")]
        static void CreateWizard()
        {
            DisplayWizard<CreateTextureArray>("Create Texture Array", "Create");
        }

        public void OnWizardCreate()
        {
            if (_textures.Length == 0) return;

            string path = EditorUtility.SaveFilePanelInProject("Save Texture Array",
                "Texture Array",
                "asset",
                "Save Texture Array");

            if (path.Length == 0) return;

            var t = _textures[0];

            var ta = new Texture2DArray(t.width,
                t.height,
                _textures.Length,
                t.format,
                t.mipmapCount > 1);

            ta.anisoLevel = t.anisoLevel;
            ta.filterMode = t.filterMode;
            ta.wrapMode = t.wrapMode;

            for (int i = 0; i < _textures.Length; i++)
            {
                for (int j = 0; j < t.mipmapCount; j++)
                {
                    Graphics.CopyTexture(_textures[i], 0, j, ta, i, j);
                }
            }

            AssetDatabase.CreateAsset(ta, path);
        }
    }
}

