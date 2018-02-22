using System.Globalization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    public sealed class TextureWindow : LayerPreview
    {
        public TextureWindow()
        {
            Title = "Textures";

            Refresh();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var position = ProWorld.Window.position;

            #region Settings
            GUILayout.BeginArea(new Rect(position.width - 200, 0, 200, 200), "Settings", GUI.skin.window);

            GUILayout.Label("Texture Resolution");
            GUILayout.BeginHorizontal();
            var s = ProWorld.Data.World.TextureData.TextureResolution;
            var bits = 0;
            while (s > 1) // much more efficient than log10 / log2 
            {
                bits++;
                s >>= 1;
            }
            bits = MyGUI.RoundSlider(bits, 6, 12);
            ProWorld.Data.World.TextureData.TextureResolution = (int)Mathf.Pow(2, bits);

            GUILayout.Label(ProWorld.Data.World.TextureData.TextureResolution.ToString(CultureInfo.InvariantCulture), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            #endregion
        }

        protected override void SceneChange()
        {
            GenerateTerrain();
            GenerateTextures();
        }

        protected override void ApplyTexture()
        {
            const int size = EditorData.TextureSize;
            IsReapplyTexture = false;

            var color = new Color[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    foreach (var layer in ProWorld.Data.Texture)
                    {
                        foreach (var t in layer.Texture)
                        {
                            var area = t.Area;

                            if (area.MaskArea[y, x])
                            {
                                color[y * size + x] = t.Splat.PreviewColor;
                                break;
                            }
                        }
                    }
                }
            }

            var c = Util.ResizeArray(color, TextureSize);
            PreviewTexture.SetPixels(c);
            PreviewTexture.Apply();

            ApplyAreaTexture();
        }

        protected override void LayerSelected(int layer)
        {
            ProWorld.NewWindow(new TextureLayerWindow(layer));
        }
    }
}
