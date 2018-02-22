using System;
using System.Globalization;
using System.Runtime.Serialization;
using UnityEngine;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class SimplexNodeGUI : Node
    {
        public new static string Title = "Simplex";
        public static NodeType Type = NodeType.Generator;

        private int _seed;

        public SimplexNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new SimplexNode(mapEditor.Data.Map), Title)
        {
            var simplex = (SimplexNoise)(((SimplexNode)Data).Noise);

            _seed = UnityEngine.Random.Range(-1000, 1000);
            simplex.SetSeed(_seed);
        }

        public override void Options()
        {
            var simplex = (SimplexNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed:", GUILayout.Width(100));
            var oldSeed = _seed;
            _seed = (int)MyGUI.StaticSlider(_seed, int.MinValue, int.MaxValue, 100, 0);
            GUILayout.Label(_seed.ToString(CultureInfo.InvariantCulture), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            if (_seed != oldSeed)
                ((SimplexNoise)(simplex.Noise)).SetSeed(_seed);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset X:", GUILayout.Width(100));
            simplex.OffsetX = MyGUI.StaticSlider(simplex.OffsetX, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(simplex.OffsetX.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset Y:", GUILayout.Width(100));
            simplex.OffsetY = MyGUI.StaticSlider(simplex.OffsetY, float.MinValue, float.MaxValue, 100);
            GUILayout.Label(simplex.OffsetY.ToString("0.00"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Range:", GUILayout.Width(100));
            simplex.Range = MyGUI.LogSlider(simplex.Range, -1, 3, GUILayout.Width(100));
            GUILayout.Label(simplex.Range.ToString("0.00"), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            var sNoise = (SimplexNoise) simplex.Noise;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Morph:", GUILayout.Width(100));
            sNoise.Morph = (OutputMorph)MyGUI.EnumSlider((int)sNoise.Morph, Enum.GetNames(typeof(OutputMorph)).Length - 1, GUILayout.Width(50));
            GUILayout.Label(sNoise.Morph.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected SimplexNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _seed = info.GetInt32("Seed");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Seed", _seed);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}