using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class CellNoiseNode : GeneratorNode
    {
        public CellNoiseNode(MapManager map)
            : base(map)

        {
            Noise = new CellNoise();

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
                    OutputData[y, x] = Mathf.Clamp01(OutputData[y, x]);
                }
            }
        }

        #region ISerializable
        public CellNoiseNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
        #endregion
    }
}
