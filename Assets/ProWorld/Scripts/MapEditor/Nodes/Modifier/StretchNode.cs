using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class StretchNode : NodeData
    {
        public float MinAfter { get; set; }
        public float MaxAfter { get; set; }
        public float MinBefore { get; set; }
        public float MaxBefore { get; set; }

        public StretchNode(MapManager map)
            : base(map)
        {
            SetInput(1);

            MinAfter = 0f;
            MaxAfter = 1f;
            MinBefore = 0.2f;
            MaxBefore = 0.8f;
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            if (!InputConnections[0])
            {
                OutputData = new float[resolution, resolution];
                return;
            }

            InputData[0] = InputConnections[0].From.OutputData;

            OutputData = Modifier.Stretch(InputData[0], MinAfter, MaxAfter, MinBefore, MaxBefore);
        }

        #region ISerializable
        public StretchNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MinBefore = (float)info.GetValue("MinBefore", typeof(float));
            MaxBefore = (float)info.GetValue("MaxBefore", typeof(float));
            MinAfter = (float)info.GetValue("MinAfter", typeof(float));
            MaxAfter = (float)info.GetValue("MaxAfter", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MinBefore", MinBefore);
            info.AddValue("MaxBefore", MaxBefore);
            info.AddValue("MinAfter", MinAfter);
            info.AddValue("MaxAfter", MaxAfter);

            base.GetObjectData(info,context);
        }
        #endregion
    }
}
