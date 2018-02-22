using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class EditorWaterAsset : ISerializable
    {
        public string Path { get; set; }
        public WaterAsset Water { get; private set; }

        public EditorWaterAsset(WaterAsset water)
        {
            Water = water;
        }

        public void UpdateObject(GameObject go)
        {
            Water.Prefab = go;
            Path = AssetDatabase.GetAssetPath(go);
        }

        public EditorWaterAsset(SerializationInfo info, StreamingContext context)
        {
            Path = info.GetString("Path");
            Water = (WaterAsset)info.GetValue("Water", typeof(WaterAsset));

            try
            {
                Water.Prefab = (GameObject)AssetDatabase.LoadAssetAtPath(Path, typeof(GameObject));
            }
            catch (Exception) // If texture missing throw exception
            {
                throw new UnityException("No object found at " + Path);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Path", Path);
            info.AddValue("Water", Water);
        }
    }
}
