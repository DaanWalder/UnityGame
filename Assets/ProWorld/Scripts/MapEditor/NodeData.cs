using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProWorldSDK
{
    [Serializable]
    public abstract class NodeData : IDeserializationCallback, ISerializable
    {
        public delegate void OnCalculateDoneEventHandler();
        public event OnCalculateDoneEventHandler CalculateDone;

        public void OnCalculateDone()
        {
            if (CalculateDone != null) CalculateDone();
        }

        [NonSerialized] public double TimeTaken;
        [NonSerialized] public float[][,] InputData;
        [NonSerialized] public float[,] OutputData = new float[1, 1];
        [NonSerialized] public static float GlobalRange = 1f;

        public Link[] InputConnections { get; set; } // each input is limted to 1 source
        public List<Link> OutputConnections { get; set; } // 1 output can go to many inputs

        public const int Resolution = 257;

        private readonly MapManager _map;

        public static implicit operator bool(NodeData exists)
        {
            return exists != null;
        }

        protected NodeData(MapManager map)
        {
            _map = map;

            OutputConnections = new List<Link>();
        }

        public void Run()
        {
            _map.CalculateFrom(this, Resolution);
        }

        protected abstract void Calculate(int resolution, float offsetX, float offsetY);
        public void CalculateNode(int resolution, float offsetX, float offsetY)
        {
            var time = DateTime.Now;
            Calculate(resolution, offsetX, offsetY);

            TimeTaken = (DateTime.Now - time).TotalMilliseconds;

            OnCalculateDone();
        }

        protected void SetInput(int i)
        {
            InputConnections = new Link[i];
            InputData = new float[i][,];
        }

        public void OnDeserialization(object sender)
        {
            InputData = new float[InputConnections.Length][,];
        }

        #region ISerializable
        protected NodeData(SerializationInfo info, StreamingContext context)
        {
            InputConnections = (Link[])info.GetValue("InputConnections", typeof (Link[]));
            OutputConnections = (List<Link>) info.GetValue("OutputConnections", typeof (List<Link>));
            _map = (MapManager)info.GetValue("Map", typeof (MapManager));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("InputConnections", InputConnections);
            info.AddValue("OutputConnections", OutputConnections);
            info.AddValue("Map", _map);
        }
        #endregion
    }

    [Serializable]
    public class Link
    {
        public NodeData To;
        public int ToIndex;

        public NodeData From;

        public static implicit operator bool(Link exists)
        {
            return exists != null;
        }

        public Link()
        {
            
        }
        public Link(NodeData from, NodeData to, int toIndex)
        {
            To = to;
            ToIndex = toIndex;
            From = from;
        }
    }
}