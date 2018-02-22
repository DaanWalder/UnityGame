using System;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class AreaNodeGUI : Node
    {
        public new static string Title = "Area";
        public static NodeType Type = NodeType.Combine;

        public AreaNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new AreaNode(mapEditor.Data.Map), Title)
        {
           
        }

        public override void Options()
        {
            var an = (AreaNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Direction:", GUILayout.Width(100));
            an.Direction = (Direction)MyGUI.EnumButton(an.Direction, GUILayout.Width(100));
            GUILayout.Label(an.Direction.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Cutoff:", GUILayout.Width(100));
            an.Cutoff = MyGUI.StaticSlider(an.Cutoff, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(an.Cutoff.ToString("0.00"), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Blur:", GUILayout.Width(100));
            an.Blur = (AreaNode.BlurMode)MyGUI.EnumButton(an.Blur, GUILayout.Width(100));
            GUILayout.Label(an.Blur.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("BlurArea:", GUILayout.Width(100));
            an.BlurArea = GUILayout.HorizontalSlider(an.BlurArea, 0, 1, GUILayout.Width(100));
            GUILayout.Label(an.BlurArea.ToString("0.00"), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected AreaNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
        #endregion
    }
}