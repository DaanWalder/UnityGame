using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class MulDivNode : NodeData
    {
        public enum Operation
        {
            Mul,
            Div,
        }

        public Operation Op { get; set; }
        public float Constant { get; set; }

        public MulDivNode(MapManager map)
            : base(map)
        {
            Op = Operation.Mul;
            Constant = 1f;

            SetInput(2);
        }

        // We override the setinput method so it's public
        public new void SetInput(int i)
        {
            InputConnections = new Link[i];
            InputData = new float[i][,];
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

            switch (Op)
            {
                case Operation.Mul:
                    OutputData = Combination.Mul(Constant, InputData);
                    break;
                case Operation.Div:
                    OutputData = Combination.Div(Constant, InputData);
                    break;
            }
        }

        #region ISerializable
        public MulDivNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Op = (Operation)info.GetValue("Op", typeof(Operation));
            Constant = (float)info.GetValue("Constant", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Op", Op);
            info.AddValue("Constant", Constant);
        }

        #endregion
    }
}
