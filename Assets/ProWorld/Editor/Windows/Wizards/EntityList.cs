using System.Collections.Generic;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

public class EntityList : ScriptableWizard
{
    public List<EntityData> CurrentEntities;
    private Vector2 _scroll = Vector2.zero;
    private bool _close;

    public delegate void OnSelectionDelegate(List<EntityData> e);
    public OnSelectionDelegate OnSelection;

    private readonly List<EntityData> _currentEntity = new List<EntityData>();

    public static void Show(List<EntityData> currentEntities, OnSelectionDelegate onSelection)
    {
        var el = DisplayWizard<EntityList>("Create Texture", "Apply");
        el.CurrentEntities = currentEntities;
        el.OnSelection = onSelection;
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

        var entities = ProWorld.Data.World.Entities;

        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.BeginHorizontal();

        foreach (var e in entities)
        {
#if UNITY_4_0
            var preview = e.Prefab ? AssetPreview.GetAssetPreview(e.Prefab) : Util.White;
#else
            var preview = e.Prefab ? EditorUtility.GetAssetPreview(e.Prefab) : Util.White;
#endif

            if (!CurrentEntities.Contains(e))
            {
                x += 64;
                if (x > width - 20)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    x = 5;
                    x += 64;
                }

                if (_currentEntity.Contains(e)) GUI.color = Color.red;
                if (GUILayout.Button(preview, GUIStyle.none, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    if (_currentEntity.Contains(e))
                        _currentEntity.Remove(e);
                    else
                        _currentEntity.Add(e);
                }
                GUI.color = Color.white;
            }
            GUI.color = Color.white;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        //if (Ted != null)
        if (GUILayout.Button("Add"))
        {
            OnSelection(_currentEntity);
            Close();
        }
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
        //OnSelection();
    }
}