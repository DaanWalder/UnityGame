using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class SimplexNode : GeneratorNode
    {
        public SimplexNode(MapManager map)
            : base(map)
        {
            Noise = new SimplexNoise();

            SetInput(0);
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            OutputData = new float[resolution, resolution];

            // We calc 1 extra position if stitching
            var nFactor = (resolution + 1) / (float)resolution;

            var combinedRange = Range * GlobalRange;

            var oX = (OffsetX + offsetX) * combinedRange;
            var oY = (OffsetY + offsetY) * combinedRange;

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    OutputData[y, x] = Noise.Noise((x / (float)resolution * combinedRange) * nFactor + oX, (y / (float)resolution * combinedRange) * nFactor + oY);
                }
            }
        }

        #region ISerializable
        public SimplexNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
        #endregion
    }
}
