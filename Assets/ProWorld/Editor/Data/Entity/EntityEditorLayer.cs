using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public sealed class EntityEditorLayer : ISerializable
    {
        [NonSerialized] private Color[] _generatedTextureColor;
        [NonSerialized] public Texture2D GeneratedTexture;

        public List<EntityEditorGroup> Entities { get; private set; }
        public EntityLayer Layer { get; private set; }

        public EntityEditorLayer(EntityLayer layer)
        {
            Layer = layer;
            Entities = new List<EntityEditorGroup>();
        }

        public void UpdateAllTextures(int resolution)
        {
            foreach (var t in Entities)
            {
                t.UpdateTextures(resolution);
            }

            ApplyTexture();
        }
        public void UpdateAreaTexture(int area, int resolution)
        {
            Entities[area].UpdateTextures(resolution);

            ApplyTexture();
        }
        private void ApplyTexture()
        {
            UnityEngine.Object.DestroyImmediate(GeneratedTexture);
            var resolution = (int)Mathf.Sqrt(_generatedTextureColor.Length);
            GeneratedTexture = new Texture2D(resolution, resolution);
            GeneratedTexture.SetPixels(_generatedTextureColor);
            GeneratedTexture.Apply();
        }

        public void GenerateAllTextures()
        {
            foreach (var t in Entities)
            {
                t.GenerateTreeTexture();
            }

            var size = ProWorld.Data.World.TerrainWidth;
            var world = ProWorld.Data.World.EntityData.Generator.Densisty;
            var resolution = world.GetLength(0);

            _generatedTextureColor = new Color[resolution * resolution];

            var nfactor = resolution / (float)size;

            foreach (var entity in Entities)
            {
                foreach (var box in entity.Entity.Boxes)
                {
                    foreach (var tree in box.Objects)
                    {
                        var x = (int) (tree.Position.x*nfactor);
                        var y = (int) (tree.Position.z*nfactor);

                        _generatedTextureColor[y * resolution + x] = Color.green;
                    }
                }
            }
        }

        public void AddToLayer(EntityGroup eg)
        {
            Layer.AddEntityGroup(eg);
            Entities.Add(new EntityEditorGroup(eg));
        }
        public void RemoveGroup(EntityEditorGroup eg)
        {
            Layer.RemoveEntityGroup(eg.Entity);
            Entities.Remove(eg);
        }

        public EntityEditorLayer(SerializationInfo info, StreamingContext context)
        {
            Entities = (List<EntityEditorGroup>) info.GetValue("Entities", typeof (List<EntityEditorGroup>));
            Layer = (EntityLayer) info.GetValue("Layer", typeof (EntityLayer));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Entities", Entities);
            info.AddValue("Layer", Layer);
        }
    }
}
