using System;
using System.Collections.Generic;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class TextureEditorLayer
    {
        public List<TextureEditorArea> Texture { get; private set; }

        public TextureLayer Layer { get; private set; }

        public TextureEditorLayer(TextureLayer tl)
        {
            Texture = new List<TextureEditorArea>();
            Layer = tl;
        }

        public void UpdateAllTextures(int resolution)
        {
            foreach (var t in Texture)
            {
                t.UpdateTextures(resolution);
            }
        }

        public void UpdateAreaTexture(int area, int resolution)
        {
            Texture[area].UpdateTextures(resolution);
        }

        public void AddToLayer(TextureSplat ts, IMap mf)
        {
            var ta = Layer.Add(ts, mf);
            Texture.Add(new TextureEditorArea(ta));
        }
        public void Remove(TextureEditorArea tea)
        {
            Layer.Textures.Remove(tea.Area);
            Texture.Remove(tea);
        }
        public void Insert(int index, TextureEditorArea tea)
        {
            Layer.Textures.Insert(index, tea.Area);
            Texture.Insert(index, tea);
        }
    }
}
