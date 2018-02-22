using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public sealed class EditorData : ISerializable
    {
        public const int TerrainSize = 512;
        public const int TextureSize = 256;

        public static string Folder = "ProWorldSavedData";

        public World World { get; private set; }
        public WorldData WorldData;

        public TerrainWorkerEditor Terrain { get; private set; }

        public List<TextureEditorLayer> Texture { get; private set; }
        public List<EntityEditorLayer> Entity { get; private set; }
        public MapEditorData EntityDensityMap { get; private set; }

        public List<TextureEditorSplat> Textures { get; private set; }
        public List<EntityEditorData> Entities  { get; private set; }
        public EditorWaterAsset Water { get; private set; }

        public bool IsRealTime { get; set; }

        public EditorData()
        {
            World = new World();
            WorldData = new WorldData();
            Terrain = new TerrainWorkerEditor(WorldData, World.TerrainData);
            Texture = new List<TextureEditorLayer>();
            Entity = new List<EntityEditorLayer>();
            SetupMapmanager(); // Build a default density map for trees

            Textures = new List<TextureEditorSplat>();
            Entities = new List<EntityEditorData>();
            Water = new EditorWaterAsset(World.Water);

            SetLayers(3);
        }

        private void SetupMapmanager()
        {
            var dm = World.EntityData.Generator.DensityMap;

            EntityDensityMap = new MapEditorData(dm);
            var me = new MapEditor(EntityDensityMap, null);

            var sn = new SimplexNodeGUI(me)
                         {
                             WindowRect = new Rect(100, 140, 102, 100)
                         };
            ((SimplexNode) (sn.Data)).Range = 4;

            var fbm = new FractalBrownianMotionNodeGUI(me)
                          {
                              WindowRect = new Rect(350, 140, 121, 100)
                          };
            ((FractalBrownianMotionNode)(fbm.Data)).FBm.Gain = 0.6f;
            ((FractalBrownianMotionNode)(fbm.Data)).FBm.Lacunarity = 0.7f;
            
            me.AddNode(sn);
            me.AddNode(fbm);

            me.OutputLink(sn);
            me.InputLink(fbm, 0);

            me.OutputLink(fbm);
            me.InputLink(EntityDensityMap.Output, 0);
        }

        public void SetLayers(int layers)
        {
            World.SetLayers(layers);

            if (Texture.Count > layers)
            {
                for (var i = Texture.Count - 1; i >= layers; i--)
                {
                    Texture.RemoveAt(i);
                    Entity.RemoveAt(i);
                }
            }
            else if (Texture.Count < layers)
            {
                var c = Texture.Count;
                for (var i = 0; i < layers - c; i++)
                {
                    Texture.Add(new TextureEditorLayer(World.TextureData.Generator.TextureLayers[Texture.Count]));
                    Entity.Add(new EntityEditorLayer(World.EntityData.Generator.EntityLayers[Entity.Count]));
                }
            }
        }

        public TextureEditorSplat GetEditorSplat(TextureSplat splat)
        {
            return Textures.FirstOrDefault(t => t.Splat == splat);
        }

        public Color GetPreviewColor(TextureSplat splat)
        {
            foreach (var t in Textures)
            {
                if (t.Splat == splat)
                {
                    return t.PreviewColor;
                }
            }

            return Color.black;
        }

        public TextureEditorSplat AddTexture(TextureSplat ts)
        {
            World.Textures.Add(ts);

            var tes = new TextureEditorSplat(ts);
            Textures.Add(tes);

            return tes;
        }
        public EntityEditorData AddEntity(EntityData ed)
        {
            World.Entities.Add(ed);

            var eed = new EntityEditorData(ed);
            Entities.Add(eed);

            return eed;
        }

        public EditorData(SerializationInfo info, StreamingContext context)
        {
            World = (World)info.GetValue("World", typeof (World));
            WorldData = (WorldData)info.GetValue("WorldData", typeof(WorldData));

            Terrain = (TerrainWorkerEditor)info.GetValue("Terrain", typeof(TerrainWorkerEditor));
            Texture = (List<TextureEditorLayer>)info.GetValue("Texture", typeof(List<TextureEditorLayer>));
            Entity = (List<EntityEditorLayer>)info.GetValue("Entity", typeof(List<EntityEditorLayer>));

            EntityDensityMap = (MapEditorData) info.GetValue("EntityDensityMap", typeof (MapEditorData));
            
            Textures = (List<TextureEditorSplat>)info.GetValue("Textures", typeof(List<TextureEditorSplat>));
            Entities = (List<EntityEditorData>)info.GetValue("Entities", typeof(List<EntityEditorData>));
            Water = (EditorWaterAsset) info.GetValue("Water", typeof (EditorWaterAsset));

            IsRealTime = info.GetBoolean("IsRealTime");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("World", World);
            info.AddValue("WorldData", WorldData);
            
            info.AddValue("Terrain", Terrain);
            info.AddValue("Texture", Texture);
            info.AddValue("Entity", Entity);

            info.AddValue("EntityDensityMap", EntityDensityMap);

            info.AddValue("Textures", Textures);
            info.AddValue("Entities", Entities);
            info.AddValue("Water", Water);

            info.AddValue("IsRealTime", IsRealTime);
        }
    }
}