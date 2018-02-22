using System;
using System.Collections.Generic;
using ProWorldSDK;
using UnityEngine;
#if !UNITY_EDITOR
using System.IO;
#endif

public class WorldBuilder : MonoBehaviour
{
    private Manager _manager;
    public string WorldFileLocation;
    public World World;
    public int Neighbours = 1;

    public List<Texture2D> Textures = new List<Texture2D>();
    public List<GameObject> Entities = new List<GameObject>();

    public GameObject Water;

	// Use this for initialization
	public void Awake ()
	{
#if !UNITY_EDITOR // World loading is handled manually within the editor using the BuildProcessor
        if (WorldFileLocation == string.Empty)
        {
            enabled = false;
            return;
        }

        // remove the pw extension and add the .world extension
        var path = Path.GetDirectoryName(WorldFileLocation) + Path.GetFileNameWithoutExtension(WorldFileLocation) + ".world";
	    path = Application.dataPath + "/ProWorld" + path;
        
        if (!File.Exists(path))
        {
            Debug.Log("World doesn't exist at location: " + path);
            enabled = false;
            return;
        }
        
        World = FileOperations.LoadFromFile(path);
#endif

        if (World == null) throw new UnityException("World is missing.");
	}

    public void Start()
    {
        if (Textures.Count != World.Textures.Count) throw new UnityException("Texture counts don't match.");
        if (Entities.Count != World.Entities.Count) throw new UnityException("Entity counts don't match.");

        for (var index = 0; index < Textures.Count; index++)
            World.Textures[index].Texture = Textures[index];

        for (var index = 0; index < Entities.Count; index++)
            World.Entities[index].Prefab = Entities[index];

        World.Water.Prefab = Water;

        _manager = new Manager(World, Neighbours);
    }

    public void Update()
    {
        var position = transform.position;
        var index = new Point(position) / World.TerrainWidth;

        if (transform.position.x < 0) index.X -= 1;
        if (transform.position.z < 0) index.Y -= 1;

        // Checks to see if there is a new heightmap, if so creates terrain and applies it
        // Run in main thread
        _manager.Update();
        // Remove terrain that is now out of range
        // Run in main thread
        _manager.CleanupTerrain(position);
        // Create terrain heightmap at a point and it's neighbours
        // This is done in a seperate thread
        _manager.CreateTerrainWithNeighbours(index);
    }

    public void AddCustomWorker(Worker worker)
    {
        if (World == null) throw new Exception("Either world hasn't been set or you're all AddCustomWorker in Awake(). Use Start() instead.");
        
        World.CustomWorkers.Add(worker);
    }

    public void RemoveCustomWorker(Worker worker)
    {
        World.CustomWorkers.Remove(worker);
    }
}
