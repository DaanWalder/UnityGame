using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class TextureWorker : Worker, ILayer
    {
        private int _areaPerFrame = 128;
        public int AreaPerFrame
        {
            get { return _areaPerFrame; }
            set { _areaPerFrame = value >= TextureResolution ? TextureResolution : Util.UpperPowerOfTwo(value); }
        }

        public int TextureResolution = 512;

        public TextureGenerator Generator;

        [NonSerialized]private bool _isApplySettings;
        [NonSerialized]private int _index;

        public TextureWorker(int textureResolution, int layers = 1)
        {
            TextureResolution = textureResolution;

            Generator = new TextureGenerator();

            Priority = 4;

            SetLayers(layers);
        }

        public void SetLayers(int layers)
        {
            if (Generator.TextureLayers.Count > layers)
            {
                for (var i = Generator.TextureLayers.Count - 1; i >= layers; i--)
                {
                    Generator.TextureLayers.RemoveAt(i);
                }
            }
            else if (Generator.TextureLayers.Count < layers)
            {
                var c = Generator.TextureLayers.Count;
                for (var i = 0; i < layers - c; i++)
                {
                    Generator.TextureLayers.Add(new TextureLayer());
                }
            }
        }
        // TODO MOVE
        public TextureArea AddToLayer(TextureSplat ts, IMap mf, int layer)
        {
            if (layer >= Generator.TextureLayers.Count)
                throw new UnityException("Layer out of range");

            return Generator.TextureLayers[layer].Add(ts, mf);
        }

        public override void Generate(object sender, DoWorkEventArgs e)
        {
            var data = (WorldData)e.Argument;

            data.Alphas = Generator.GetAlphas(data, TextureResolution);
            data.SplitAlphas = SplitAlphas(data.Alphas);
        }

        public override bool Apply(WorldData data, DateTime startTime, double duration)
        {
            if (data.SplitAlphas == null) return false;
            if (!data.Terrain) return true;

            if (!_isApplySettings)
            {
                data.Terrain.terrainData.alphamapResolution = TextureResolution;

                var textures = new List<TextureSplat>();
                foreach (var layer in Generator.TextureLayers)
                {
                    foreach (var t in layer.Textures)
                    {
                        if (!textures.Contains(t.Splat))
                            textures.Add(t.Splat);
                    }
                }

                data.Terrain.terrainData.splatPrototypes = textures.Select(s => s.ToSplatPrototype()).ToArray();

                _isApplySettings = true;
            }

            var alpha = data.SplitAlphas[_index];

            var width = TextureResolution / _areaPerFrame;

            var y = _index / width;
            var x = _index % width;

            data.Terrain.terrainData.SetAlphamaps(x * _areaPerFrame, y * _areaPerFrame, alpha);

            if (++_index == data.SplitAlphas.Count)
            {
                return true;
            }

            return false;
        }

        private List<float[,,]> SplitAlphas(float[,,] alphas)
        {
            var height = alphas.GetLength(0);
            var width = alphas.GetLength(1);
            var layers = alphas.GetLength(2);

            var sectionsX = width / _areaPerFrame;// +(width % _areaPerFrame > 0 ? 1 : 0);
            var sectionsY = height / _areaPerFrame;// +(height % _areaPerFrame > 0 ? 1 : 0);
            //tw.Width = sectionsX;

            var alphasList = new List<float[,,]>();

            var startY = 0;
            for (var yy = 0; yy < sectionsY; yy++)
            {
                var startX = 0;
                for (var xx = 0; xx < sectionsX; xx++)
                {
                    var alpha = new float[_areaPerFrame, _areaPerFrame, layers];

                    for (var y = 0; y < _areaPerFrame; y++)
                    {
                        for (var x = 0; x < _areaPerFrame; x++)
                        {
                            for (var l = 0; l < layers; l++)
                            {
                                alpha[y, x, l] = alphas[y + startY, x + startX, l];
                            }
                        }
                    }

                    alphasList.Add(alpha);
                    startX += _areaPerFrame;
                }
                startY += _areaPerFrame;
            }


            return alphasList;
        }

        #region ISerializable
        public TextureWorker(SerializationInfo info, StreamingContext context)
            :  base(info, context)
        {
            _areaPerFrame = info.GetInt32("AreaPerFrame");
            TextureResolution = info.GetInt32("TextureResolution");
            Generator = (TextureGenerator)info.GetValue("Generator", typeof(TextureGenerator));
        }
	    public override void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
            info.AddValue("AreaPerFrame", _areaPerFrame);
            info.AddValue("TextureResolution", TextureResolution);
            info.AddValue("Generator", Generator);

            base.GetObjectData(info, context);
	    }
        #endregion
    }
}
