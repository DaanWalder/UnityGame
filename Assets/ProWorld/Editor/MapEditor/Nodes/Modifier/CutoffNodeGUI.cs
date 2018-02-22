using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class CutoffNodeGUI : Node
    {
        public new static string Title = "Cutoff";
        public static NodeType Type = NodeType.Modifier;

        public CutoffNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new CutoffNode(mapEditor.Data.Map), Title)
        {
        }

        public override void Options()
        {
            var stretch = (CutoffNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("MinAfter:", GUILayout.Width(100));
            stretch.Min = GUILayout.HorizontalSlider(stretch.Min, 0, 1, GUILayout.Width(100));
            GUILayout.Label(stretch.Min.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("MaxAfter:", GUILayout.Width(100));
            stretch.Max = GUILayout.HorizontalSlider(stretch.Max, 0, 1, GUILayout.Width(100));
            GUILayout.Label(stretch.Max.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Handle Min:", GUILayout.Width(100));
            //stretch.MinHandle = (Modifier.CutoffHandle)MyGUI.EnumButton(stretch.MinHandle, GUILayout.Width(100));
            stretch.MinHandle = (Modifier.CutoffHandle)EditorGUILayout.EnumPopup(stretch.MinHandle, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Handle Max:", GUILayout.Width(100));
            //stretch.MaxHandle = (Modifier.CutoffHandle)MyGUI.EnumButton(stretch.MaxHandle, GUILayout.Width(100));
            stretch.MaxHandle = (Modifier.CutoffHandle)EditorGUILayout.EnumPopup(stretch.MaxHandle, GUILayout.Width(100));

            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected CutoffNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion
    }
}
