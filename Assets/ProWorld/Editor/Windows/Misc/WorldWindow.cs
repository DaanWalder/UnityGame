using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProWorldEditor
{
    [Serializable]
    public abstract class WindowLayout
    {
        public string Title = "Window";

        public virtual void Update() { }
        public abstract void OnGUI();
        public virtual void Clean() { }
        public virtual void Refresh() { }
        public virtual void Apply() { }
    }
    [Serializable]
    public abstract class AreaLayout
    {
        public abstract void OnGUI();
        public virtual void Clean() { }
    }

    [Serializable]
    public sealed class WorldWindow : Preview
    {
        [Flags]
        private enum WorkToDo
        {
            None,
            Terrain = 1,
            Texture = 2,
            Entity = 4,
            All = Terrain | Texture | Entity
        }

        private const int TexturePreviewSize = 128;

        private Texture2D _heightMap;
        private Texture2D _areaMap;
        private Texture2D _textureMap;
        private Texture2D _entityMap;

        private WorkToDo _workToDo = WorkToDo.All;

        public WorldWindow()
        {
            Title = "ProWorld";

            var res = ProWorld.Data.Terrain.Resolution;

            _heightMap = new Texture2D(res, res);
            _areaMap = new Texture2D(res, res);
            _textureMap = new Texture2D(res, res);
            _entityMap = new Texture2D(res, res);

            IsPreviewChanged = true;
            DoWork();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var position = ProWorld.Window.position;

            GUI.enabled = !Worker.IsBusy;
            #region Options
            //GUILayout.BeginArea(new Rect(0, 536, 523, position.height - 536 - 20), "Options", GUI.skin.window);
            GUILayout.BeginArea(new Rect(position.width - 200, 200, 200, 200), "Options", GUI.skin.window);

            //GUILayout.BeginHorizontal();
            GUILayout.Label("Water Level");
            GUILayout.BeginHorizontal();
            var waterBefore = ProWorld.Data.World.Water.WaterLevel;
            ProWorld.Data.World.Water.WaterLevel = GUILayout.HorizontalSlider(ProWorld.Data.World.Water.WaterLevel, 0, 0.99f, GUILayout.Width(100));
            GUILayout.Label(ProWorld.Data.World.Water.WaterLevel.ToString("0.00")
                + " (" + (ProWorld.Data.World.Water.WaterLevel * ProWorld.Data.World.TerrainHeight).ToString("0000") + ")");

            if (Event.current.type == EventType.Used && (Math.Abs(waterBefore - ProWorld.Data.World.Water.WaterLevel) > float.Epsilon))
            {
                if (ProWorld.Data.IsRealTime) 
                    ApplyWindow.ApplyWater();

                ApplyWaterChange();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Water Texture");
            var water = ProWorld.Data.World.Water;

            var prefab = (GameObject)EditorGUILayout.ObjectField(water.Prefab, typeof (GameObject), false);
            if (prefab != water.Prefab)
            {
                ProWorld.Data.Water.UpdateObject(prefab);
            }
            GUILayout.EndArea();
            #endregion

            #region Settings
            GUILayout.BeginArea(new Rect(position.width - 200, 0, 200, 200), "Settings", GUI.skin.window);

            GUILayout.Label("Height Map Resolution");
            GUILayout.BeginHorizontal();
            var s = ProWorld.Data.World.TerrainData.Resolution - 1;
            var bits = 0;
            while (s > 1) // much more efficient than log10 / log2 
            {
                bits++;
                s >>= 1;
            }
            bits = MyGUI.RoundSlider(bits, 9, 12);
            ProWorld.Data.World.TerrainData.Resolution = (int)Mathf.Pow(2, bits) + 1;

            GUILayout.Label(ProWorld.Data.World.TerrainData.Resolution.ToString(CultureInfo.InvariantCulture), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.Label("Terrain Size");
            GUILayout.BeginHorizontal();
            var size = ProWorld.Data.World.TerrainWidth;
            var sizeb = size;
            size = Mathf.RoundToInt(MyGUI.LogSlider(size, 2, 4));
            if (size != sizeb)
                size = Util.RoundToLog(size);

            var sizeT = GUILayout.TextField(size.ToString(CultureInfo.InvariantCulture), 5, GUILayout.Width(50));
            sizeT = Regex.Replace(sizeT, "^[^0-9]", "");
            int.TryParse(sizeT, out size);
            ProWorld.Data.World.TerrainWidth = size;
            GUILayout.EndHorizontal();

            GUILayout.Label("Terrain Height");
            GUILayout.BeginHorizontal();
            var height = ProWorld.Data.World.TerrainHeight;
            var heightb = height;
            height = Mathf.RoundToInt(MyGUI.LogSlider(height, 1, 3));
            if (height != heightb)
                height = Util.RoundToLog(height);

            var heightT = GUILayout.TextField(height.ToString(CultureInfo.InvariantCulture), 5, GUILayout.Width(50));
            heightT = Regex.Replace(heightT, "^[^0-9]", "");
            int.TryParse(heightT, out height);
            ProWorld.Data.World.TerrainHeight = height;
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            #endregion

            GUI.enabled = true;

            #region Terrain
            GUILayout.BeginArea(new Rect(523, 0, 139, 152), "Terrain", GUI.skin.window);
            if (GUILayout.Button(_heightMap, GUIStyle.none, GUILayout.Width(TexturePreviewSize), GUILayout.Height(TexturePreviewSize)))
            {
                _workToDo = WorkToDo.All;
                ProWorld.NewWindow(new MapEditor(ProWorld.Data.Terrain.Function, ProWorld.Window, ApplyTerrain));
            }
            GUILayout.EndArea();
            #endregion

            #region Layer
            GUILayout.BeginArea(new Rect(523, 152, 139, 152), "Layers", GUI.skin.window);
            if (GUILayout.Button(_areaMap, GUIStyle.none, GUILayout.Width(TexturePreviewSize), GUILayout.Height(TexturePreviewSize)))
            {
                _workToDo = WorkToDo.All;
                ProWorld.NewWindow(new LayerWindow());
            }
            GUILayout.EndArea();
            #endregion

            #region Texture
            GUILayout.BeginArea(new Rect(523, 304, 139, 152), "Textures", GUI.skin.window);
            if (GUILayout.Button(_textureMap, GUIStyle.none, GUILayout.Width(TexturePreviewSize), GUILayout.Height(TexturePreviewSize)))
            {
                _workToDo = WorkToDo.Entity;
                ProWorld.NewWindow(new TextureWindow());
            }
            GUILayout.EndArea();
            #endregion

            #region Entities
            GUILayout.BeginArea(new Rect(523, 456, 139, 152), "Entities", GUI.skin.window);
            if (GUILayout.Button(_entityMap, GUIStyle.none, GUILayout.Width(TexturePreviewSize), GUILayout.Height(TexturePreviewSize)))
            {
                _workToDo = WorkToDo.None;
                ProWorld.NewWindow(new EntityWindow());
            }
            GUILayout.EndArea();
            #endregion
        }

        protected override void SceneChange()
        {
            _workToDo = WorkToDo.All;
        }
        protected override void DoCalculate()
        {
            if ((_workToDo & WorkToDo.Terrain) == WorkToDo.Terrain)
            {
                GenerateTerrain();
            }
            if ((_workToDo & WorkToDo.Texture) == WorkToDo.Texture)
            {
                GenerateTextures();
            }
            if ((_workToDo & WorkToDo.Entity) == WorkToDo.Entity)
            {
                GenerateEntities();
            }

            _workToDo = WorkToDo.None;
        }
        
        protected override void Rebuild()
        {
            var res = ProWorld.Data.Terrain.Resolution;

            Object.DestroyImmediate(_heightMap);
            _heightMap = new Texture2D(res, res);
            Object.DestroyImmediate(_areaMap);
            _areaMap = new Texture2D(res, res);
            Object.DestroyImmediate(_textureMap);
            _textureMap = new Texture2D(res, res);
            Object.DestroyImmediate(_entityMap);
            _entityMap = new Texture2D(res, res);

            base.Rebuild();
        }
        protected override void ApplyTexture()
        {
            IsReapplyTexture = false;

            // Handles Preview/Area textures
            ApplyWaterChange();

            Util.ApplyMapToTexture(ref _heightMap, ProWorld.Data.WorldData.Heights);
            ApplyTextureMap();
            ApplyEntityMap();

        }
        private void ApplyWaterChange()
        {
            Util.ApplyWaterToTexture(ref PreviewTexture, ProWorld.Data.WorldData.Heights, ProWorld.Data.World.Water.WaterLevel);
            var c = Util.DivideAreas(ProWorld.Data.WorldData.Heights, ProWorld.Data.World.TerrainData.Layers, ProWorld.Data.World.Water.WaterLevel);

            _areaMap.SetPixels(c);
            _areaMap.Apply();
        }
        private void ApplyTextureMap()
        {
            const int size = EditorData.TextureSize;
            var color = new Color[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    //foreach (var layer in ProWorld.Data.World.TextureData.TextureLayers)
                    foreach (var layer in ProWorld.Data.Texture)
                    {
                        foreach (var t in layer.Texture)
                        {
                            var textureArea = t.Area;

                            if (textureArea.MaskArea[y, x])
                            {
                                color[y * size + x] = t.Splat.PreviewColor;
                                break;
                            }
                        }
                    }
                }
            }

            var resizedTexture = Util.ResizeArray(color, TextureSize);

            _textureMap.SetPixels(resizedTexture);
            _textureMap.Apply();
        }
        private void ApplyEntityMap()
        {
            const int size = EditorData.TerrainSize;
            var colors = new Color[size * size];

            var resized = Util.ResizeArray(ProWorld.Data.World.EntityData.Generator.Densisty, size);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    colors[y * size + x] = new Color(0, resized[y, x], 0.25f);
                }
            }

            _entityMap.SetPixels(colors);
            _entityMap.Apply();
        }
        public override void Clean()
        {
            Object.DestroyImmediate(_heightMap);
            Object.DestroyImmediate(_areaMap);
            Object.DestroyImmediate(_textureMap);
            Object.DestroyImmediate(_entityMap);

            base.Clean();
        }

        public void ApplyTerrain(float[,] heights)
        {
            ApplyWindow.ApplyTerrain(heights);
        }
    }
}