using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProWorldSDK
{
    public delegate void TerrainHandler(WorldData tw);
    public delegate void LoadedHandler(List<WorldData> tw);
    public delegate void FirstHandler();

    [Serializable]
	public class Manager
    {
        private const double FrameDuration = 1000.00 / 60.00; // Default 60fps

        public event TerrainHandler TerrainGenerated;
        public event LoadedHandler SetGenerated;
        public event FirstHandler FirstLoad;

        [NonSerialized]private bool _hasLoaded;
        [NonSerialized]private bool _last;

        public void OnSetGenerated(List<WorldData> data)
	    {
            if (SetGenerated != null)
                SetGenerated(data);

	        if (!_hasLoaded && FirstLoad != null) // run first load only
            {
                FirstLoad();
                _hasLoaded = true;
            }
	    }
	    private void OnTerrainGenerated(WorldData data)
	    {
            if (TerrainGenerated != null)
                TerrainGenerated(data);
	    }

	    private int _neighbours = 1;
	    public int Neighbours
	    {
	        get { return _neighbours; }
	        set
	        {
	            _neighbours = value;
	            TerrainDataPool.ResizePool(_neighbours);
	        }
	    }

        [NonSerialized]private readonly Dictionary<Point, Terrain> _terrains = new Dictionary<Point, Terrain>();
        [NonSerialized]private readonly List<Point> _existing = new List<Point>();

        //[NonSerialized]private readonly List<Worker> _worker = new List<Worker>(); // List of workers to do work on each piece of terrain
        [NonSerialized]private readonly List<WorkManager> _workers = new List<WorkManager>(); // Workers currently working
        [NonSerialized] private readonly List<WorkManager> _workersDone = new List<WorkManager>(); // Workers currently working
        [NonSerialized]private readonly List<WorldData> _eventDoneData = new List<WorldData>();

        [NonSerialized]private Point _currentPosition;
        [NonSerialized]private readonly Dictionary<Point, Terrain> _terrainToRemove = new Dictionary<Point, Terrain>();

        [NonSerialized]private int _delay;
        [NonSerialized]private bool _setColliders;
        [NonSerialized]private bool _setNeighbours;
        [NonSerialized]private int _updateIndex;

        private readonly World _world;

        public Manager(World world, int neighbours = 0)
        {
            _world = world;
            _neighbours = neighbours;

            TerrainDataPool.Setup(_neighbours);
        }

        public void Update()
        {
            if (_delay > 0)
            {
                _delay--;
                return;
            }

            if (!CheckTerrainOutOfArea())
            {
                if (RemoveTerrain()) return;
            }

            var startTime = DateTime.Now;

            if (_workers.Count != 0)
            {
                for (var i = 0; i < _workers.Count; i++)
                {
                    var w = _workers[i];

                    if (w.Update(startTime, FrameDuration))
                    {
                        _eventDoneData.Add(w.Data); // for event
                        _workersDone.Add(w);
                        _workers.Remove(w);

                        w.Show();
                        _setColliders = true;
                        _delay = 1;

                        OnTerrainGenerated(w.Data); // send terrain generated event
                        break;
                    }

                    if ((DateTime.Now - startTime).TotalMilliseconds > FrameDuration/2)
                        // if we've used more than half frame just return
                    {
                        break;
                    }
                }
            }
            else 
            {
                if (_setColliders)
                {
                    if (_updateIndex < _existing.Count)
                    {
                        SetCollider(_existing[_updateIndex++]);
                        _delay = 1;
                    }
                    else
                    {
                        _updateIndex = 0;
                        _setColliders = false; // this also has advantage of delaying 1 frame
                        _setNeighbours = true;
                    }
                }
                else if (_setNeighbours)
                {
                    if (_updateIndex < _existing.Count)
                    {
                        SetNeighbours(_existing[_updateIndex++]);
                    }
                    else
                    {
                        _updateIndex = 0;
                        _setNeighbours = false; // this also has advantage of delaying 1 frame
                    }
                }
            }

            if (_last  && !_setNeighbours) // just finished setting all neighbours, fire off the completed set event
            {
                OnSetGenerated(_eventDoneData);
                _eventDoneData.Clear();
            }

            _last = _setNeighbours;
        }

	    public void CreateTerrainWithNeighbours(Point index)
        {
            if (_workers.Count != 0) return;
            if (_terrainToRemove.Count != 0) return; // Clean up old terrain first to free up terrainData

            var x = index.X;
            var y = index.Y;

            for (var j = y - _neighbours; j <= y + _neighbours; j++)
            {
                for (var i = x - _neighbours; i <= x + _neighbours; i++)
                {
                    var p = new Point(i, j);

                    if (!_existing.Contains(p))
                    {
                        _existing.Add(p);

                        var terrain = GenerateTerrain(p);
                        _workers.Add(new WorkManager(_world, terrain, p.ToVector2()));
                    }
                    else
                    {
                        if (_terrainToRemove.ContainsKey(p))
                        {
                            _terrainToRemove.Remove(p);
                        }
                    }
                }
            }
        }
        public void CleanupTerrain(Vector3 position)
        {
            _currentPosition = new Point(position);
        }
        private bool CheckTerrainOutOfArea()
        {
            var clean = new List<Point>();

            var isDone = true;

            var size = _world.TerrainWidth;

            foreach (var t in _terrains)
            {
                var point = t.Key;
                var terrainPosition = point * size + size / 2;

                if (Mathf.Abs(terrainPosition.X - _currentPosition.X) > size * (_neighbours + 1) || Mathf.Abs(terrainPosition.Y - _currentPosition.Y) > size * (_neighbours + 1))
                {
#if UNITY_4_0
                    _terrains[point].gameObject.SetActive(false); // if we going to remove it, may as well hide it
#else
                    _terrains[point].gameObject.active = false; // if we going to remove it, may as well hide it
#endif
                    _terrainToRemove.Add(point, _terrains[point]);
                    clean.Add(point);

                    // only set 1 per frame, but if we set one we might not be done so come back later
                    isDone = false;
                    break; 
                }
            }

            foreach (var c in clean)
            {
                _terrains.Remove(c);
            }

            return isDone;
        }

        private bool RemoveTerrain()
        {
            if (_terrainToRemove.Count == 0) return false;

            var p = new Point(); // always gets assigned

            foreach (var t in _terrainToRemove)
            {
                p = t.Key;
                var terrain = t.Value;

                TerrainDataPool.FreeUp(terrain.terrainData);
                terrain.terrainData = null;
                Object.Destroy(terrain.gameObject);

                var worker = _workersDone.Find(w => w.Data.Position == p.ToVector2());
                if (worker != null)
                {
                    worker.Clean();
                    _workersDone.Remove(worker);
                }

                break; // Do 1 per frame
            }

            _terrainToRemove.Remove(p);
            _existing.Remove(p);
            _delay = 1;

            return true;
        }

        private Terrain GenerateTerrain(Point index)
        {
            var x = index.X;
            var y = index.Y;

            var size = _world.TerrainWidth;

            var go = new GameObject("Terrain (" + x.ToString("000") + "," + y.ToString("000") + ")");
            go.transform.position = new Vector3(x*size, 0, y*size);
#if UNITY_4_0
            go.SetActive(false); // if we going to remove it, may as well hide it
#else
            go.active = false;
#endif

            var terrain = go.AddComponent<Terrain>();
            var tc = go.AddComponent<TerrainCollider>();
            var td = TerrainDataPool.GetNext();

            tc.terrainData = td;

            terrain.terrainData = td;
            terrain.treeDistance = _world.TreeDistance;
            terrain.treeBillboardDistance = _world.BillboardStart;
            terrain.treeCrossFadeLength = _world.FadeLength;
            terrain.treeMaximumFullLODCount = _world.TreeMaximumFullLODCount;

            // Water
            var waterData = _world.Water;

            if (waterData.Prefab != null)
            {
                var water = ((GameObject) Object.Instantiate(waterData.Prefab)).transform;
                water.parent = terrain.transform;
                water.name = "Water";


                var v = Vector3.zero;
                v.x = _world.TerrainWidth/2f;
                v.y = waterData.WaterLevel * _world.TerrainHeight;
                v.z = _world.TerrainWidth / 2f;
                water.localPosition = v;
            }

            if (!_terrains.ContainsKey(index))
                _terrains.Add(index, terrain);

            return terrain;
        }

        private void SetCollider(Point index)
        {
            _terrains[index].terrainData.SetHeights(0,0, new float[0,0]);
        }
        private void SetNeighbours(Point index)
        {
            var left = CheckTerrain(index + new Point(-1, 0));
            var top = CheckTerrain(index + new Point(0, 1));
            var right = CheckTerrain(index + new Point(1, 0));
            var bottom = CheckTerrain(index + new Point(0, -1));

            var res = _world.TerrainData.Resolution;

            if (top != null)
            {
                var heights = top.terrainData.GetHeights(0, 0, res, 1);
                _terrains[index].terrainData.SetHeights(0, res - 1, heights);
            }
            if (right != null)
            {
                var heights = right.terrainData.GetHeights(0, 0, 1, res);
                _terrains[index].terrainData.SetHeights(res - 1, 0, heights);
            }

            // fix any corner
            var topRight = CheckTerrain(index + new Point(1, 1));
            if (topRight != null)
            {
                var heights = topRight.terrainData.GetHeights(0, 0, 1, 1);
                _terrains[index].terrainData.SetHeights(res - 1, res - 1, heights);
            }

            _terrains[index].SetNeighbors(left, top, right, bottom);
            _terrains[index].Flush();
        }

	    private Terrain CheckTerrain(Point index)
	    {
	        return _terrains.ContainsKey(index) ? _terrains[index] : null;
	    }
	}
}
