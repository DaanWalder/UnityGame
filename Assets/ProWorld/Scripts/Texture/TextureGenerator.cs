using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class TextureGenerator : ITexture, ISerializable
    {
        public List<TextureLayer> TextureLayers = new List<TextureLayer>();

        public TextureGenerator()
        {
            
        }

        public float[, ,] GetAlphas(WorldData data, int resolution)
        {
            // Generate all layer textures
            GenerateAllLayerTextures(data.LayerMasks, resolution, data.Position.x, data.Position.y);
            BlurAlphas(resolution);
            return MergeAlphas(resolution, data.Heights);
        }

        #region Masks
        public void GenerateAllLayerTextures(int[,] map, int resolution, float offsetX = 0, float offsetY = 0)
        {
            // Generate all layer textures
            for (var index = 0; index < TextureLayers.Count; index++)
                GenerateLayerTextures(index, map, resolution, offsetX, offsetX);
        }

        public void GenerateLayerTextures(int layer, int[,] map, int resolution, float offsetX = 0, float offsetY = 0)
        {
            UpdateLayerMasks(TextureLayers[layer], resolution, offsetX, offsetY);
            CalculateMaskSections(layer, map, resolution);
        }
        public void UpdateLayerMasks(TextureLayer layer, int resolution, float offsetX = 0, float offsetY = 0)
        {
            // We go through update all masks based off the function, except for the bottom layer which is always all true
            if (layer.Textures.Count > 0)
            {
                layer.Textures[0].SetAsBase(resolution);
            }
            for (var index = 1; index < layer.Textures.Count; index++)
            {
                var area = layer.Textures[index];
                area.UpdateMask(resolution, offsetX, offsetY);
            }
        }

        public void CalculateMaskSections(int layer, int[,] layerMask, int resolution)
        {
            var textureLayer = TextureLayers[layer];
            if (textureLayer.Textures.Count == 0) return;

            if (layerMask == null) return;
            var map = Util.ResizeArray(layerMask, resolution);

            var left = Util.LayerToBool(map, layer);

            // Work top to bottom, but ignore bottom layer
            for (var i = textureLayer.Textures.Count - 1; i > 0; i--)
            {
                var t = textureLayer.Textures[i];
                t.MaskArea = new bool[resolution, resolution];

                for (var y = 0; y < resolution; y++)
                {
                    for (var x = 0; x < resolution; x++)
                    {
                        var val = left[y, x] && t.MaskFunction[y, x];

                        t.MaskArea[y, x] = val;

                        if (val)
                            left[y, x] = false;
                    }
                }
            }

            textureLayer.Textures[0].MaskArea = new bool[resolution, resolution];
            // Set bottom layer to anything leftover instead
            Array.Copy(left, textureLayer.Textures[0].MaskArea, left.Length);
        }
        #endregion
        #region Alpha
        public void BlurAlphas(int resolution)
        {
            foreach (var layer in TextureLayers)
            {
                var radius = resolution / 50;

                // Blur each layer
                foreach (var t in layer.Textures)
                {
                    var ms = Util.BoolToFloat(t.MaskArea);
                    t.Alpha = Smooth.SmoothGrayscale(ms, radius);
                }
            }
        }

        public float[, ,] MergeAlphas(int resolution, float[,] heightMap)
        {
            var textures = new List<TextureSplat>();
            foreach (var layer in TextureLayers)
            {
                foreach (var t in layer.Textures)
                {
                    if (!textures.Contains(t.Splat))
                        textures.Add(t.Splat);
                }
            }

            var alphaMapData = new float[resolution, resolution, textures.Count];

            // used to store any cells that are trying to fade with the same layer to fix at a later point
            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    foreach (var layer in TextureLayers)
                    {
                        foreach (var texture in layer.Textures)
                        {
                            var index = textures.FindIndex(s => s == texture.Splat);

                            if (index < 0)
                            {
                                //No texture found, must be empty layer. Should not happen
                                continue;
                            }

                            alphaMapData[y, x, index] += texture.Alpha[y, x];
                        }
                    }

                    var sum = 0f;
                    for (var l = 0; l < textures.Count; l++)
                    {
                        sum += alphaMapData[y, x, l];
                    }

                    for (var l = 0; l < textures.Count; l++)
                    {
                        alphaMapData[y, x, l] = alphaMapData[y, x, l]/sum;
                    }
                }
            }

            // Calculate fading between sections
            /*for (var index = 0; index < TextureLayers.Count; index++)
            {
                var tlayer = TextureLayers[index];
                var min = index - 1 >= 0 ? cutoffs[index - 1] - fadeSize : -fadeSize * 2;
                var max = index + 1 < cutoffs.Count ? cutoffs[index] + fadeSize : 1 + fadeSize * 2;

                var heights = Util.ResizeArray(heightMap, resolution);

                foreach (var texture in tlayer.Textures)
                {
                    var splat = texture.Splat;
                    var layer = textures.FindIndex(s => s == splat);

                    if (layer < 0)
                    {
                        //No texture found, must be empty layer. Should not happen
                        continue;
                    }

                    for (var y = 0; y < resolution; y++)
                    {
                        for (var x = 0; x < resolution; x++)
                        {
                            var height = heights[y, x];

                            // Calculate any fading between sections
                            /*float fade = 1;
                            if (height < min + fadeSize * 2)
                            {
                                fade = (height - min) / (fadeSize * 2);
                            }
                            else if (height > max - fadeSize * 2)
                            {
                                fade = (max - height) / (fadeSize * 2);
                            }*/

                            // If no value set, we don't want to apply anything. This avoids overriding previous data
                            /*if (Math.Abs(texture.Alpha[y, x]) > float.Epsilon)
                            {
                                // If we've already set a value it means we're trying to fade between the same texture so record this instead and fix it later
                                if (Math.Abs(alphaMapData[y, x, layer]) > float.Epsilon)
                                {
                                    //alphaMapData[y, x, layer] = 1f;//fade * texture.Alpha[y, x];
                                    updateFade.Add(new FadeUpdate(x, y, layer));
                                }
                                else
                                    alphaMapData[y, x, layer] = fade * texture.Alpha[y, x];

                            }

                            alphaMapData[y, x, layer] = texture.Alpha[y, x];
                        }
                    }
                }
            }*/

            // Fix all the same fades
            /*foreach (var f in updateFade)
            {
                var fadeValue = 0f;

                for (var l = 0; l < textures.Count; l++)
                {
                    if (f.Layer != l)
                    {
                        fadeValue += alphaMapData[f.Y, f.X, l];
                    }
                }

                alphaMapData[f.Y, f.X, f.Layer] = 1 - fadeValue;
            }*/

            return alphaMapData;
        }
        #endregion

        public TextureGenerator(SerializationInfo info, StreamingContext context)
        {
            TextureLayers = (List<TextureLayer>)info.GetValue("TextureLayers", typeof (List<TextureLayer>));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TextureLayers", TextureLayers);
        }
    }
}