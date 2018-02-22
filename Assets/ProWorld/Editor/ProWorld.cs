using System;
using System.Collections.Generic;
using ProWorldSDK;
using ProWorldEditor;
using UnityEngine;
using UnityEditor;
using FileOperations = ProWorldEditor.FileOperations;

[Serializable]
public class ProWorld : EditorWindow
{
    public const int BarHeight = 20;

    public static List<WindowLayout> Windows = new List<WindowLayout>();

    private static bool _last; // State of isPlaying
    private static bool _load; // first load check

    private static Texture2D _texture; // Texture used as a hack to detect when a scene changes

    private static EditorData _data;
    public static EditorData Data
    {
        get { return _data; }
        set
        {
            _data = value;
        }
    }

    private static ProWorld _window;
    public static ProWorld Window
    {
        get
        {
            if (!_window)
            {
                _window = GetWindow<ProWorld>();
                _window.minSize = new Vector2(1246, 666);
            }
            return _window;
        }
        private set { _window = value; }
    }

    [MenuItem("Window/ProWorld/ProWorld")]
    public static void Init()
    {
        Window = GetWindow<ProWorld>();
        Window.minSize = new Vector2(1246, 666);

        Data = new EditorData();

        Windows.Clear();
        Windows.Add(new WorldWindow());

        var check = EditorPrefs.GetInt("ProWorldFirstLoad", 0);
        if (check != 2)
        {
            AboutProWorld.CreateWindow();
            EditorPrefs.SetInt("ProWorldFirstLoad", 2);
        }
    }

    static ProWorld()
    {
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
    }

    public static void Initialize()
    {
        foreach (var w in Windows)
        {
            w.Clean();
        }

        Windows.Clear();

        Data = null;

        //Show existing window instance. If one doesn't exist, make one.
        Window = GetWindow<ProWorld>();
        Window.minSize = new Vector2(1246, 666);
    }

    private bool _isCompiling;
    private bool _rebuild;

// ReSharper disable UnusedMember.Local
    private void Update()
// ReSharper restore UnusedMember.Local
    {
        Repaint(); // To frequent, change this only when line moves

        if (Windows.Count == 0 && !EditorApplication.isCompiling) // we've compiled and restarted
        {
            Initialize();

            Data = FileOperations.LoadTemp(); 

            Windows.Add(new WorldWindow());

            _isCompiling = false;
        }

        // If compiling quickly save a temp copy
        if (!_isCompiling && EditorApplication.isCompiling)
        {
            Clean();

            FileOperations.SaveTemp(Data);
            _isCompiling = true;
        }

        if (Util.White == null)
        {
            Util.White = new Texture2D(64, 64);
            var colors = new Color[64 * 64];

            for (var j = 0; j < 64; j++)
            {
                for (var i = 0; i < 64; i++)
                {
                    colors[j * 64 + i] = Color.white;
                }
            }

            Util.White.SetPixels(colors);
            Util.White.Apply();
        }

        if (Windows.Count != 0)
            Windows[Windows.Count - 1].Update();
    }

// ReSharper disable UnusedMember.Local
    private void OnGUI()
// ReSharper restore UnusedMember.Local
    {
        if (!_load)
        {
            _load = true;
            _texture = new Texture2D(0,0);
        }

        // On scene change textures are disappearing in 3.5.6 so we rebuild
        if (_texture == null) // This is my hacked way of detecting if we change scenes by using a dummy texture
        {
            _texture = new Texture2D(0, 0);

            Windows.Clear();
        }

        GUILayout.BeginArea(new Rect(0, BarHeight, position.width, position.height - BarHeight));
        if (Windows.Count != 0)
        {
            Windows[Windows.Count - 1].OnGUI();
        }
        GUILayout.EndArea();

        // Navigation bar
        GUILayout.BeginArea(new Rect(0, 0, position.width, BarHeight));
        Bar.OnGUI();
        GUILayout.EndArea();
    }

    public static void PlaymodeStateChanged()
    {
        // Aren't playing and we weren't previously (this happens because _last is reset when playing in unity engine)
        if (!Application.isPlaying && !_last)
        {
            FileOperations.SaveTemp(Data);
        }
        // Were playing now we aren't, regenerate windows because all temp textures are destroyed
        else if (!Application.isPlaying && _last)
        {
            Windows.Clear();
        }

        _last = Application.isPlaying;
    }

    private void Clean()
    {
        DestroyImmediate(Util.White);
    }

    public static void NewWindow(WindowLayout window)
    {
        Windows.Add(window);
    }

    public static void SetWindow(int index)
    {
        if (index == Windows.Count - 1)
            return;

        for (var i = Windows.Count - 1; i > index; i--)
        {
            Windows[i].Clean();
            Windows.RemoveAt(i);
        }

        Windows[Windows.Count - 1].Refresh();
    }

    public static void ResetWindow()
    {
        for (var i = Windows.Count - 1; i > 0; i--)
        {
            Windows[i].Clean();
            Windows.RemoveAt(i);
        }

        Windows[0].Refresh();
    }
}