using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class StretchNodeGUI : Node
    {
        public new static String Title = "Stretch";
        public static NodeType Type = NodeType.Modifier;

        public StretchNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new StretchNode(mapEditor.Data.Map), Title)
        {

        }

        public override void Options()
        {
            var stretch = (StretchNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("MinBefore:", GUILayout.Width(100));
            stretch.MinBefore = GUILayout.HorizontalSlider(stretch.MinBefore, 0, 1, GUILayout.Width(100));
            GUILayout.Label(stretch.MinBefore.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("MaxBefore:", GUILayout.Width(100));
            stretch.MaxBefore = GUILayout.HorizontalSlider(stretch.MaxBefore, 0, 1, GUILayout.Width(100));
            GUILayout.Label(stretch.MaxBefore.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("MinAfter:", GUILayout.Width(100));
            stretch.MinAfter = GUILayout.HorizontalSlider(stretch.MinAfter, 0, 1, GUILayout.Width(100));
            GUILayout.Label(stretch.MinAfter.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("MaxAfter:", GUILayout.Width(100));
            stretch.MaxAfter = GUILayout.HorizontalSlider(stretch.MaxAfter, 0, 1, GUILayout.Width(100));
            GUILayout.Label(stretch.MaxAfter.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();
            
            base.Options();
        }

        #region ISerialize
        protected StretchNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        #endregion
    }
}
