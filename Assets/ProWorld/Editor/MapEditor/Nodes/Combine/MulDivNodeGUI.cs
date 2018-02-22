using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class MulDivNodeGUI : Node
    {
        public new static string Title = "MulDiv";
        public static NodeType Type = NodeType.Combine;

        private int _inputs;

        public MulDivNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new MulDivNode(mapEditor.Data.Map), Title)
        {
            _inputs = 2;
        }

        public override void Options()
        {
            var an = (MulDivNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mul or Div:", GUILayout.Width(100));
            an.Op = (MulDivNode.Operation)MyGUI.EnumSlider((int)an.Op, Enum.GetNames(typeof(MulDivNode.Operation)).Length - 1, GUILayout.Width(50));
            GUILayout.Label(an.Op.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Inputs:", GUILayout.Width(100));
            var oldInput = _inputs;
            _inputs = MyGUI.RoundSlider(_inputs, 1, 4, GUILayout.Width(100));
            GUILayout.Label(_inputs.ToString(CultureInfo.InvariantCulture), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            if (_inputs != oldInput)
            {
                foreach (var input in Data.InputConnections.Where(input => input))
                {
                    input.From.OutputConnections.Remove(input);
                }

                Links = new LinkGUI[_inputs];
                an.SetInput(_inputs);

                Run();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Constant:", GUILayout.Width(100));
            an.Constant = MyGUI.LogSlider(an.Constant, -2, 2, GUILayout.Width(100));
            GUILayout.Label(an.Constant.ToString("0.00"), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected MulDivNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _inputs = info.GetInt32("Inputs");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Inputs", _inputs);
        }
        #endregion
    }
}