using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class WaterAsset : ISerializable
    {
        //public bool IsUseWater = true;
        public float WaterLevel = 0.2f;
        public GameObject Prefab;

        public WaterAsset()
        {
            
        }

        public WaterAsset(SerializationInfo info, StreamingContext context)
        {
            //IsUseWater = info.GetBoolean("IsUseWater");
            WaterLevel = (float)info.GetValue("WaterLevel", typeof (float));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("IsUseWater", IsUseWater);
            info.AddValue("WaterLevel", WaterLevel);
        }
    }
}
