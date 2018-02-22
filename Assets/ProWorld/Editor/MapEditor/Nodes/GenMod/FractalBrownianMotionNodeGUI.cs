using System;
using System.Globalization;
using System.Runtime.Serialization;
using UnityEngine;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class FractalBrownianMotionNodeGUI : Node
    {
        public new static string Title = "fBm";
        public static NodeType Type = NodeType.GenMod;

        public FractalBrownianMotionNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new FractalBrownianMotionNode(mapEditor.Data.Map), Title)
        {

        }

        public INoise Noise;

        public override void Options()
        {
            var fbm = (FractalBrownianMotionNode) Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("h:", GUILayout.Width(100));
            fbm.FBm.H = MyGUI.LogSlider(fbm.FBm.H, -1, 1.477121255f, GUILayout.Width(100));
            GUILayout.Label(fbm.FBm.H.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Octaves:", GUILayout.Width(100));
            fbm.FBm.Octaves = (int)GUILayout.HorizontalSlider(fbm.FBm.Octaves, 1, 16, GUILayout.Width(100));
            GUILayout.Label(fbm.FBm.Octaves.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Gain:", GUILayout.Width(100));
            fbm.FBm.Gain = GUILayout.HorizontalSlider(fbm.FBm.Gain, 0, 1.5f, GUILayout.Width(100));
            GUILayout.Label(fbm.FBm.Gain.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Lacunarity:", GUILayout.Width(100));
            fbm.FBm.Lacunarity = MyGUI.LogSlider(fbm.FBm.Lacunarity, -1, 1, GUILayout.Width(100));
            GUILayout.Label(fbm.FBm.Lacunarity.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            /*GUILayout.BeginHorizontal();
            GUILayout.Label("Morph:", GUILayout.Width(100));
            fbm.FBm.Morph = (OutputMorph)MyGUI.EnumSlider((int)fbm.FBm.Morph, Enum.GetNames(typeof(OutputMorph)).Length - 1, GUILayout.Width(50));
            GUILayout.Label(fbm.FBm.Morph.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();*/

            base.Options();
        }

        #region ISerialize
        protected FractalBrownianMotionNodeGUI(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            Data = (FractalBrownianMotionNode)info.GetValue("fbm", typeof(FractalBrownianMotionNode));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("fbm", Data, typeof(FractalBrownianMotionNode));

            base.GetObjectData(info, context);
        }
        #endregion
    }
}