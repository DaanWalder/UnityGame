using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class CutoffNode : NodeData
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public Modifier.CutoffHandle MinHandle { get; set; }
        public Modifier.CutoffHandle MaxHandle { get; set; }

        public CutoffNode(MapManager map)
            : base(map)
        {
            SetInput(1);

            Min = 0f;
            Max = 1f;
            MinHandle = Modifier.CutoffHandle.Cutoff;
            MaxHandle = Modifier.CutoffHandle.Cutoff;
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            if (!InputConnections[0])
            {
                OutputData = new float[resolution, resolution];
                return;
            }

            InputData[0] = InputConnections[0].From.OutputData;

            OutputData = Modifier.Cutoff(InputData[0], Min, Max, MinHandle, MaxHandle);
        }

        #region ISerializable
        public CutoffNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MinHandle = (Modifier.CutoffHandle)info.GetValue("MinHandle", typeof(Modifier.CutoffHandle));
            MaxHandle = (Modifier.CutoffHandle)info.GetValue("MaxHandle", typeof(Modifier.CutoffHandle));
            Min = (float)info.GetValue("Min", typeof(float));
            Max = (float)info.GetValue("Max", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MinHandle", MinHandle);
            info.AddValue("MaxHandle", MaxHandle);
            info.AddValue("Min", Min);
            info.AddValue("Max", Max);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
