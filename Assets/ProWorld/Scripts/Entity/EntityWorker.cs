using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProWorldSDK
{
    [Serializable]
    public class EntityWorker : Worker, ILayer
	{
        public int Density = 2000;

        private readonly int _size;

        public EntityGenerator Generator { get; private set; }

        [NonSerialized]private DateTime _startTime;
        [NonSerialized]private double _duration;
        [NonSerialized]private bool _applyCollider;
        [NonSerialized]private bool _isApplySettings;

        public EntityWorker(World world, int size)
        {
            _size = size;
            Generator = new EntityGenerator(world);

            Priority = 8;
        }

        public void SetLayers(int layers)
        {
            if (Generator.EntityLayers.Count > layers)
            {
                for (var i = Generator.EntityLayers.Count - 1; i >= layers; i--)
                {
                    Generator.EntityLayers.RemoveAt(i);
                }
            }
            else if (Generator.EntityLayers.Count < layers)
            {
                var c = Generator.EntityLayers.Count;
                for (var i = 0; i < layers - c; i++)
                {
                    Generator.EntityLayers.Add(new EntityLayer());
                }
            }
        }

        public override void Generate(object sender, DoWorkEventArgs e)
        {
            var data = (WorldData)e.Argument;

            // TODO STORE BOXES
            var entities = Generator.GetEntities(data);
            data.EntityToPlace = new Queue<Entity>(entities);
        }

        public override bool Apply(WorldData data, DateTime starTime, double duration)
        {
            _startTime = starTime;
            _duration = duration;

            if (data.EntityToPlace == null) return false;
            if (!data.Terrain) return true;

            if (!_isApplySettings)
            {
                var td = data.Terrain.terrainData;

                var entities = new List<GameObject>();
                foreach (var e in data.EntityToPlace)
                {
                    if (!entities.Contains(e.Data.Prefab))
                        entities.Add(e.Data.Prefab);
                }

                var tps = new TreePrototype[entities.Count];

                for (var i = 0; i < entities.Count; i++)
                {
                    tps[i] = new TreePrototype
                    {
                        prefab = entities[i]
                    };
                }

                td.treePrototypes = tps;
                td.RefreshPrototypes();

                // Clear any previous trees
                td.treeInstances = new TreeInstance[0];

                _isApplySettings = true;

                return false;
            }

            if (!_applyCollider)
            {
                if (ApplyTree(data.Terrain, data.EntityToPlace))
                {
                    _applyCollider = true;
                }

                // we either didn't complete all work or we did but we don't want to apply colliders till next frame anyway
                return false;
            }

            return true;
        }

        private bool ApplyTree(Terrain terrain, Queue<Entity> entities)
        {
            var td = terrain.terrainData;

            while (entities.Count > 0)
            {
                var entity = entities.Dequeue();

                var pos = entity.Position;
                pos /= td.size.x;
                pos.y = 0;

                var index = 0;
                for (var i = 0; i < td.treePrototypes.Length; i++)
                {
                    if (td.treePrototypes[i].prefab == entity.Data.Prefab)
                    {
                        index = i;
                        break;
                    }
                }
                
                var color = Random.Range(0.6f, 1f); //0.973f

                var ti = new TreeInstance
                {
                    color = new Color(color, color, color, 1.000f),
                    heightScale = 1,
                    lightmapColor = new Color(1.000f, 1.000f, 1.000f, 1.000f),
                    widthScale = 1,
                    position = pos,
                    prototypeIndex = index
                };

                terrain.AddTreeInstance(ti);
                terrain.Flush();

                if ((DateTime.Now - _startTime).TotalMilliseconds > _duration / 2f) // We dedicate half the time to updating physics
                {
                    return false;
                }
            }

            return true;
        }

        #region ISerializable
        protected EntityWorker(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Density = info.GetInt32("Density");
            _size = info.GetInt32("Size");
            Generator = (EntityGenerator)info.GetValue("Generator", typeof(EntityGenerator));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Density", Density);
            info.AddValue("Size", _size);
            info.AddValue("Generator", Generator);
        }

        #endregion
    }
}
