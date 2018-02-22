using System;
using System.Collections.Generic;
using System.Globalization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
    {
    [Serializable]
    public sealed class LayerWindow : Preview
    {
        private readonly List<float> _layers;

        private readonly GUIStyle _stretch;

        private int _areas;
        private bool _isUpdateNumAreas;
        private bool _isUpdateAreas;

        private readonly Texture2D _slide = new Texture2D(20, 180);
        
        public LayerWindow()
        {
            Title = "Layers";

            _layers = ProWorld.Data.World.TerrainData.Layers;
            _areas = _layers.Count;

            _stretch = new GUIStyle(GUIStyle.none)
                           {
                               stretchHeight = true, 
                               stretchWidth = true
                           };

            Refresh();
        }

        protected override void SceneChange()
        {
            GenerateTerrain();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var position = ProWorld.Window.position;

            GUILayout.BeginArea(new Rect(position.width - 200, 0, 200, position.height - 130), "Options", GUI.skin.window);
            Options();
            GUILayout.EndArea();
        }

        private void Options()
        {
            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
            {
                if (_isUpdateNumAreas)
                {
                    _isUpdateNumAreas = false;

                    ProWorld.Data.SetLayers(_areas);
                    ApplyTexture();
                    UpdateSliderTexture();
                }
                if (_isUpdateAreas)
                {
                    _isUpdateAreas = false;
                    ApplyTexture();
                    UpdateSliderTexture();
                }
            }
            
            GUILayout.Label("Areas");
            GUILayout.BeginHorizontal();
            _areas = MyGUI.RoundSlider(_areas, 1, 8);
            GUILayout.Label(_areas.ToString(CultureInfo.InvariantCulture), GUILayout.Width(40));
            GUILayout.EndHorizontal();

            // Don't do on the fly otherwise it'd cause lots of updates
            if (Event.current.type == EventType.Used && _areas != _layers.Count)
            {
                _isUpdateNumAreas = true;
            }

            GUILayout.BeginHorizontal(); // Begin Hor 1
            var heightsBack = new GUIContent(_slide);
            GUILayout.BeginVertical(); // Begin Vert 1
            GUILayout.Space(5);
            var rect = GUILayoutUtility.GetRect(heightsBack, _stretch, GUILayout.Width(20), GUILayout.Height(180));
            GUILayout.Space(5);
            GUILayout.EndVertical(); // End Vert 1
            GUI.Box(rect, heightsBack, _stretch);

            // -1 because we don't want to be able to change last element
            for (var i = 0; i < _layers.Count - 1; i++)
            {
                //const float space = World.FadeSize * 2 + 0.01f;
                const float space = 0.01f; // TODO DYNAMIC

                var min = i - 1 >= 0 ? _layers[i - 1] + space : space; // check out of range
                var max = _layers[i + 1] - space; // check out of range

                var before = _layers[i];
                _layers[i] = GUILayout.VerticalSlider(_layers[i], 1, 0, GUILayout.Width(20), GUILayout.Height(190));
                _layers[i] = Mathf.Clamp(_layers[i], min, max);

                if (Event.current.type == EventType.Used && Math.Abs(before - _layers[i]) > float.Epsilon)
                // Don't do on the fly otherwise it'd cause lots of updates
                {
                    _isUpdateAreas = true;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal(); // End Hor 1

            GUILayout.BeginHorizontal(); // Begin Hor 2
            GUILayout.Space(20);
            for (var i = 0; i < _layers.Count - 1; i++)
            {
                GUILayout.Label((_layers[i] * 100).ToString("00"), GUILayout.Width(18));
            }
            GUILayout.EndHorizontal(); // End Hor 2
        }

        protected override void ApplyTexture()
        {
            IsReapplyTexture = false;

            var c = Util.DivideAreas(ProWorld.Data.WorldData.Heights, _layers, ProWorld.Data.World.Water.WaterLevel);

            PreviewTexture.SetPixels(c);
            PreviewTexture.Apply();
        }

        private void UpdateSliderTexture()
        {
            var colors = Colors();
            _slide.SetPixels(colors);
            _slide.Apply();
        }

        private Color[] Colors()
        {
            var colors = new Color[180 * 20];
            var last = 0;

            var lColor = new Color[_areas];

            // Pregenerate colors for speed
            for (var index = 0; index < lColor.Length; index++ )
            {
                var d = index / (float)_areas;

                //var water = ProWorld.Data.World.WaterLevel;
                //var r = 0;
                // function = (dif / (1-d)) * d + (water * max - min) / (water - 1);
                //var g = 0.5f / (1 - water) * d + (water - 0.5f) / (water - 1); // function that takes water -> 1 and returns 0.25 -> 1
                //var b = 0.125f / (1 - water) * d + (water * 0.25f - 0.125f) / (water - 1); // function that takes water -> 1 and returns 0.25 -> 1

                const int r = 0;
                var g = d * 0.75f + 0.25f;
                var b = d * 0.1875f + 0.0625f;

                lColor[index] = new Color(r,g,b);
            }

            for (var y = 0; y < 180; y++)
            {
                var data = y/180f;

                for (var i = 0; i < _areas; i++)
                {
                    if (data < _layers[i])
                    {
                        var c = lColor[i];

                        var water = ProWorld.Data.World.Water.WaterLevel;

                        if (data <= water)
                        {
                            c.r = 0;
                            //c.g = c.g;
                            c.b = 1;
                        }

                        for (var x = 0; x < 20; x++)
                        {
                            // Add a black line between
                            colors[y*20 + x] = i != last ? Color.black : c;
                        }

                        last = i;
                        break;
                    }
                }
            }
            return colors;
        }

        public override void Refresh()
        {
            base.Refresh();

            UpdateSliderTexture();
        }
    }
}