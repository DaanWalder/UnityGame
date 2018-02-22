using UnityEditor;
using UnityEngine;

class AboutProWorld : EditorWindow
{
    [MenuItem("Window/ProWorld/Changes")]
    public static void CreateWindow()
    {
        GetWindowWithRect<AboutProWorld>(new Rect(Screen.currentResolution.width - 165, Screen.currentResolution.height - 160, 330, 320), true, "News");
    }

    private Vector2 _scroll;

    public void OnGUI()
    {
        GUILayout.Label("Welcome to ProWorld 2.x");
        GUILayout.Space(5);
        GUILayout.Label("What's new:");

        _scroll = GUILayout.BeginScrollView(_scroll, false, true);
        var ww = GUI.skin.label.wordWrap;
        GUI.skin.label.wordWrap = true;

        GUILayout.Space(5);
        GUILayout.Label("# 2.0.1");
        GUILayout.Space(5);
        GUILayout.Label("-Fixed various bugs.\n" +
                        "-ADVANCED: Add your own custom workers.\n" +
                        "\t-Check demonstration code comments.\n" +
                        "-Video tutorial up. Check readme.txt", GUILayout.Width(300));

        GUILayout.Space(5);
        GUILayout.Label("# 2.0");
        GUILayout.Space(5);
        GUILayout.Label("-Full rewrite to allow for future expandability.\n" +
                        "-New layout including the new preview window.\n" +
                        "-World generation on the fly at runtime.\n" +
                        "-Real-time preview while editing.\n" +
                        "-New nodes including erosion and area splitting.\n" +
                        "-Algorithm optimization; up to 20x faster.\n" +
                        "-Entity placement density map.\n" +
                        "-and more...", GUILayout.Width(300));
        
        GUILayout.Space(5);
        GUILayout.Label("For detailed tutorial video please check forums.");

        GUILayout.Space(5);
        GUILayout.Label("Backwards Compatibility:");
        GUILayout.Label("-2.0 fundamentally different to 1.0.\n" +
                        "-Unable provide backwards compatibility with it.\n" +
                        "-Archived copy of 1.0 included if required.\n" +
                        "-All future versions backwards compatible.", GUILayout.Width(300));

        GUILayout.Space(5);
        GUILayout.Label("Contact me anytime for help, to report a bug or \ngive a suggestion.", GUILayout.Width(300));
        GUILayout.Label("-Forum ID: tertle\n" +
                        "-Email: tim@bovinelabs.com", GUILayout.Width(300));
       
        GUI.skin.label.wordWrap = ww;

        GUILayout.EndScrollView();
    }
}