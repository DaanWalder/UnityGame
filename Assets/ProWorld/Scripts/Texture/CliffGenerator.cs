using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class CliffGenerator : IMap, ISerializable
    {
        private readonly TerrainWorker _terrain;
        public float Cutoff = 0.7f;

        public CliffGenerator(TerrainWorker terrain)
        {
            _terrain = terrain;
        }

        public float[,] GetArea(int resolution, float offsetX = 0, float offsetY = 0)
        {
            //_terrain.GetTerrain(new Vector2(offsetX, offsetY)); // TODO READD

            var mask = new float[resolution,resolution];

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    mask[y, x] = _terrain.GetSteepness(x / ((float)resolution - 1), y / ((float)resolution - 1)) > Cutoff ? 1f : 0f;
                }
            }

            return mask;
        }

        public CliffGenerator(SerializationInfo info, StreamingContext context)
        {
            _terrain = (TerrainWorker) info.GetValue("Terrain", typeof (TerrainWorker));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Terrain", _terrain);
        }
    }
}
