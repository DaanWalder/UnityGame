using System;
using System.Globalization;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class CellNoiseNodeGUI : Node
    {
        public new static string Title = "CellNoise";
        public static NodeType Type = NodeType.Generator;

        public CellNoiseNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new CellNoiseNode(mapEditor.Data.Map), Title)
        {
            var cn = (CellNoiseNode)Data;
            cn.Range = 8f;
        }

        public override void Options()
        {
            var cn = (CellNoiseNode)Data;
            var noise = (CellNoise) cn.Noise;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset X:", GUILayout.Width(100));
            cn.OffsetX = MyGUI.StaticSlider(cn.OffsetX, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(cn.OffsetX.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset Y:", GUILayout.Width(100));
            cn.OffsetY = MyGUI.StaticSlider(cn.OffsetY, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(cn.OffsetY.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Range:", GUILayout.Width(100));
            cn.Range = MyGUI.LogSlider(cn.Range, -1, 2, GUILayout.Width(100));
            GUILayout.Label(cn.Range.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Dim:", GUILayout.Width(100));
            noise.Dim = MyGUI.RoundSlider(noise.Dim, 2, 3, GUILayout.Width(100));
            GUILayout.Label(noise.Dim.ToString(CultureInfo.InvariantCulture), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if (noise.Dim > 2)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Z:", GUILayout.Width(100));
                noise.Z = MyGUI.StaticSlider(noise.Z, float.MinValue, float.MaxValue, 100);
                GUILayout.Label(noise.Z.ToString("0.00"), GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }

            /*GUILayout.BeginHorizontal();
            GUILayout.Label("Type:", GUILayout.Width(50));
            cn.Cell.Type = (cn.cnType)MyGUI.EnumSlider((int)cn.Type, 6, GUILayout.Width(50));
            GUILayout.Label(cn.Type.ToString(), GUILayout.Width(100));
            GUILayout.EndHorizontal();*/

            GUILayout.BeginHorizontal();
            GUILayout.Label("Type:", GUILayout.Width(100));
            //noise.Type = (VoronoiType)MyGUI.EnumSlider((int)noise.Type, 5, GUILayout.Width(50));
            noise.Type = (VoronoiType)EditorGUILayout.EnumPopup(noise.Type, GUILayout.Width(100));
            //GUILayout.Label(noise.Type.ToString(), GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance:", GUILayout.Width(100));
            //noise.DistType = (DistanceMethod)MyGUI.EnumSlider((int)noise.DistType, 5, GUILayout.Width(50));
            noise.DistType = (DistanceMethod)EditorGUILayout.EnumPopup(noise.DistType, GUILayout.Width(100));
            //GUILayout.Label(noise.DistType.ToString(), GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected CellNoiseNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion
    }
}