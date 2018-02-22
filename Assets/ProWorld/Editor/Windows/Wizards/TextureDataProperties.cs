using ProWorldEditor;
using UnityEditor;
using UnityEngine;

public class TextureDataProperties : ScriptableWizard
{
    public TextureEditorArea TextureD;
    public TextureEditorLayer Layer;
    private Vector2 _scroll = Vector2.zero;
    private bool _close;

    public delegate void OnChangeDelegate(TextureEditorArea td);

    public OnChangeDelegate OnChange;

    public static void CreateTDP(TextureEditorArea textureData, OnChangeDelegate onChange)
    {
        var tdp = DisplayWizard<TextureDataProperties>("Create Texture", "Apply");
        tdp.TextureD = textureData;
        //tdp.Ted = ted;
        tdp.OnChange = onChange;
    }

// ReSharper disable UnusedMember.Local
    private void Update()
// ReSharper restore UnusedMember.Local
    {
        if (_close || EditorApplication.isCompiling)
            Close();
    }

// ReSharper disable UnusedMember.Local
    private void OnGUI()
// ReSharper restore UnusedMember.Local
    {
        var width = position.width;
        var x = 5;

        var textures = ProWorld.Data.World.Textures;

        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.BeginHorizontal();

        if (textures.Count == 0)
        {
            GUILayout.Label("No textures assets added.");
        }
        else
        {
            foreach (var t in textures)
            {
                x += 64;
                if (x > width - 20)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    x = 5;
                    x += 64;
                }

                if (TextureD.Area.Splat == t) GUI.color = Color.red;
                if (GUILayout.Button(t.Texture, GUIStyle.none, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    //TextureD.Area.Splat = t;
                    TextureD.SetSplat(t);
                    Close();
                }
                GUI.color = Color.white;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        //if (Ted != null)
            if (GUILayout.Button("Remove"))
            {
                TextureD.SetSplat(null);
                Close();
            }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

// ReSharper disable UnusedMember.Local
    private void OnLostFocus()
// ReSharper restore UnusedMember.Local
    {
        // Can't close in here as it throws an error
        // WindowLayouts are invalid. Please use 'Window -> Layouts -> Revert Factory Settings...' menu to fix it.
        _close = true;
    }

// ReSharper disable UnusedMember.Local
    private void OnDestroy()
// ReSharper restore UnusedMember.Local
    {
        OnChange(TextureD);
    }
}