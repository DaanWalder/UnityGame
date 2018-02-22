using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    public static class Bar
    {
        public static void OnGUI()
        {
            GUILayout.BeginHorizontal();
            for (var index = 0; index < ProWorld.Windows.Count; index++)
            {
                var name = ProWorld.Windows[index].Title;

                if (GUILayout.Button(name, EditorStyles.miniButtonMid, GUILayout.Width(100)))
                {
                    ProWorld.SetWindow(index);
                }
            }
            GUILayout.FlexibleSpace();

            if (ProWorld.Data != null)
            {
                if (ProWorld.Data.IsRealTime) GUI.color = Color.red;
                if (GUILayout.Button("Preview", EditorStyles.miniButtonMid))
                {
                    ProWorld.Data.IsRealTime = !ProWorld.Data.IsRealTime;
                }
            }

            GUI.color = Color.white;

            GUILayout.Space(20);
            if (GUILayout.Button("Assets", EditorStyles.miniButtonMid))
            {
                var windows = ProWorld.Windows;
                if (windows != null)
                {
                    var exists = false;

                    for (int index = 0; index < windows.Count; index++)
                    {
                        var window = windows[index];
                        if (window is ImportAssets)
                        {
                            exists = true;

                            if (index != windows.Count - 1)
                            {
                                windows.Remove(window);
                                ProWorld.NewWindow(window);
                            }

                            break;
                        }
                    }

                    if (!exists)
                        ProWorld.NewWindow(new ImportAssets());
                }
            }

            GUILayout.Space(20);

            if (GUILayout.Button("New", EditorStyles.miniButtonMid))
            {
                ProWorld.Initialize();
                ProWorld.Data = new EditorData();
                ProWorld.NewWindow(new WorldWindow());
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Save", EditorStyles.miniButtonMid))
            {
                var path = EditorUtility.SaveFilePanel("Save to file", @"Assets\ProWorld\ProWorldSavedData", "proworld.pw", "pw");

                if (path.Length != 0)
                {
                    FileOperations.SaveToFile(path, ProWorld.Data);
                }
            }
            if (GUILayout.Button("Load", EditorStyles.miniButtonMid))
            {
                var path = EditorUtility.OpenFilePanel("Save to file", @"Assets\ProWorld\ProWorldSavedData", "pw");

                if (path.Length != 0)
                {
                    //ProWorld.World.Clean();
                    ProWorld.Initialize();
                    ProWorld.Data = FileOperations.LoadFromFile(path);
                    ProWorld.NewWindow(new WorldWindow());
                }

            }
            if (GUILayout.Button("Apply", EditorStyles.miniButtonMid))
            {
                var windows = ProWorld.Windows;
                if (windows != null)
                {
                    var exists = false;
                    
                    for (int index = 0; index < windows.Count; index++)
                    {
                        var window = windows[index];
                        if (window is ApplyWindow)
                        {
                            exists = true;

                            if (index != windows.Count - 1)
                            {    
                                windows.Remove(window);
                                ProWorld.NewWindow(window);
                            }
                            break;
                        }
                    }

                    if (!exists)
                        ProWorld.NewWindow(new ApplyWindow());
                }
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
    }
}