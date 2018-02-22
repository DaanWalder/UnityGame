using System;
using System.Collections.Generic;
using ProWorldSDK;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProWorldEditor
{
    public abstract class LayerPreview : Preview
    {
        protected const int LayerPreviewSize = 256;

        private readonly List<float> _layers;
        private Texture2D _layerTexture;

        //protected float[,] LayerData;

        private int _lastLayer = -1;

        protected LayerPreview()
        {
            _layers = ProWorld.Data.World.TerrainData.Layers;

            _layerTexture = new Texture2D(TextureSize, TextureSize);
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var data = ProWorld.Data.WorldData;

            #region Layers

            GUILayout.BeginArea(new Rect(523, 0, LayerPreviewSize + 11, LayerPreviewSize + 24), "Layers",
                                GUI.skin.window);

            var layerContent = new GUIContent(_layerTexture);
            var rect = GUILayoutUtility.GetRect(layerContent, GUIStyle.none, GUILayout.Width(LayerPreviewSize),
                                                GUILayout.Height(LayerPreviewSize));

            var currentLayer = _lastLayer;

            if (Event.current.type == EventType.Repaint) // otherwise doesn't do rect.Contains calculations correctly
            {
                var pickpos = Event.current.mousePosition;

                if (rect.Contains(pickpos))
                {
                    if (data.Heights != null)
                    {
                        var ratio = TextureSize/LayerPreviewSize;

                        var x = (Convert.ToInt32(pickpos.x) - (int) rect.x); // *ratio;
                        var y = (Convert.ToInt32(pickpos.y) - (int) rect.y); // *ratio;

                        // Need to invert y - for data 0,0 is bottom left, coords 0,0 is top left
                        var xx = x*ratio;
                        var yy = (int) (rect.height - y - 1)*ratio;

                        var d = data.Heights[yy, xx];

                        for (var i = 0; i < _layers.Count; i++)
                        {
                            if (d <= _layers[i])
                            {
                                currentLayer = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    currentLayer = -1;
                }
            }

            if (currentLayer != _lastLayer && !Worker.IsBusy)
            {
                _lastLayer = currentLayer;

                ApplyAreaTexture();
            }

            if (GUI.Button(rect, layerContent, GUIStyle.none) && !Worker.IsBusy)
            {
                LayerSelected(currentLayer);
            }

            GUILayout.EndArea();

            #endregion
        }

        protected void ApplyAreaTexture()
        {
            var c = Util.DivideAreasHighlight(ProWorld.Data.WorldData.Heights, _layers, ProWorld.Data.World.Water.WaterLevel, _lastLayer);

            _layerTexture.SetPixels(c);
            _layerTexture.Apply();
        }

        protected override void Rebuild()
        {
            Object.DestroyImmediate(_layerTexture);
            _layerTexture = new Texture2D(TextureSize, TextureSize);

            base.Rebuild();
        }

        public override void Clean()
        {
            Object.DestroyImmediate(_layerTexture);

            base.Clean();
        }

        protected abstract void LayerSelected(int layer);
    }
}
