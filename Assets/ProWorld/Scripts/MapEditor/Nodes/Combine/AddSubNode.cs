using System;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public class AddSubNode : NodeData
    {
        public enum Operation
        {
            Add,
            Sub,
        }

        public Combination.Morph Morph { get; set; }
        public Operation Op { get; set; }
        public float Constant { get; set; }

        public AddSubNode(MapManager map)
            : base(map)
        {
            Morph = Combination.Morph.Shift;
            Op = Operation.Add;
            Constant = 0f;

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
                case Operation.Add:
                    OutputData = Combination.Add(Morph, Constant, InputData);
                    break;
                case Operation.Sub:
                    OutputData = Combination.Sub(Morph, Constant, InputData);
                    break;
            }
        }

        #region ISerializable
        public AddSubNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Op = (Operation) info.GetValue("Op", typeof (Operation));
            Morph = (Combination.Morph)info.GetValue("Morph", typeof(Combination.Morph));
            Constant = (float)info.GetValue("Constant", typeof (float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Op", Op);
            info.AddValue("Morph", Morph);
            info.AddValue("Constant", Constant);
        }

        #endregion
    }
}
