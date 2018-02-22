using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ProWorldSDK
{
    public class WorkManager
    {
        public WorldData Data;

        private readonly Queue<Worker> _toCreate;// = new Queue<Worker>();
        private readonly List<Worker> _toUpdate = new List<Worker>();
        private readonly List<Worker> _toClean = new List<Worker>();

        private uint _priority;
        private bool _setHeights;
        private bool _setColliders;

        public WorkManager(World world, Terrain terrain, Vector2 position)
        {
            // duplicate world
            var worldCopy = MakeWorldCopy(world);

            // Manually copy textures across
            for (var index = 0; index < world.Textures.Count; index++)
                worldCopy.Textures[index].Texture = world.Textures[index].Texture;
            // Manually copy entities across
            for (var index = 0; index < world.Entities.Count; index++)
                worldCopy.Entities[index].Prefab = world.Entities[index].Prefab;

            Data = new WorldData(worldCopy, terrain, position);

            var worker = new List<Worker>(worldCopy.CustomWorkers)
                             {
                                 worldCopy.TerrainData, 
                                 worldCopy.TextureData, 
                                 worldCopy.EntityData
                             };

            var sortedWorkers = from w in worker
                                orderby w.Priority
                                select w;

            _toCreate = new Queue<Worker>(sortedWorkers);
        }

        private World MakeWorldCopy(World world)
        {
            using (var m = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(m, world);
                m.Position = 0;

                return (World)formatter.Deserialize(m);
            }
        }

        public bool Update(DateTime startTime, double duration)
        {
            while (_toCreate.Count > 0)
            {
                var w = _toCreate.Peek();

                // We only want to do work on same priority work
                if (w.Priority == _priority)
                {
                    // Create then move to update queue
                    w.Create(Data);
                    _toClean.Add(w);
                    _toUpdate.Add(_toCreate.Dequeue());
                }
                else
                {
                    // If all current priority work is done, we update priority
                    if (_toUpdate.Count == 0)
                    {
                        _priority = w.Priority;
                    }

                    break;
                }
            }

            if (_toUpdate.Count > 0)
            {
                for (var i = 0; i < _toUpdate.Count; i++)
                {
                    var w = _toUpdate[i];

                    if (w.Apply(Data, startTime, duration))
                    {
                        _toUpdate.Remove(w);
                        i--;

                        if (_toUpdate.Count == 0 && _toCreate.Count == 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - startTime).TotalMilliseconds > duration/2)
                            // if we've used more than half frame just return
                        {
                            break;
                        }
                    }
                }
            }

            return false;
        }

        public void Show()
        {
            if (!Data.Terrain) return;

#if UNITY_4_0
            Data.Terrain.gameObject.SetActive(true);
#else
            Data.Terrain.gameObject.active = true;
#endif
        }

        public void Clean()
        {
            foreach (var worker in _toClean)
            {
                worker.Clean();
            }
        }
    }
}