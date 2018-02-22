using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class FractalBrownianMotionNode : GeneratorNode, IReceiverNode
    {
        public FractalBrownianMotion FBm = new FractalBrownianMotion();

        public FractalBrownianMotionNode(MapManager map)
            : base(map)
        {
            SetInput(1);
            Noise = FBm;
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            if (!InputConnections[0])
            {
                OutputData = new float[resolution,resolution];
                return;
            }
          
            var node = (GeneratorNode) (InputConnections[0].From);

            if (node.Noise == null)
            {
                OutputData = new float[resolution, resolution];
                return;
            }
            SetNoise(node.Noise);

            OutputData = new float[resolution, resolution];

            // We calc 1 extra position if stitching
            var nFactor = (resolution + 1) / (float)resolution;

            var combinedRange = node.Range * GlobalRange;

            var oX = (node.OffsetX + offsetX) * combinedRange;
            var oY = (node.OffsetY + offsetY) * combinedRange;

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    OutputData[y, x] = Noise.Noise((x / (float)resolution * combinedRange) * nFactor + oX, (y / (float)resolution * combinedRange) * nFactor + oY);
                }
            }
        }

        #region ISerializable
        public FractalBrownianMotionNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FBm = (FractalBrownianMotion) info.GetValue("FBm", typeof (FractalBrownianMotion));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("FBm", FBm);
        }

        #endregion

        public void SetNoise(INoise noise)
        {
            FBm.NoiseFunction = noise;
        }
    }
}
