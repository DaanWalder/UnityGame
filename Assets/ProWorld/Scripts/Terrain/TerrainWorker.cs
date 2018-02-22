using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class TerrainWorker : Worker, ILayer
    {
        private int _areaPerFrame = 64;
        public int AreaPerFrame
        {
            get { return _areaPerFrame; }
            set { _areaPerFrame = value >= Resolution ? Resolution : Util.UpperPowerOfTwo(value); }
        }

        public int Resolution { get; set; }
        public MapManager Function { get; private set; }

        public List<float> Layers { get; private set; }

        [NonSerialized] private bool _isAppliedSettings;
        [NonSerialized] private int _index;

        public TerrainWorker(int resolution)
        {
            Resolution = resolution;
            Function = new MapManager();

            Priority = 0;

            Layers = new List<float>(); 
        }

        public override void Generate(object sender, DoWorkEventArgs e)
        {
            var data = (WorldData)e.Argument;

            data.Heights = GetTerrain(data.Position);
            data.LayerMasks = SplitLayers(data.Heights);
            data.SplitTerrain = SplitHeight(data.Heights);
        }

        public override bool Apply(WorldData data, DateTime startTime, double duration)
        {
            if (data.SplitTerrain != null)
            {
                if (data.Terrain)
                {
                    if (!_isAppliedSettings)
                    {
                        data.Terrain.terrainData.heightmapResolution = Resolution;
                        data.Terrain.terrainData.size = new Vector3(data.World.TerrainWidth, data.World.TerrainHeight, data.World.TerrainWidth);

                        _isAppliedSettings = true;
                    }

                    var height = data.SplitTerrain[_index];

                    var sections = Resolution / _areaPerFrame;

                    var y = _index / sections;
                    var x = _index % sections;

                    data.Terrain.terrainData.SetHeights(x * _areaPerFrame, y * _areaPerFrame, height);

                    if (++_index == data.SplitTerrain.Count)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public float[,] GetTerrain(Vector2 index)
        {
            var x = index.x;
            var y = index.y;

            NodeData.GlobalRange = 1f;
            var heights = Function.GetArea(Resolution, x, y);

            return heights;
        }
        public float[,] GetTerrain(int resolution, Vector2 index, float range = 1f)
        {
            var x = index.x;
            var y = index.y;

            NodeData.GlobalRange = range;
            var heights = Function.GetArea(resolution, x, y);

            return heights;
        }
        public int[,] SplitLayers(float[,] map)
        {
            var resolution = map.GetLength(0);

            var layerMasks = new int[resolution, resolution];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var d = map[y, x];

                    for (var i = 0; i < Layers.Count; i++)
                    {
                        var min = i - 1 >= 0 ? Layers[i - 1] : 0.00f;
                        var max = i + 1 < Layers.Count ? Layers[i] : 1.01f;

                        if (d >= min && d < max)
                        {
                            layerMasks[y, x] = i;
                            break;
                        }
                    }
                }
            }

            return layerMasks;
        }
        private List<float[,]> SplitHeight(float[,] heights)
        {
            var height = heights.GetLength(0);
            var width = heights.GetLength(1);

            var sectionsX = width / _areaPerFrame;
            var sectionsY = height / _areaPerFrame;

            var heightOut = new List<float[,]>();

            var startY = 0;
            for (var yy = 0; yy < sectionsY; yy++)
            {
                var bonusY = yy == sectionsY - 1 ? 1 : 0; // since height maps are power of 2 + 1, 

                var startX = 0;
                for (var xx = 0; xx < sectionsX; xx++)
                {
                    var bonusX = xx == sectionsX - 1 ? 1 : 0;

                    var heightSection = new float[_areaPerFrame + bonusY, _areaPerFrame + bonusX];

                    for (var y = 0; y < _areaPerFrame + bonusY; y++)
                    {
                        for (var x = 0; x < _areaPerFrame + bonusX; x++)
                        {
                            heightSection[y, x] = heights[y + startY, x + startX];
                        }
                    }

                    heightOut.Add(heightSection);
                    startX += _areaPerFrame;
                }
                startY += _areaPerFrame;
            }

            return heightOut;
        }

        public float GetSteepness(float x, float y)
        {
            /*var multi = (Resolution - 2);
            var widthPerStep = _world.TerrainWidth/(float)(Resolution - 1);
            
            var xx = (int) (x*multi);
            var yy = (int) (y*multi);

            var ratio = _world.TerrainHeight/widthPerStep;

            var slopex = Mathf.Atan((Heights[xx + 1, yy] - Heights[xx, yy])*ratio)/(90*Mathf.Deg2Rad);
            var slopey = Mathf.Atan((Heights[xx, yy + 1] - Heights[xx, yy])*ratio)/(90 * Mathf.Deg2Rad);

            var slope = Mathf.Sqrt(slopex*slopex + slopey*slopey) / Mathf.Sqrt(2);

            return slope;*/

            return 0f;
        }

        public void SetLayers(int layers)
        {
            if (Layers.Count > layers)
            {
                for (var i = Layers.Count - 1; i >= layers; i--)
                {
                    Layers.RemoveAt(i);
                }
            }
            else if (Layers.Count < layers)
            {
                var c = Layers.Count;
                for (var i = 0; i < layers - c; i++)
                {
                    Layers.Add(0);
                }
            }

            // Redo cutoffs
            var layerCount = Layers.Count;
            for (var i = 0; i < layerCount; i++)
            {
                Layers[i] = (i + 1) / (float)layerCount;
            }
        }

        #region ISerializable
        public TerrainWorker(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _areaPerFrame = info.GetInt32("AreaPerFrame");
            Resolution = info.GetInt32("Resolution");
            Function = (MapManager)info.GetValue("Function", typeof(MapManager));
            Layers = (List<float>)info.GetValue("Layers", typeof(List<float>));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("AreaPerFrame", _areaPerFrame);
            info.AddValue("Resolution", Resolution);
            info.AddValue("Function", Function);
            info.AddValue("Layers", Layers);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
