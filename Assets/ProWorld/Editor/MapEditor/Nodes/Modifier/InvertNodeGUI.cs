using System;
using System.Runtime.Serialization;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class InvertNodeGUI : Node
    {
        public new static string Title = "Invert";
        public static NodeType Type = NodeType.Modifier;

        public InvertNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new InvertNode(mapEditor.Data.Map), Title)
        {
        }

        #region ISerialize
        protected InvertNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        #endregion
    }
}
