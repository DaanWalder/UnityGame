using System;
using System.Globalization;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class ErosionNodeGUI : Node
    {
        public new static string Title = "Erosion";
        public static NodeType Type = NodeType.GenMod;

        public ErosionNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new ErosionNode(mapEditor.Data.Map), Title)
        {

        }

        public override void Options()
        {
            var en = (ErosionNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Iterations:", GUILayout.Width(100));
            en.Iterations = (int)MyGUI.LogSlider(en.Iterations, 0, 2, GUILayout.Width(100));
            GUILayout.Label(en.Iterations.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Type:", GUILayout.Width(100));
            en.Type = (ErosionNode.ErosionType)MyGUI.EnumSlider((int)en.Type, Enum.GetNames(typeof(ErosionNode.ErosionType)).Length - 1, GUILayout.Width(100));
            GUILayout.Label(en.Type.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected ErosionNodeGUI(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }
}