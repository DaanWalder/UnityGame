using ProWorldEditor;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

public class TextureSplatProperties : ScriptableWizard
{
    public TextureEditorSplat TextureSplatOld;
    public TextureSplat TextureSplatNew;
    private bool _close;
    private bool _isPicking;

    public static void CreateTDP(TextureEditorSplat tes)
    {
        var tdp = DisplayWizard<TextureSplatProperties>("Create Splat Prototype", "Apply");
        tdp.minSize = new Vector2(222, 174);
        tdp.maxSize = new Vector2(222, 174);
        tdp.TextureSplatOld = tes;
        tdp.TextureSplatNew = new TextureSplat(tes.Splat);
    }

// ReSharper disable UnusedMember.Local
    private void Update()
// ReSharper restore UnusedMember.Local
    {
        if (EditorApplication.isCompiling || (_close && !_isPicking)) // Don't want to close if we lost focus from picking texture
        {
            IsRemove();
            Close();
        }
    }

// ReSharper disable UnusedMember.Local
    private void OnGUI()
// ReSharper restore UnusedMember.Local
    {
        if (Event.current.type == EventType.ExecuteCommand)
        {
            _isPicking = false;
            _close = false;
            Focus();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Texture: ", GUILayout.Width(100));
        var rect = GUILayoutUtility.GetRect(64, 64, EditorStyles.objectField, GUILayout.MaxHeight(64), GUILayout.MaxWidth(64));

        var mouse = Event.current.mousePosition;

        if (Event.current.type == EventType.MouseDown && rect.Contains(mouse))
        {
            _isPicking = true;
        }

        TextureSplatNew.Texture = (Texture2D)EditorGUI.ObjectField(rect, TextureSplatNew.Texture, typeof(Texture2D), false);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        TextureSplatNew.TileSize.X = EditorGUILayout.FloatField("Tile Size X", TextureSplatNew.TileSize.X);
        TextureSplatNew.TileSize.Y = EditorGUILayout.FloatField("Tile Size Y", TextureSplatNew.TileSize.Y);
        TextureSplatNew.TileOffset.X = EditorGUILayout.FloatField("Tile Offset X", TextureSplatNew.TileOffset.X);
        TextureSplatNew.TileOffset.Y = EditorGUILayout.FloatField("Tile Offset Y", TextureSplatNew.TileOffset.Y);

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove"))
        {
            Remove();
            Close();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply"))
        {
            if (!IsRemove())
            {
                TextureSplatOld.UpdateTexture(TextureSplatNew);
            }

            Close();
        }
        GUILayout.EndHorizontal();
    }

    private bool IsRemove()
    {
        if (TextureSplatNew.Texture == null)
        {
            Remove();
            return true;
        }
        return false;
    }
    private void Remove()
    {
        ProWorld.Data.Textures.Remove(TextureSplatOld);
        ProWorld.Data.World.Textures.Remove(TextureSplatOld.Splat);
    }
// ReSharper disable UnusedMember.Local
    private void OnDestroy()
// ReSharper restore UnusedMember.Local
    {
        IsRemove();
    }
// ReSharper disable UnusedMember.Local
    private void OnLostFocus()
// ReSharper restore UnusedMember.Local
    {
        // Can't close in here as it throws an error so we just set a flag
        // WindowLayouts are invalid. Please use 'Window -> Layouts -> Revert Factory Settings...' menu to fix it.
        _close = true;
    }
// ReSharper disable UnusedMember.Local
    private void OnFocus()
// ReSharper restore UnusedMember.Local
    {
        _close = false;
    }
}