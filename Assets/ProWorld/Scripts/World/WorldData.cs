using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class WorldData : ISerializable
    {
        public World World { get; private set; }
        public Terrain Terrain { get; private set; }
        public Vector2 Position = Vector2.zero;

        #region Terrain
        public float[,] Heights { get; set; }
        public int[,] LayerMasks { get; set; }

        // Terrain real time placement
        public List<float[,]> SplitTerrain { get; set; }
        #endregion

        #region Texture
        public float[, ,] Alphas { get; set; }

        // Texture real time placement
        public List<float[, ,]> SplitAlphas { get; set; }
        #endregion

        #region Entity
        //public Entity[] Entities { get; set; }
        public Queue<Entity> EntityToPlace { get; set; }
        #endregion

        public WorldData(World world, Terrain terrain, Vector2 position)
        {
            World = world;
            Terrain = terrain;
            Position = position;
        }
        public WorldData()
        {
            Terrain = null;
            Position = Vector2.zero;
        }

        public bool IsPointInMask(int layer, int x, int y)
        {
            return LayerMasks[y, x] == layer;
        }

        public WorldData(SerializationInfo info, StreamingContext context)
        {
            Position = Vector2.zero;
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }
    }
}
