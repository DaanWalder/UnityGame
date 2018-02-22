using System.IO;
using ProWorldEditor;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(WorldBuilder))]
class WorldBuilderEditor : Editor
{
    private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
    private bool _rebuild;
    private string _rebuildPath = string.Empty;

    public override void OnInspectorGUI ()
    {
        var wb = (WorldBuilder) target;
        
        if (_rebuild) // rebuilding can only be done in main thread
        {
            Rebuild(_rebuildPath, wb);
            _rebuild = false;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("World:", GUILayout.Width(80));
        GUILayout.Label(wb.WorldFileLocation);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            var path = EditorUtility.OpenFilePanel("ProWorld file", @"Assets\ProWorld\"+EditorData.Folder, "pw");

            Rebuild(path, wb);
        }
        GUILayout.EndHorizontal();

        wb.Neighbours = EditorGUILayout.IntSlider("Neighbours:", wb.Neighbours, 0, 3);

        EditorGUILayout.LabelField("Textures", "Count: " + wb.Textures.Count);
        foreach (var t in wb.Textures)
        {
            EditorGUILayout.ObjectField(t, typeof(Texture2D), false, GUILayout.Width(128), GUILayout.Height(16));
        }

        EditorGUILayout.LabelField("Entities", "Count: " + wb.Entities.Count);
        foreach (var t in wb.Entities)
        {
            EditorGUILayout.ObjectField(t, typeof(GameObject), false, GUILayout.Width(128));
        }

        EditorGUILayout.LabelField("Water");
        EditorGUILayout.ObjectField(wb.Water, typeof(GameObject), false, GUILayout.Width(128));
    }

    public static void Rebuild(string path, WorldBuilder wb)
    {
        if (File.Exists(path))
        {
            var index = path.IndexOf(EditorData.Folder, System.StringComparison.Ordinal); // Find the index of the special folder name
            if (index != -1) // if the folder name exists
            {
                wb.WorldFileLocation = path.Substring(index + EditorData.Folder.Length); // cut from the end of the special folder name
            }
            else
            {
                Debug.Log("Please select file in " + Util.FirstCharToUpper(EditorData.Folder) + " folder.");
            }

            var editorData = ProWorldEditor.FileOperations.LoadFromFile(path);
            //wb.World = editorData.World;
            //Debug.Log(wb.World.Textures.Count);
            SetupAssets(editorData, wb);
        }
        else
        {
            wb.WorldFileLocation = null;
            SetupAssets(null, wb);
        }
    }

    private static void SetupAssets(EditorData editorData, WorldBuilder wb)
    {
        wb.Textures.Clear();
        wb.Entities.Clear();

        if (editorData == null) return;

        foreach (var t in editorData.Textures)
        {
            wb.Textures.Add(t.Splat.Texture);
        }
        foreach (var t in editorData.Entities)
        {
            wb.Entities.Add(t.Entity.Prefab);
        }

        wb.Water = editorData.Water.Water.Prefab;
    }

    public void OnEnable()
    {
        var wb = (WorldBuilder) target;

        var sourcePaths = Directory.GetDirectories(Application.dataPath, EditorData.Folder, SearchOption.AllDirectories);

        foreach (var sourcePath in sourcePaths)
        {
            var fullPath = sourcePath + wb.WorldFileLocation;

            if (File.Exists(fullPath))
            {
                Rebuild(fullPath, wb);
                CreateFileWatcher(fullPath);
            }
        }
    }
    public void OnDisable()
    {
        DisableFileWatcher();
    }
    public void OnDestroy()
    {
        DisableFileWatcher();
    }

    private void CreateFileWatcher(string path)
    {
        // Create a new FileSystemWatcher and set its properties.
        _watcher.Path = Path.GetDirectoryName(path);

        _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        // Only watch text files.
        _watcher.Filter = "*.pw";
        //_watcher.Filter = Path.GetFileName(path);

        // Add event handlers.
        _watcher.Changed += OnChanged;
        //watcher.Created += new FileSystemEventHandler(OnChanged);
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;

        // Begin watching.
        _watcher.EnableRaisingEvents = true;
    }
    private void DisableFileWatcher()
    {
        _watcher.Changed -= OnChanged;
        //watcher.Created += new FileSystemEventHandler(OnChanged);
        _watcher.Deleted -= OnDeleted;
        _watcher.Renamed -= OnRenamed;

        _watcher.EnableRaisingEvents = false;
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
        // Specify what is done when a file is changed, created, or deleted.
        _rebuild = true;
        _rebuildPath = e.FullPath;
    }

    private void OnDeleted(object source, FileSystemEventArgs e)
    {
        Debug.Log("Destroyed: " + e.FullPath);

        _rebuild = true;
        _rebuildPath = string.Empty;
    }

    private void OnRenamed(object source, RenamedEventArgs e)
    {
        Debug.Log("Renamed: " + e.FullPath);

        _rebuild = true;
        _rebuildPath = string.Empty;
    }
}
//*/