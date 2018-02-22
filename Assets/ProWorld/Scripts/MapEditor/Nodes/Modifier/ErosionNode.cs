using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class ErosionNode : NodeData, IReceiverNode
    {
        public enum ErosionType
        {
            Thermal,
            Hydraulic,
            Improved
        }

        //public float Threshold = 0.002f;
        //public float Change = 0.05f;
        public int Iterations = 4;
        public ErosionType Type = ErosionType.Thermal;

        public Erosion Er = new Erosion();

        public ErosionNode(MapManager map)
            : base(map)
        {
            SetInput(1);
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
                OutputData = new float[resolution,resolution];
                return;
            }

            var noise = node.Noise;

            OutputData = new float[resolution,resolution];

            // We calc 1 extra position if stitching
            var nFactor = (resolution + 1)/(float) resolution;

            var combinedRange = node.Range*GlobalRange;

            const int radius = 5;

            var oX = (node.OffsetX + offsetX)*combinedRange;
            var oY = (node.OffsetY + offsetY)*combinedRange;

            var input = new float[radius*2 + resolution,radius*2 + resolution];

            for (var y = -radius; y < resolution + radius; y++)
            {
                for (var x = -radius; x < resolution + radius; x++)
                {
                    input[y + radius, x + radius] = noise.Noise((x/(float) resolution*combinedRange)*nFactor + oX,
                                                                (y/(float) resolution*combinedRange)*nFactor + oY);
                }
            }

            float[,] output;

            switch (Type)
            {
                    //case ErosionType.Thermal:
                default:
                    output = Er.ThermalErosion(input, Iterations);
                    break;
                case ErosionType.Hydraulic:
                    output = Er.HydraulicErosion(input, Iterations);
                    break;
                case ErosionType.Improved:
                    output = Er.ImprovedErosion(input, Iterations);
                    break;
            }

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    OutputData[y, x] = output[radius + y, radius + x];
                }
            }
        }

        #region ISerializable
        public ErosionNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Er = (Erosion)info.GetValue("Er", typeof(Erosion));
            Type = (ErosionType)info.GetValue("Type", typeof(ErosionType));
            Iterations = info.GetInt32("Iterations");

        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Er", Er);
            info.AddValue("Type", Type);
            info.AddValue("Iterations", Iterations);
        }

        #endregion
    }
}