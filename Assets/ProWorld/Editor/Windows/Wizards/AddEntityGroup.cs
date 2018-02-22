using System;
using System.Collections.Generic;
using ProWorldEditor;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

public class AddEntityGroup : ScriptableWizard
{
    private const int TextureAreaSize = 256;
    private const int DataSize = 256;

    private TextureEditorLayer _textureLayer;

    private readonly List<TextureArea> _textureAreas = new List<TextureArea>();

    private Texture2D _texture;
    private bool _close;

    public delegate void OnAddDelegate(List<TextureArea> ta);
    private OnAddDelegate _addDelegate;

    public static void Create(TextureEditorLayer textureLayer, OnAddDelegate add)
    {
        var aeg = DisplayWizard<AddEntityGroup>("Create Entity Group");
        aeg.minSize = new Vector2(257,309);
        aeg.maxSize = new Vector2(257, 309);

        aeg._textureLayer = textureLayer;
        aeg._addDelegate = add;
        aeg._texture = new Texture2D(DataSize, DataSize);
        aeg.ApplyTexture();
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
        GUILayout.Label("Select regions to place trees");

        GUILayout.BeginHorizontal();
        var rect = GUILayoutUtility.GetRect(TextureAreaSize, TextureAreaSize, TextureAreaSize, TextureAreaSize);

        Graphics.DrawTexture(rect, _texture, 0, 0, 0, 0);

        if (Event.current.type == EventType.MouseDown)
        {
            var pickpos = Event.current.mousePosition;

            if (rect.Contains(pickpos))
            {
                var x = (Convert.ToInt32(pickpos.x) - (int) rect.x); // *ratio;
                var y = (Convert.ToInt32(pickpos.y) - (int) rect.y); // *ratio;

                const float ratio = DataSize / (float)TextureAreaSize;

                var xx = (int)(x*ratio);
                var yy = (int) ((rect.height - y - 1)*ratio);

                foreach (var t in _textureLayer.Layer.Textures)
                {
                    if (t.MaskArea[yy, xx])
                    {
                        if (_textureAreas.Contains(t))
                            _textureAreas.Remove(t);
                        else
                            _textureAreas.Add(t);

                        ApplyTexture();
                        break;
                    }
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Add", GUILayout.Width(100)))
        {
            _addDelegate(_textureAreas);
            Close();
        }
    }

   
    private void ApplyTexture()
    {
        //var textureWorker = ProWorld.Data.World.TextureData;

        var color = new Color[DataSize*DataSize];

        for (var y = 0; y < DataSize; y++)
        {
            for (var x = 0; x < DataSize; x++)
            {
                var found = false;

                for (int index = 0; index < _textureLayer.Layer.Textures.Count; index++)
                {
                    var t = _textureLayer.Layer.Textures[index];
                    if (t.MaskArea[y, x])
                    {
                        var c = _textureAreas.Contains(t) ? Color.yellow : _textureLayer.Texture[index].Splat.PreviewColor;
                        color[y * DataSize + x] = c;
                        found = true;
                        break;
                    }
                }

                if (!found) color[y * DataSize + x] = Color.black;
            }
        }

        _texture.SetPixels(color);
        _texture.Apply();

        Repaint();
    }

    /*public static Color[] DivideAreasHighlight(TextureEditorLayer tl, int highlight)
    {
        var t = tl.Texture;

        Debug.Log(t[0].Area.MaskArea.GetLength(0));

        var color = new Color[0];

        /*var size = tl. data.GetLength(0);
        var color = new Color[size * size];

        

        var lColor = new Color[t.Count];

        

        // Pregenerate colors for speed
        for (var index = 0; index < lColor.Length; index++)
        {
            var d = index / (float)t.Count;

            const int r = 0;
            var g = d * 0.75f + 0.25f;
            var b = d * 0.1875f + 0.0625f;

            lColor[index] = new Color(r, g, b);
        }

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                for (var i = 0; i < cutoffs.Count; i++)
                {
                    if (data[y, x] <= cutoffs[i])
                    {
                        var c = lColor[i];

                        if (i == highlight)
                        {
                            c = Color.yellow;
                        }
                        else if (data[y, x] < waterLevel)
                        {
                            //c.g = c.g;
                            c.b = 1;
                        }

                        color[y * size + x] = c;

                        break;
                    }
                }
            }
        }*/

        /*return color;
    }*/

// ReSharper disable UnusedMember.Local
    private void OnLostFocus()
// ReSharper restore UnusedMember.Local
    {
        // Can't close in here as it throws an error
        // WindowLayouts are invalid. Please use 'Window -> Layouts -> Revert Factory Settings...' menu to fix it.
        _close = true;
    }
}