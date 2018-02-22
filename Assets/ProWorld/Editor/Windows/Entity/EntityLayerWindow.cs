using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    public sealed class EntityLayerWindow : Preview
    {
        private const int ThumbSize = 64;

        private readonly int _layer;
        private readonly EntityWorker _entityWorker;
        private readonly EntityEditorLayer _editorLayer;

        private readonly TextureEditorLayer _textureLayer;

        private Vector2 _scroll;

        private int[,] _previewData;

        private readonly EntityGenerator _generator;
        private EntityGenerator.TreePlacementQuality _precision;

        private int _currentID = -1;

        private bool _update;
        private bool _isCalculateDone;

        public EntityLayerWindow(int layer)
        {
            Title = "Entity Layer";

            _layer = layer;
            _entityWorker = ProWorld.Data.World.EntityData;
            _editorLayer = ProWorld.Data.Entity[_layer];

            _textureLayer = ProWorld.Data.Texture[_layer];

            _generator = _entityWorker.Generator;
            _precision = _generator.PlacementQuality;

            Refresh();
        }

        public override void OnGUI()
        {
            if (_isCalculateDone)
            {
                ApplyWindow.ApplyEntity();
                _isCalculateDone = false;
            }

            var texture = _currentID == -1 ? _editorLayer.GeneratedTexture : _editorLayer.Entities[_currentID].GeneratedTexture;
            if (texture != null) PreviewTexture = texture;

            base.OnGUI();

            var position = ProWorld.Window.position;

            GUILayout.BeginArea(new Rect(position.width - 200, 0, 200, position.height - 20), "Options", GUI.skin.window);
            GUI.enabled = !Worker.IsBusy;
            Options();
            GUI.enabled = true;
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(523, 0, 290, 536), "Layers", GUI.skin.window);
            Layers();
            GUILayout.EndArea();
        }

        private void Options()
        {
            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
            {
                if (_update)
                {
                    _update = false;

                    Refresh();
                }
            }

            GUILayout.Label("Quality  Preview");
            GUILayout.BeginHorizontal();
            _precision = (EntityGenerator.TreePlacementQuality)MyGUI.EnumButton(_precision);
            GUILayout.EndHorizontal();

            if (_currentID < 0) return;

            var eg = _editorLayer.Layer.Groups[_currentID];

            eg.IsUseDensityMap = GUILayout.Toggle(eg.IsUseDensityMap, "Use Density Map");
            eg.IsInWater = GUILayout.Toggle(eg.IsInWater, "Place in Water");

            GUILayout.Label("Seed");
            GUILayout.BeginHorizontal();
            eg.Seed = (int)MyGUI.StaticSlider(eg.Seed, -1000, 1000, 100, 0);
            GUILayout.Label(eg.Seed.ToString(CultureInfo.InvariantCulture), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.Label("Density");
            GUILayout.BeginHorizontal();
            eg.Density = (int)MyGUI.LogSlider(eg.Density, 0, 5, GUILayout.Width(100));
            GUILayout.Label(eg.Density.ToString(CultureInfo.InvariantCulture), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.Used)
            {
                _update = true;
            }

            var trees = eg.Entities;
            var currentTreeData = trees.Keys.ToList();

            if(GUILayout.Button("Add"))
            {
                EntityList.Show(currentTreeData, AddTrees);
            }

            _scroll = GUILayout.BeginScrollView(_scroll);

            foreach (var t in currentTreeData)
            {
                GUILayout.BeginHorizontal();

#if UNITY_4_0
                var preview = t.Prefab ? AssetPreview.GetAssetPreview(t.Prefab) : Util.White;
#else
                var preview = t.Prefab ? EditorUtility.GetAssetPreview(t.Prefab) : Util.White;
#endif

                GUILayout.BeginHorizontal();
                GUILayout.Box(preview, GUILayout.Width(64), GUILayout.Height(64));

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Density");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X"))
                {
                    trees.Remove(t);
                    Refresh();
                    break;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                trees[t] = MyGUI.LogSlider(trees[t], -1, 1);
                GUILayout.Label(trees[t].ToString("0.00"), GUILayout.Width(40));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
        private void Layers()
        {
            if (GUILayout.Button("Add"))
            {
                AddEntityGroup.Create(_textureLayer, AddArea);
            }

            var mouse = Event.current.mousePosition;

            const int xSize = 50;

            var entities = _editorLayer.Entities;

            for (var i = 0; i < entities.Count; i++)
            {
                var eeg = entities[i];

                GUILayout.BeginHorizontal();

                var rect = GUILayoutUtility.GetRect(280, ThumbSize);

                var rectPreview = rect;
                rectPreview.width = ThumbSize;

                if (_currentID == i) GUI.color = Color.red;
                GUI.Box(rectPreview, eeg.AreaTexture);

                var rectlabel = new Rect(rect);
                rectlabel.width -= (xSize + ThumbSize);
                rectlabel.x += ThumbSize;

                var rectClick = new Rect(rect);
                rectClick.width -= xSize;

                if (Event.current.type == EventType.MouseDown && rectClick.Contains(mouse))
                {
                    _currentID = _currentID == i ? -1 : i;
                }

                GUI.Label(rectlabel, _currentID == i ? "Selected Layer" : "");
                GUI.color = Color.white;

                var rectButtons = new Rect(rect);
                rectButtons.width = xSize;
                rectButtons.x += rectlabel.width + ThumbSize;
                rectButtons.height = 20;

                GUILayout.BeginVertical();

                if (GUI.Button(rectButtons, "X"))
                {
                    _editorLayer.RemoveGroup(eeg);
                    i--;
                    _currentID = -1;
                    Refresh();
                }

                rectButtons.y += 20 + 2;
                if (i == 0) GUI.enabled = false;
                if (GUI.Button(rectButtons, "Up"))
                {
                    entities.Remove(eeg);
                    entities.Insert(i - 1, eeg);
                    _currentID--;
                }
                GUI.enabled = true;

                rectButtons.y += 20 + 2;
                if (i == entities.Count - 1) GUI.enabled = false;
                if (GUI.Button(rectButtons, "Down"))
                {
                    entities.Remove(eeg);
                    entities.Insert(i + 1, eeg);
                    _currentID++;
                }
                GUI.enabled = true;

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
        }

        private void AddTrees(List<EntityData> entities)
        {
            foreach(var ed in entities)
            {
                _editorLayer.Entities[_currentID].Entity.Entities.Add(ed, 1);
            }

            Refresh();
        }
        private void AddArea(List<TextureArea> ta)
        {
            if (ta.Count == 0) return;

            var eg = new EntityGroup(ta)
                         {
                             IsUseDensityMap = true, 
                             Seed = Random.Range(-1000, 1000)
                         };
            _editorLayer.AddToLayer(eg);

            _currentID = _editorLayer.Entities.Count - 1;
            Refresh();
        }

        protected override void SceneChange()
        {
            
        }

        protected override void DoCalculate()
        {
            var eg = _entityWorker.Generator;

            var tmp = eg.PlacementQuality;
            eg.PlacementQuality = _precision;

            if (_currentID == -1)
                eg.GenerateEntities(ProWorld.Data.WorldData, _layer);
            else
                eg.GenerateEntityGroup(ProWorld.Data.WorldData, _editorLayer.Layer.Groups[_currentID], _layer);

            eg.PlacementQuality = tmp;

            GenerateTexture();

            _isCalculateDone = ProWorld.Data.IsRealTime;
        }

        private void GenerateTexture()
        {
            _editorLayer.GenerateAllTextures();
        }

        protected override void ApplyTexture()
        {
            IsReapplyTexture = false;

            _editorLayer.UpdateAllTextures(ThumbSize);
        }
    }
}