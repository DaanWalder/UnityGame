using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProWorldSDK;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ProWorldEditor
{
    public class ApplyWindow : WindowLayout
    {
        private WorkManager _manager;

        // static
        private Terrain _terrain;
        private float _range;
        private Vector2 _offset = Vector2.zero;

        // dynamic
        private GameObject _gameObject;
        private string _path = string.Empty;

        public ApplyWindow()
        {
            Title = "Apply";
            GetSelection(ref _terrain);
            GetGameObject(ref _gameObject);
        }

        public override void Update()
        {
            if (_manager != null)
            {
                var startTime = DateTime.Now;
                const double time = 1000.00/30.00;

                if (_manager.Update(startTime, time))
                {
                    // apply
                    _manager = null;
                }
            }
        }

        public override void OnGUI()
        {
            var position = ProWorld.Window.position;

            GUILayout.BeginArea(new Rect(position.width - 200, 0, 200, 150), "Single Terrain", GUI.skin.window);
            SingleApply();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 200, 200, 200, 200), "Real-Time Terrain", GUI.skin.window);
            RealTimeApply();
            GUILayout.EndArea();
            
        }

        private void SingleApply()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Terrain:", GUILayout.Width(80));
            if (GUILayout.Button("Selection", EditorStyles.miniButton))
            {
                GetSelection(ref _terrain);
            }
            GUILayout.EndHorizontal();
            _terrain = (Terrain)EditorGUILayout.ObjectField(_terrain, typeof(Terrain), true);

            GUILayout.Label("Position:");
            _offset.x = EditorGUILayout.IntField("X:", (int)_offset.x);
            _offset.y = EditorGUILayout.IntField("Y:", (int)_offset.y);

            GUILayout.FlexibleSpace();
            if (_manager == null)
            {
                if (GUILayout.Button("Apply"))
                {
                    ApplyToTerrain();
                }
            }
            else
            {
                if (GUILayout.Button("Stop"))
                {
                    _manager = null;
                }
            }
        }

        private void RealTimeApply()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GameObject:", GUILayout.Width(80));
            if (GUILayout.Button("Selection", EditorStyles.miniButton))
            {
                GetGameObject(ref _gameObject);
            }
            GUILayout.EndHorizontal();
            _gameObject = (GameObject)EditorGUILayout.ObjectField(_gameObject, typeof(GameObject), true);

            GUILayout.BeginHorizontal();
            GUILayout.Label("World:", GUILayout.Width(80));
            if (GUILayout.Button("Browse", EditorStyles.miniButton))
            {
                _path = EditorUtility.OpenFilePanel("ProWorld file", @"Assets\ProWorld\" + EditorData.Folder, "pw");
            }
            GUILayout.EndHorizontal();
            GUILayout.TextField(_path == string.Empty ? "No world selected" : _path);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply"))
            {
                GenerateWorldBuilder();
            }

        }

        public static void GetSelection(ref Terrain terrain)
        {
            var active = Selection.activeGameObject;
            if (!active) return;

            var t = active.GetComponent<Terrain>();
            if (!t) return;

            terrain = t;
        }

        public static void GetGameObject(ref GameObject go)
        {
            var active = Selection.activeGameObject;
            if (!active) return;

            go = active;
        }

// ReSharper disable UnusedMember.Local
        private static List<Terrain> GetSelections()
