using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class SmoothNode : NodeData, IReceiverNode
    {
        public float Radius = 0.1f;

        public SmoothNode(MapManager map)
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

            var node = (GeneratorNode)(InputConnections[0].From);

            if (node.Noise == null)
            {
                OutputData = new float[resolution, resolution];
                return;
            }
            //SetNoise(node.Noise);
            var noise = node.Noise;

            OutputData = new float[resolution, resolution];

            // We calc 1 extra position if stitching
            var nFactor = (resolution + 1) / (float)resolution;

            var combinedRange = node.Range * GlobalRange;

            var radius = (int)(resolution * Radius);
            //var ires = radius * 2 + resolution;

            var oX = (node.OffsetX + offsetX) * combinedRange;
            var oY = (node.OffsetY + offsetY) * combinedRange;

            var input = new float[radius * 2 + resolution, radius * 2 + resolution];

            for (var y = -radius; y < resolution + radius; y++)
            {
                for (var x = -radius; x < resolution + radius; x++)
                {
                    input[y + radius, x + radius] = noise.Noise((x / (float)resolution * combinedRange) * nFactor + oX,
                                                                (y / (float)resolution * combinedRange) * nFactor + oY);
                }
            }

            var output = Smooth.SmoothGrayscale(input, radius);

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    OutputData[y, x] = output[radius + y, radius + x];
                }
            }


            /*InputData[0] = InputConnections[0].From.OutputData;

            var radius = (int)(resolution*Radius);

            OutputData = Smooth.SmoothGrayscale(InputData[0], radius);*/
        }

        #region ISerializable
        public SmoothNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Radius = (float)info.GetValue("Radius", typeof(float));

        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Radius", Radius);
        }

        #endregion
    }
}