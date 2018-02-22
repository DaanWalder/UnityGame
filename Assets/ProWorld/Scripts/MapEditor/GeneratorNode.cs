using System.Runtime.Serialization;

namespace ProWorldSDK
{
    public abstract class GeneratorNode : NodeData
    {
        public float Range = 1;
        public float OffsetX = 0f;
        public float OffsetY = 0f;

        public INoise Noise;

        protected GeneratorNode(MapManager map)
            : base(map)
        {
            
        }

        protected GeneratorNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Range = (float)info.GetValue("Range", typeof(float));
            OffsetX = (float)info.GetValue("OffsetX", typeof(float));
            OffsetY = (float)info.GetValue("OffsetY", typeof(float));

            Noise = (INoise)info.GetValue("Noise", typeof(INoise));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Range", Range);
            info.AddValue("OffsetX", OffsetX);
            info.AddValue("OffsetY", OffsetY);

            info.AddValue("Noise", Noise);
        }
    }
}
