using System;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class EntityEditorGroup
    {
        private const int PreviewResolution = 512;

        [NonSerialized] public Texture2D AreaTexture;
        [NonSerialized] public Texture2D GeneratedTexture;
        [NonSerialized] private Color[] _generatedTextureColor;

        public EntityGroup Entity;

        public EntityEditorGroup(EntityGroup entity)
        {
            Entity = entity;
            GeneratedTexture = new Texture2D(1, 1);
        }

        public void UpdateTextures(int resolution)
        {
            var c = CombineMaps();

            var resizedArray = Util.ResizeArray(c, resolution);
            Util.ApplyBoolMapToTexture(ref AreaTexture, resizedArray);

            UnityEngine.Object.DestroyImmediate(GeneratedTexture);
            GeneratedTexture = new Texture2D(PreviewResolution, PreviewResolution);
            GeneratedTexture.SetPixels(_generatedTextureColor);
            GeneratedTexture.Apply();
        }
        public void GenerateTreeTexture()
        {
            var size = ProWorld.Data.World.TerrainWidth;

            _generatedTextureColor = new Color[PreviewResolution*PreviewResolution];

            var nfactor = PreviewResolution/(float) size;

            foreach (var box in Entity.Boxes)
            {
                foreach (var tree in box.Objects)
                {
                    var x = (int) (tree.Position.x*nfactor);
                    var y = (int) (tree.Position.z*nfactor);
                    //var r = (int)Mathf.Max(tree.Data.Radius[0], tree.Data.Radius[1], 1);
                    //var r = 1;

                    _generatedTextureColor[y*PreviewResolution + x] = Color.green;
                }
            }
        }

        private bool[,] CombineMaps()
        {
            if (Entity.Area.Count == 0)
                return new bool[1,1];

            var size = Entity.Area[0].MaskArea.GetLength(0);
            var b = new bool[size,size];

            for(var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
// ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var area in Entity.Area)
// ReSharper restore LoopCanBeConvertedToQuery
                    {
                        if (area.MaskArea[y, x])
                        {
                            b[y, x] = true;
                            break;
                        }
                    }
                }
            }

            return b;
        }
    }
}
