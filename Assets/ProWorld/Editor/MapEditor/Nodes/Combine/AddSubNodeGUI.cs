using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class AddSubNodeGUI : Node
    {
        private int _inputs;
        public new static string Title = "AddSub";
        public static NodeType Type = NodeType.Combine;

        public AddSubNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new AddSubNode(mapEditor.Data.Map), Title)
        {
            _inputs = 2;
        }

        public override void Options()
        {
            var an = (AddSubNode)Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Add or Sub:", GUILayout.Width(100));
            an.Op = (AddSubNode.Operation)MyGUI.EnumSlider((int)an.Op, Enum.GetNames(typeof(AddSubNode.Operation)).Length - 1, GUILayout.Width(50));
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
            an.Constant = GUILayout.HorizontalSlider(an.Constant, 0, 1, GUILayout.Width(100));
            GUILayout.Label(an.Constant.ToString("0.00"), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Morph:", GUILayout.Width(100));
            an.Morph = (Combination.Morph)MyGUI.EnumSlider((int)an.Morph, Enum.GetNames(typeof(Combination.Morph)).Length - 1, GUILayout.Width(50));
            GUILayout.Label(an.Morph.ToString(), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected AddSubNodeGUI(SerializationInfo info, StreamingContext context) : base(info, context)
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