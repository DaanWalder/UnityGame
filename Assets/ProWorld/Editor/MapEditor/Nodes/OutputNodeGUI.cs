using System;
using System.Runtime.Serialization;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class OutputNodeGUI : Node
    {
        public new static String Title = "Output";
        public static NodeType Type = NodeType.Output;

        public OutputNodeGUI(MapEditor mapEditor, OutputNode node)
            : base(mapEditor, node, Title)
        {
            IsHasOutput = false;
        }
        #region ISerialize
        protected OutputNodeGUI(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion
    }
}