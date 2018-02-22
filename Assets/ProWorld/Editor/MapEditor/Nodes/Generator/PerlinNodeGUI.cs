using System;
using System.Runtime.Serialization;
using UnityEngine;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class PerlinNodeGUI : Node
    {
        public new static string Title = "Perlin";
        public static NodeType Type = NodeType.Generator;

        public PerlinNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new PerlinNode(mapEditor.Data.Map), Title)
        {
        }

        public override void Options()
        {
            var perlin = (PerlinNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset X:", GUILayout.Width(100));
            perlin.OffsetX = MyGUI.StaticSlider(perlin.OffsetX, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(perlin.OffsetX.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset Y:", GUILayout.Width(100));
            perlin.OffsetY = MyGUI.StaticSlider(perlin.OffsetY, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(perlin.OffsetY.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Range:", GUILayout.Width(100));
            perlin.Range = MyGUI.LogSlider(perlin.Range, -1, 3, GUILayout.Width(100));
            GUILayout.Label(perlin.Range.ToString("0.00"), GUILayout.Width(30));
            GUILayout.EndHorizontal();
            base.Options();
        }

        #region ISerialize
        protected PerlinNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}