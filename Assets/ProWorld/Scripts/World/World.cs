using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class World : ISerializable
    {
        private const int CurrentVersion = 11;
        public int Version = CurrentVersion;

        #region Constants
        private const int DefaultTerrainResolution = 513;
        private const int DefaultTextureResolution = 512;
        #endregion

        private int _terrainWidth = 1000;
        private int _terrainHeight = 100;
        private int _treeDistance = 1000;
        private int _billboardStart = 50;
        private int _fadeLength = 5;
        private int _treeMaximumFullLODCount = 50;

        // Terrain Properties
        public int TerrainWidth
        {
            get { return _terrainWidth; }
            set { _terrainWidth = value; }
        }
        public int TerrainHeight
        {
            get { return _terrainHeight; }
            set { _terrainHeight = value; }
        }

        // Tree Properties
        public int TreeDistance
        {
            get { return _treeDistance; }
            set { _treeDistance = value; }
        }
        public int BillboardStart
        {
            get { return _billboardStart; }
            set { _billboardStart = value; }
        }
        public int FadeLength
        {
            get { return _fadeLength; }
            set { _fadeLength = value; }
        }
        public int TreeMaximumFullLODCount
        {
            get { return _treeMaximumFullLODCount; }
            set { _treeMaximumFullLODCount = value; }
        }

        // Workers
        public TerrainWorker TerrainData { get; set; }
        public TextureWorker TextureData { get; set; }
        public EntityWorker EntityData { get; set; }
        public List<Worker> CustomWorkers { get; set; }

        // Assets
        public List<TextureSplat> Textures { get; set; }
        public List<EntityData> Entities { get; set; }
        public WaterAsset Water { get; set; } 

        public World()
        {
            TerrainData = new TerrainWorker(DefaultTerrainResolution);
            TextureData = new TextureWorker(DefaultTextureResolution, 3);
            EntityData = new EntityWorker(this, 2000);
            CustomWorkers = new List<Worker>();

            Textures = new List<TextureSplat>();
            Entities = new List<EntityData>();
            Water = new WaterAsset();
        }

        public void SetLayers(int layers)
        {
            TerrainData.SetLayers(layers);
            TextureData.SetLayers(layers);
            EntityData.SetLayers(layers);
        }

        public World(SerializationInfo info, StreamingContext context)
        {
            Version = info.GetInt32("Version");

            TerrainData = (TerrainWorker)info.GetValue("TerrainData", typeof(TerrainWorker));
            TextureData = (TextureWorker)info.GetValue("TextureData", typeof(TextureWorker));
            EntityData = (EntityWorker)info.GetValue("EntityData", typeof(EntityWorker));

            Textures = (List<TextureSplat>)info.GetValue("Textures", typeof(List<TextureSplat>));
            Entities = (List<EntityData>)info.GetValue("Entities", typeof(List<EntityData>));
            Water = (WaterAsset)info.GetValue("Water", typeof(WaterAsset));

            _terrainWidth = info.GetInt32("TerrainWidth");
            _terrainHeight = info.GetInt32("TerrainHeight");
            _treeDistance = info.GetInt32("TreeDistance");
            _billboardStart = info.GetInt32("BillboardStart");
            _fadeLength = info.GetInt32("FadeLength");
            _treeMaximumFullLODCount = info.GetInt32("TreeMaximumFullLodCount");

            // Version Changes
            if (Version < 11)
            {
                CustomWorkers = new List<Worker>();
            }
            else
            {
                CustomWorkers = (List<Worker>)info.GetValue("CustomWorkers", typeof(List<Worker>));
            }

            Version = CurrentVersion;

        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", Version);

            info.AddValue("TerrainData", TerrainData);
            info.AddValue("TextureData", TextureData);
            info.AddValue("EntityData", EntityData);

            info.AddValue("Textures", Textures);
            info.AddValue("Entities", Entities);
            info.AddValue("Water", Water);

            info.AddValue("TerrainWidth", _terrainWidth);
            info.AddValue("TerrainHeight", _terrainHeight);
            info.AddValue("TreeDistance", _treeDistance);
            info.AddValue("BillboardStart", _billboardStart);
            info.AddValue("FadeLength", _fadeLength);
            info.AddValue("TreeMaximumFullLodCount", _treeMaximumFullLODCount);

            info.AddValue("CustomWorkers", CustomWorkers);
        }
    }
}