// ReSharper restore UnusedMember.Local
        {
            var l = new List<Terrain>();

            var active = Selection.gameObjects;
            if (active.Length == 0) return l;

// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var a in active)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                var t = a.GetComponent<Terrain>();
                if (t)
                {
                    l.Add(t);
                }
            }

            return l;
        }

        private void ApplyToTerrain()
        {
            if (_terrain == null)
            {
                Debug.Log("No Terrain selected.");
                return;
            }
            if (_terrain.terrainData == null)
            {
                _terrain.terrainData = new TerrainData();
            }

            var world = ProWorld.Data.World;

            // Setupwater
            var waterData = world.Water;

            if (waterData.Prefab != null)
            {

                var water = _terrain.transform.Find("Water");
                if (water == null)
                {
                    water = ((GameObject) Object.Instantiate(waterData.Prefab)).transform;
                    water.parent = _terrain.transform;
                    water.name = "Water";
                }

                var v = Vector3.zero;
                v.x = world.TerrainWidth/2f;
                v.y = waterData.WaterLevel*world.TerrainHeight;
                v.z = world.TerrainWidth/2f;
                water.localPosition = v;
            }

            _manager = new WorkManager(world, _terrain, _offset);
        }

        private void GenerateWorldBuilder()
        {
            if (_gameObject == null)
            {
                Debug.Log("No GameObject selected.");
                return;
            }
            if (_path == string.Empty)
            {
                Debug.Log("No ProWorld file selected.");
                return;
            }

            var wb = _gameObject.GetComponent<WorldBuilder>();

// ReSharper disable ConvertIfStatementToNullCoalescingExpression
            if (wb == null)
// ReSharper restore ConvertIfStatementToNullCoalescingExpression
                wb = _gameObject.AddComponent<WorldBuilder>();

            WorldBuilderEditor.Rebuild(_path, wb);
        }

        public static void ApplyTerrain(float[,] heights)
        {
            Terrain t = null;
            GetSelection(ref t);

            if (t == null) return;

            var world = ProWorld.Data.World;

            t.terrainData.heightmapResolution = heights.GetLength(0);
            t.terrainData.size = new Vector3(world.TerrainWidth, world.TerrainHeight, world.TerrainWidth);
            t.terrainData.SetHeights(0, 0, heights);
        }

        public static void ApplyWater()
        {
            var waterData = ProWorld.Data.World.Water;

            Terrain t = null;
            GetSelection(ref t);

            if (t == null) return;

            var water = t.transform.Find("Water");
            if (waterData.Prefab == null) return;

            if (water == null)
            {
                water = ((GameObject)Object.Instantiate(waterData.Prefab)).transform;
                water.parent = t.transform;
                water.name = "Water";
            }

            var v = Vector3.zero;
            v.x = ProWorld.Data.World.TerrainWidth/2f;
            v.y = waterData.WaterLevel*ProWorld.Data.World.TerrainHeight;
            v.z = ProWorld.Data.World.TerrainWidth / 2f;
            water.localPosition = v;
        }

        public static void ApplyTexture()
        {
            Terrain terrain = null;
            GetSelection(ref terrain);

            if (terrain == null) return;

            const int resolution = EditorData.TextureSize;
            var layers = ProWorld.Data.World.TextureData.Generator.TextureLayers;

            terrain.terrainData.alphamapResolution = resolution;

            var textures = new List<TextureSplat>();
            foreach (var layer in layers)
            {
                foreach (var texture in layer.Textures)
                {
                    if (!textures.Contains(texture.Splat))
                        textures.Add(texture.Splat);
                }
            }

            terrain.terrainData.splatPrototypes = textures.Select(s => s.ToSplatPrototype()).ToArray();

            var alphaMapData = new float[resolution, resolution, textures.Count];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    foreach (var layer in ProWorld.Data.Texture)
                    {
                        foreach (var t in layer.Texture)
                        {
                            var area = t.Area;
                            var index = textures.FindIndex(s => s == area.Splat);

                            if (area.MaskArea[y, x])
                            {
                                alphaMapData[y, x, index] = 1;
                                break;
                            }
                        }
                    }
                }
            }


            terrain.terrainData.SetAlphamaps(0, 0, alphaMapData);
        }

        public static void ApplyEntity()
        {
            Terrain terrain = null;
            GetSelection(ref terrain);

            if (terrain == null) return;

            var td = terrain.terrainData;

            var data = ProWorld.Data.World.EntityData.Generator.EntityLayers;

            var entityToPlace =  new Queue<Entity>((from layer in data
                                  from groups in layer.Groups
                                  from box in groups.Boxes
                                  from e in box.Objects
                                  select e).ToArray());


            var entities = new List<GameObject>();
            foreach (var e in entityToPlace)
            {
                if (!entities.Contains(e.Data.Prefab))
                    entities.Add(e.Data.Prefab);
            }

            var tps = new TreePrototype[entities.Count];

            for (var i = 0; i < entities.Count; i++)
            {
                tps[i] = new TreePrototype
                {
                    prefab = entities[i]
                };
            }

            td.treePrototypes = tps;
            td.RefreshPrototypes();

            // Clear any previous trees
            td.treeInstances = new TreeInstance[0];

            while (entityToPlace.Count > 0)
            {
                var entity = entityToPlace.Dequeue();

                var pos = entity.Position;
                pos /= td.size.x;
                pos.y = 0;

                var index = 0;
                for (var i = 0; i < td.treePrototypes.Length; i++)
                {
                    if (td.treePrototypes[i].prefab == entity.Data.Prefab)
                    {
                        index = i;
                        break;
                    }
                }

                var color = Random.Range(0.6f, 1f); //0.973f

                var ti = new TreeInstance
                {
                    color = new Color(color, color, color, 1.000f),
                    heightScale = 1,
                    lightmapColor = new Color(1.000f, 1.000f, 1.000f, 1.000f),
                    widthScale = 1,
                    position = pos,
                    prototypeIndex = index
                };

                terrain.AddTreeInstance(ti);
                terrain.Flush();
            }
        }
    }
}