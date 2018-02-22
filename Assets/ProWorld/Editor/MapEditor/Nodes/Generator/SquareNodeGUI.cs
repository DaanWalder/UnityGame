using System;
using System.Globalization;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class SquareNodeGUI : Node
    {
        public new static string Title = "Square";
        public static NodeType Type = NodeType.Generator;

        public SquareNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new SquareNode(mapEditor.Data.Map), Title)
        {
            

        }

        public override void Options()
        {
            var square = (SquareNode)Data;
            var noise = (Square)square.Noise;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset X:", GUILayout.Width(100));
            square.OffsetX = MyGUI.StaticSlider(square.OffsetX, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(square.OffsetX.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset Y:", GUILayout.Width(100));
            square.OffsetY = MyGUI.StaticSlider(square.OffsetY, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(square.OffsetY.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Range:", GUILayout.Width(100));
            square.Range = MyGUI.LogSlider(square.Range, -1, 3, GUILayout.Width(100));
            GUILayout.Label(square.Range.ToString("0.00"), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Frequency:", GUILayout.Width(100));
            noise.Frequency = GUILayout.HorizontalSlider(noise.Frequency, 0.1f, 2, GUILayout.Width(100));
            GUILayout.Label(noise.Frequency.ToString(CultureInfo.InvariantCulture), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Cutoff:", GUILayout.Width(100));
            noise.Cutoff = GUILayout.HorizontalSlider(noise.Cutoff, 0f, 1f, GUILayout.Width(100));
            GUILayout.Label(noise.Cutoff.ToString(CultureInfo.InvariantCulture), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected SquareNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
        #endregion

    }
}
