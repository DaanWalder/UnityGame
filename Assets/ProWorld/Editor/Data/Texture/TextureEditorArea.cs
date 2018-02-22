using System;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class TextureEditorArea
    {
        public TextureArea Area;

        public IMapEditorData Med;
        public TextureEditorSplat Splat;

        [NonSerialized] public Texture2D MaskFunctionTexture;
        [NonSerialized] public Texture2D MaskAreaTexture;

        public TextureEditorArea(TextureArea area)
        {
            var map = area.MapFunction as MapManager;
            if (map != null)
                Med = new MapEditorData(map);

            Area = area;

            SetSplat(area.Splat);
        }

        public void UpdateTextures(int resolution)
        {
            if (Area.MaskFunction != null)
            {
                var mf = Util.ResizeArray(Area.MaskFunction, resolution);
                if (Splat != null)
                    Util.ApplyBoolMapToTexture(ref MaskFunctionTexture, mf, Splat.PreviewColor);
            }
            if (Area.MaskArea != null)
            {
                var ma = Util.ResizeArray(Area.MaskArea, resolution);
                if (Splat != null)
                    Util.ApplyBoolMapToTexture(ref MaskAreaTexture, ma, Splat.PreviewColor);
            }
        }

        public void SetSplat(TextureSplat ts)
        {
            Area.Splat = ts;
            Splat = ProWorld.Data.GetEditorSplat(ts);
        }
    }
}
