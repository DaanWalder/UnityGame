using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    sealed class TextureLayerWindow : Preview
    {
        private const int ThumbSize = 64;

        private readonly int _layer;
        private readonly TextureEditorLayer _editorLayer;

        private int _currentMask = -2;

        private Vector2 _scroll;
        private bool _isCalculateDone;

        public TextureLayerWindow(int layer)
        {
            Title = "Texture Layer";

            _layer = layer;

            _editorLayer = ProWorld.Data.Texture[_layer];

            Refresh();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (_isCalculateDone)
            {
                ApplyWindow.ApplyTexture();
                _isCalculateDone = false;
            }

            GUILayout.BeginArea(new Rect(523, 0, 290, 536), "Layers", GUI.skin.window);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Area"))
            {              
                _editorLayer.AddToLayer(new TextureSplat(), new MapManager());
                _currentMask = _editorLayer.Texture.Count-1;
                TextureDataProperties.CreateTDP(_editorLayer.Texture[_currentMask], CheckRemove);
            }
            GUI.enabled = false; // Cliff temp disabled till I fix some issues
            if (GUILayout.Button("Cliff"))
            {
                _editorLayer.AddToLayer(new TextureSplat(), new CliffGenerator(ProWorld.Data.World.TerrainData));
                _currentMask = _editorLayer.Texture.Count - 1;
                TextureDataProperties.CreateTDP(_editorLayer.Texture[_currentMask], CheckRemove);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll);

            // Do it backwards
            for (var i = _editorLayer.Layer.Textures.Count - 1; i >= 0; i--)
            {
                //var l = _textureLayer.Textures[i];
                var el = _editorLayer.Texture[i];

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(el.Area.Splat.Texture, GUI.skin.box, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize)))
                {
                    _currentMask = i;
                    TextureDataProperties.CreateTDP(el, CheckRemove);
                }
                GUI.enabled = true;
                if (i == 0) // Top tier is not editable
                {
                    GUILayout.Box(el.MaskFunctionTexture, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize));
                }
                else
                {
                    if (GUILayout.Button(el.MaskFunctionTexture, GUI.skin.box, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize)))
                    {
                        var med = el.Med as MapEditorData;
                        if (med != null)
                        {
                            _currentMask = i;
                            ProWorld.NewWindow(new MapEditor(med, ProWorld.Window));
                        }
                    }
                }
                GUILayout.Box(el.MaskAreaTexture, GUI.skin.box, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize));

                GUILayout.BeginVertical();
                if (GUILayout.Button("X", GUILayout.Width(50)))
                {
                    _editorLayer.Remove(el);

                    _currentMask = i == 0 ? 0 : -1;

                    Refresh();
                }
                if (i == _editorLayer.Layer.Textures.Count - 1) GUI.enabled = false;
                if (GUILayout.Button("Up", GUILayout.Width(50)))
                {
                    _editorLayer.Remove(el);
                    _editorLayer.Insert(i + 1, el);

                    if (i == 0)
                    {
                        _editorLayer.Layer.Textures[0].SetAsBase(EditorData.TextureSize);
                        _currentMask = 1;
                        Refresh();
                    }
                }
                GUI.enabled = true;
                if (i == 0) GUI.enabled = false;
                if (GUILayout.Button("Down", GUILayout.Width(50)))
                {
                    _editorLayer.Remove(el);
                    _editorLayer.Insert(i - 1, el);

                    if (i == 1)
                    {
                        el.Area.SetAsBase(EditorData.TextureSize);
                        _currentMask = 1;
                        Refresh();
                    }
                }
                GUI.enabled = true;
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void CheckRemove(TextureEditorArea textureEditorArea)
        {
            if (textureEditorArea.Area.Splat == null || textureEditorArea.Area.Splat.Texture == null)
            {
                _editorLayer.Remove(textureEditorArea);
                _currentMask = -1;
            }

            Refresh();
        }

        protected override void SceneChange()
        {
            GenerateTerrain();
            GenerateTextures();

            // TODO CHANGE MAYBE ONLY GENERATE THIS LAYER?!?
            //((TextureGenerator)textureWorker.Generator).GenerateLayerTextures(_editorLayer.Layer, _layer, ProWorld.Data.WorldData.LayerMasks, Offset.x, Offset.y);
        }

        protected override void DoCalculate()
        {
            if (_currentMask >= -1)
            {
                var textureWorker = ProWorld.Data.World.TextureData;

                // Create masks
                if (_currentMask == 0)
                    _editorLayer.Layer.Textures[_currentMask].SetAsBase(EditorData.TextureSize);
                else if(_currentMask > 0)
                    _editorLayer.Layer.Textures[_currentMask].UpdateMask(EditorData.TextureSize, Offset.x, Offset.y);

                textureWorker.Generator.CalculateMaskSections(_layer, ProWorld.Data.WorldData.LayerMasks, EditorData.TextureSize);
            }

            _currentMask = -2;

            _isCalculateDone = ProWorld.Data.IsRealTime;
        }

        protected override void ApplyTexture()
        {
            const int size = EditorData.TextureSize;
            IsReapplyTexture = false;

            _editorLayer.UpdateAllTextures(ThumbSize);

            var color = new Color[size * size];

            const float nfactor = EditorData.TerrainSize / (float)EditorData.TextureSize;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var xx = (int)Mathf.Floor(nfactor * x);
                    var yy = (int)Mathf.Floor(nfactor * y);

                    if (ProWorld.Data.WorldData.IsPointInMask(_layer, xx, yy))
                    {
                        for (int index = 0; index < _editorLayer.Layer.Textures.Count; index++)
                        {
                            var t = _editorLayer.Layer.Textures[index];
                            if (t.MaskArea[y, x])
                            {
                                var c = _editorLayer.Texture[index].Splat.PreviewColor;
                                color[y * size + x] = c;
                                break;
                            }
                        }
                    }
                    else
                    {
                        color[y * size + x] = Color.black;
                    }
                }
            }
            var resziedArray = Util.ResizeArray(color, TextureSize);

            PreviewTexture.SetPixels(resziedArray);
            PreviewTexture.Apply();
        }
    }
}
