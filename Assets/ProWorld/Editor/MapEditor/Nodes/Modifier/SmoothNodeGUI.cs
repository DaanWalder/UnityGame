using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class SmoothNodeGUI : Node
    {
        public new static string Title = "Smooth";
        public static NodeType Type = NodeType.GenMod;

        public SmoothNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new SmoothNode(mapEditor.Data.Map), Title)
        {

        }

        public override void Options()
        {
            var sn = (SmoothNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", GUILayout.Width(100));
            //sn.Radius = GUILayout.HorizontalSlider(sn.Radius, 0, 0.5f, GUILayout.Width(100));
            sn.Radius = MyGUI.LogSlider(sn.Radius, -2, -0.5f, GUILayout.Width(100));
            GUILayout.Label(sn.Radius.ToString("0.000") + " (" + (ProWorld.Data.World.TerrainWidth * sn.Radius).ToString("0") + "m)", GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected SmoothNodeGUI(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }
}