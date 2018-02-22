using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public sealed class EntityEditorData : ISerializable
    {
        public string Path { get; set; }
        public EntityData Entity { get; private set; }

        // We have to do this because there is an issue in unity <3.6 causing first call to GetAssetPreview after loading to return null
        private Texture2D _previewTexture;
        public Texture2D PreviewTexture
        {
            get
            {
                if (_previewTexture == null)
                    SetPreviewTexture();
                return _previewTexture;
            }
        }

        public EntityEditorData(EntityData entity)
        {
            Entity = entity;
        }

        public void UpdateObject(EntityData ed)
        {
            Entity.Set(ed);
            Path = AssetDatabase.GetAssetPath(Entity.Prefab);
            SetPreviewTexture();
        }

        private void SetPreviewTexture()
        {
            if (Entity.Prefab == null) return;

#if UNITY_4_0
            var texture = AssetPreview.GetAssetPreview(Entity.Prefab);
#else
            var texture = EditorUtility.GetAssetPreview(Entity.Prefab);
#endif
            if (texture == null) return;

            _previewTexture = new Texture2D(128, 128); // Can't do texture.width because we don't always call this in main thread, preview is always 128

            var colors = texture.GetPixels();

            var color = new Color(49 / 255f, 49 / 255f, 49 / 255f); // Default background color we want to ignore

            for (int index = 0; index < colors.Length; index++)
            {
                if (colors[index] == color)
                {
                    colors[index] = Color.yellow;
                    colors[index].a = 0;
                }
            }

            _previewTexture.SetPixels(colors);
            _previewTexture.Apply();
        }

        public EntityEditorData(SerializationInfo info, StreamingContext context)
        {
            Path = info.GetString("Path");
            Entity = (EntityData)info.GetValue("Entity", typeof(EntityData));

            try
            {
                Entity.Prefab = (GameObject)AssetDatabase.LoadAssetAtPath(Path, typeof(GameObject));
            }
            catch (Exception) // If texture missing throw exception
            {
                throw new UnityException("No object found at " + Path);
            }

            SetPreviewTexture();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Path", Path);
            info.AddValue("Entity", Entity);
        }
    }
}
