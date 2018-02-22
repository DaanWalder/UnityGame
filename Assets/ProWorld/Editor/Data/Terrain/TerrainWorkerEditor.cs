using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class TerrainWorkerEditor : ISerializable
    {
        public WorldData Data;
        public TerrainWorker Worker { get; private set; }
        public MapEditorData Function { get; private set; }
        public int Resolution = 512;

        public TerrainWorkerEditor(WorldData data, TerrainWorker terrain)
        {
            Worker = terrain;
            Data = data;
            Function = new MapEditorData(terrain.Function);
        }

        public void Generate(Vector2 zoom, float range = 1f)
        {
            Data.Heights = Worker.GetTerrain(Resolution, zoom, range);
            Data.LayerMasks = Worker.SplitLayers(Data.Heights);
        }
        public void Generate(WorldData data, int resolution, Vector2 zoom, float range = 1f)
        {
            data.Heights = Worker.GetTerrain(resolution, zoom, range);
            data.LayerMasks = Worker.SplitLayers(data.Heights);
        }


        #region ISerializable
        public TerrainWorkerEditor(SerializationInfo info, StreamingContext context)
        {
            Data = (WorldData) info.GetValue("Data", typeof (WorldData));
            Worker = (TerrainWorker)info.GetValue("Worker", typeof(TerrainWorker));
            Function = (MapEditorData)info.GetValue("Function", typeof(MapEditorData));
            Resolution = info.GetInt32("Resolution");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data", Data);
            info.AddValue("Worker", Worker);
            info.AddValue("Function", Function);
            info.AddValue("Resolution", Resolution);
        }
        #endregion
    }

}