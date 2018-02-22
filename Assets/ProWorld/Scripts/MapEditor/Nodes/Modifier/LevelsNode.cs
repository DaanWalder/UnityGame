using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class LevelsNode : NodeData
    {
        public float Level = 0.05f;

        public LevelsNode(MapManager map)
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

            OutputData = Modifier.Levels(InputData[0], Level);
        }

        #region ISerializable
        public LevelsNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Level = (float) info.GetValue("Level", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Level", Level);
        }

        #endregion
    }
}
