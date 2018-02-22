using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    // TODO MOVE
    public enum Direction
    {
        X,
        Y
    }

    [Serializable]
    public class AreaNode : NodeData
    {
        public enum BlurMode
        {
            Linear,
            Spherical
        }

        public BlurMode Blur = BlurMode.Linear;
        public Direction Direction = Direction.X;
        public float Cutoff = 0.5f;
        public float BlurArea = 0.1f;

        public AreaNode(MapManager map)
            : base(map)
        {
            SetInput(2);
        }

        protected override void Calculate(int resolution, float offsetX, float offsetY)
        {
            for (var i = 0; i < InputConnections.Length; i++)
            {
                if (!InputConnections[i])
                {
                    InputData[i] = new float[resolution, resolution];
                }
                else
                {
                    InputData[i] = InputConnections[i].From.OutputData;
                }
            }

            OutputData = new float[resolution,resolution];

            //var combinedRange = GlobalRange;

            var oX = offsetX * GlobalRange;
            var oY = offsetY * GlobalRange;

            var halfBlur = BlurArea/2f;


            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    float point;
                    switch (Direction)
                    {
                        case Direction.Y:
                            point = (y/(float) resolution)*GlobalRange + oY;
                            break;
                        default:
                            point = (x/(float) resolution)*GlobalRange + oX;
                            break;
                    }

                    var min = Cutoff - halfBlur;
                    var max = Cutoff + halfBlur;
                    var range = max - min;

                    if (point < min)
                        OutputData[y, x] = InputData[0][y, x];
                    else if (point > max)
                        OutputData[y, x] = InputData[1][y, x];
                    else // blur between
                    {
                        float m1;
                        float m2;

                        switch (Blur)
                        {
                            case BlurMode.Spherical:
                                //m1 = (float)(Math.Cos(point * Math.PI/range) + 1) / 2f ;
                                m1 = (float)(Math.Cos(point * Math.PI / range - min / range * Math.PI) + 1) / 2f;
                                //m2 = (float)(Math.Cos(point * Math.PI / range - Math.PI) + 1) / 2f;
                                m2 = (float)(Math.Cos(point * Math.PI / range - max / range * Math.PI) + 1) / 2f;
                                break;
                            default:
                                m1 = (max - point)/range; //1->0;
                                m2 = (point - min)/range; //0->1
                                break;
                        }

                        OutputData[y, x] = InputData[0][y, x]*m1 + InputData[1][y, x]*m2;
                    }
                }
            }

        }

        #region ISerializable
        public AreaNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Direction = (Direction) info.GetValue("Direction", typeof (Direction));
            Cutoff = (float) info.GetValue("Cutoff", typeof (float));
            Blur = (BlurMode) info.GetValue("Blur", typeof (BlurMode));
            BlurArea = (float) info.GetValue("BlurArea", typeof (float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Direction", Direction);
            info.AddValue("Cutoff", Cutoff);
            info.AddValue("Blur", Blur);
            info.AddValue("BlurArea", BlurArea);
        }

        #endregion
    }
}
