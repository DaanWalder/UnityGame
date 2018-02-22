using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class InvertNode : NodeData
    {
        public InvertNode(MapManager map)
            : base(map)
        {
            SetInput(1);
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            if (!InputConnections[0])
            {
                OutputData = new float[resolution, resolution];
                return;
            }

            InputData[0] = InputConnections[0].From.OutputData;

            OutputData = Modifier.Invert(InputData[0]);
        }

        #region ISerializable
        public InvertNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        #endregion
    }
}
