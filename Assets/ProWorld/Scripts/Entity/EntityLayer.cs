using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class EntityLayer : ISerializable
    {
        public List<EntityGroup> Groups { get; private set; }

        [NonSerialized] public Box[] Entities = new Box[0];

        public EntityLayer()
        {
            Groups = new List<EntityGroup>();
        }
        public void AddEntityGroup(EntityGroup eg)
        {
            Groups.Add(eg);
        }
        public void RemoveEntityGroup(EntityGroup eg)
        {
            Groups.Remove(eg);
        }

        public EntityLayer(SerializationInfo info, StreamingContext context)
        {
            Groups = (List<EntityGroup>)info.GetValue("Groups", typeof(List<EntityGroup>));

            Entities = new Box[0];
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Groups", Groups);
        }
    }

    [Serializable]
    public class EntityGroup : ISerializable
    {
        public int Density { get; set; }
        public int Seed { get; set; }
        public SerializableDictionary<EntityData, float> Entities { get; private set; }

        public List<TextureArea> Area { get; private set; }
        public bool IsUseDensityMap { get; set; }
        public bool IsInWater { get; set; }

        [NonSerialized] public Box[] Boxes = new Box[0];

        public EntityGroup(List<TextureArea> area)
        {
            Density = 2000;

            Entities = new SerializableDictionary<EntityData, float>();

            Area = area;

            IsUseDensityMap = true;
        }

        public EntityGroup(SerializationInfo info, StreamingContext context)
        {
            Density = info.GetInt32("Density");
            Seed = info.GetInt32("Seed");
            Entities = (SerializableDictionary<EntityData, float>) info.GetValue("Entities", typeof (SerializableDictionary<EntityData, float>));
            Area = (List<TextureArea>) info.GetValue("Area", typeof (List<TextureArea>));
            IsUseDensityMap = info.GetBoolean("IsUseDensityMap");
            IsInWater = info.GetBoolean("IsInWater");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Density", Density);
            info.AddValue("Seed", Seed);
            info.AddValue("Entities", Entities);
            info.AddValue("Area", Area);
            info.AddValue("IsUseDensityMap", IsUseDensityMap);
            info.AddValue("IsInWater", IsInWater);
        }
    }

    [Serializable]
    public class Entity : ISerializable
    {
        public EntityData Data;
        public Vector3 Position;
        //public float Density = 1;

        public Entity(EntityData td)
        {
            Data = td;
        }

        public Entity(SerializationInfo info, StreamingContext context)
        {
            Data = (EntityData) info.GetValue("Data", typeof (EntityData));
            Position = ((SerializableVector3) info.GetValue("Position", typeof (SerializableVector3))).ToVector3();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data", Data);
            info.AddValue("Position", new SerializableVector3(Position));
        }
    }

    [Serializable]
    public class EntityData : ISerializable
    {
        private GameObject _prefab;
        public GameObject Prefab
        {
            get { return _prefab; }
            set
            {
                _prefab = value;
                CheckCollider();
            }
        }

        public bool IsHasCollider { get; private set; }

        public readonly float[] Radius = new[] { 0.2f, 1 }; // 0 is trunk or shrub radius, 1 is top tree radius

        public EntityData()
        {

        }

        public EntityData(EntityData entityData)
        {
            Prefab = entityData.Prefab;
            Radius = entityData.Radius;
        }

        public void Set(EntityData td)
        {
            Prefab = td.Prefab;
            Radius[0] = td.Radius[0];
            Radius[1] = td.Radius[1];
        }

        private void CheckCollider()
        {
            if (_prefab == null)
            {
                IsHasCollider = false;
                return;
            }

            IsHasCollider = _prefab.GetComponent<Collider>() != null;
        }

        public EntityData(SerializationInfo info, StreamingContext context)
        {
            Radius = (float[])info.GetValue("Radius", typeof(float[]));
            IsHasCollider = info.GetBoolean("IsHasCollider");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Radius", Radius);
            info.AddValue("IsHasCollider", IsHasCollider);
        }
    }

    [Serializable]
    public class Box
    {
        public List<Entity> Objects = new List<Entity>();

        public void AddObject(Entity go)
        {
            Objects.Add(go);
        }
    }
}
