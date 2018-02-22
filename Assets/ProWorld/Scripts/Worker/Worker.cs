using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
	public abstract class Worker : ISerializable
	{
	    public uint Priority = 20;

        public void Create(WorldData data)
        {
            WorkerPool.QueueWork(Generate, data);
        }
        public abstract void Generate(object sender, DoWorkEventArgs e);
        public abstract bool Apply(WorldData data, DateTime startTime, double duration);
        public virtual void Clean() {  }

        /*public Worker Clone()
        {
            var c = (Worker) MemberwiseClone();

            return DeepCopy(c);
        }
        // Here for potential future need
        protected virtual Worker DeepCopy(Worker worker)
        {
            return worker;
        }*/

        protected Worker()
        {
            
        }

        #region ISerializable
        protected Worker(SerializationInfo info, StreamingContext context)
        {
            Priority = info.GetUInt32("Priority");
        }
	    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
            info.AddValue("Priority", Priority);
	    }
        #endregion
	}
}
