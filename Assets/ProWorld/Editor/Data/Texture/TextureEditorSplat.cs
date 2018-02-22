using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class TextureEditorSplat : ISerializable
    {
        public string Path { get; set; }
        public Color PreviewColor { get; private set; }
        public TextureSplat Splat { get; private set; }

        public TextureEditorSplat(TextureSplat splat)
        {
            Splat = splat;
        }

        public TextureEditorSplat(SerializationInfo info, StreamingContext context)
        {
            Path = info.GetString("Path");
            PreviewColor = ((SerializableColor)info.GetValue("PreviewColor", typeof(SerializableColor))).ToColor();
            Splat = (TextureSplat)info.GetValue("Splat", typeof(TextureSplat));

            try
            {
                Splat.Texture = (Texture2D)AssetDatabase.LoadAssetAtPath(Path, typeof(Texture2D));
            }
            catch (Exception) // If texture missing just set a blank texture
            {
                throw new UnityException("No texture found at " + Path);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Path", Path);
            info.AddValue("PreviewColor", new SerializableColor(PreviewColor));
            info.AddValue("Splat", Splat);
        }

        public void UpdateTexture(TextureSplat ts)
        {
            Splat.Set(ts);
            Path = AssetDatabase.GetAssetPath(Splat.Texture);
            SetAverageColor();
        }

        private void SetAverageColor()
        {
            var texture = Splat.Texture;

            if (texture == null)
            {
                PreviewColor = Color.black;
                return;
            }

            EUtil.CheckTexture(Path);

            var preview = new Color();
            var colors = texture.GetPixels();

            // Add all colours up
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var c in colors)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                preview += c;
            }
            // Divide by total colors to get average color
            PreviewColor = preview / colors.Length;
        }


    }
}
