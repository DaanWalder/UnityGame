using System;
using System.Globalization;
using System.Runtime.Serialization;
using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class LevelsNodeGUI : Node
    {
        public new static string Title = "Levels";
        public static NodeType Type = NodeType.Modifier;

        public LevelsNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new LevelsNode(mapEditor.Data.Map), Title)
        {

        }

        public override void Options()
        {

            var level = (LevelsNode) Data;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Level:", GUILayout.Width(100));
            level.Level = MyGUI.LogSlider(level.Level, -2, 0, GUILayout.Width(100));
            level.Level = (float) Math.Round(level.Level, 2);
            GUILayout.Label(level.Level.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));
            GUILayout.EndHorizontal();

            base.Options();
        }

        #region ISerialize
        protected LevelsNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion
    }
}
