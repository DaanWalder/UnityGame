using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class ImportNode : NodeData
    {
        public float[,] Array { get; set; }

    public ImportNode(MapManager map)
            : base(map)
        {
            SetInput(0);
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            OutputData = Util.ResizeArray(Array, resolution);
        }

        #region ISerializable
        public ImportNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Array = (float[,]) info.GetValue("Array", typeof (float[,]));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Array", Array);
        }
        #endregion
    }
}
