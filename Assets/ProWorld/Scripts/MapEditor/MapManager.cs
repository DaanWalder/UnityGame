using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class MapManager : IMap, ISerializable
    {
        public event EventHandler CalculateDone;

        private void OnCalculateDone(object sender, EventArgs e)
        {
            if (CalculateDone != null)
                CalculateDone(sender, e);
        }

        private BackgroundWorker _bw = new BackgroundWorker();

        public List<NodeData> Nodes { get; private set; }
        public OutputNode OutputNode { get; set; }

        public MapManager()
        {
            Nodes = new List<NodeData>();
            OutputNode = new OutputNode(this);
        }

        public bool IsCalculating()
        {
            return _bw.IsBusy;
        }

        private MapManager DeepClone()
        {
            lock (this)
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, this);
                    ms.Position = 0;

                    return (MapManager) formatter.Deserialize(ms);
                }
            }
        }

        public bool LinkNodes(NodeData from, NodeData to, int index, int resolution, int offsetX = 0, int offsetY = 0)
        {
            if (index >= to.InputConnections.Length)
                throw new UnityException("Input doesn't exist");

            if (from != to)
            {
                if (to is IReceiverNode)
                {
                    if (!(from is GeneratorNode))
                    {
                        return false;
                    }
                }

                var lastLink = new Link(from, to, index);

                from.OutputConnections.Add(lastLink);
                to.InputConnections[index] = lastLink;
                to.InputData[index] = from.OutputData;

                if (IsInfiniteLoop(from))
                {
                    // Remove the link
                    from.OutputConnections.Remove(lastLink);
                    to.InputConnections[index] = null;
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// This can be run multiple times simultaneously as it clones the nodes
        /// </summary>
        /// <param name="resolution"> </param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public float[,] GetArea(int resolution, float offsetX = 0, float offsetY = 0)
        {
            var map = DeepClone();
            var stack = map.BuildBackFromOutput();

            while (stack.Count > 0)
            {
                var nodes = stack.Pop();
                foreach (var node in nodes)
                {
                    node.CalculateNode(resolution, offsetX, offsetY);
                }
            }
            return map.OutputNode.OutputData;
        }

        public void CalculateAll(int resolution, float offsetX = 0, float offsetY = 0)
        {
            _bw = new BackgroundWorker();
            _bw.DoWork += delegate { CalculateAllWork(resolution, offsetX, offsetY); };
            _bw.RunWorkerCompleted += OnCalculateDone;
            _bw.RunWorkerAsync();
        }
        private void CalculateAllWork(int resolution, float offsetX = 0, float offsetY = 0)
        {
            var stack = BuildBackFromAll();

            while (stack.Count > 0)
            {
                var nodes = stack.Pop();
                foreach (var node in nodes)
                {
                    node.CalculateNode(resolution, offsetX, offsetY);
                }
            }
        }

        public void CalculateFrom(NodeData from, int resolution, int offsetX = 0, int offsetY = 0)
        {
            _bw = new BackgroundWorker();
            _bw.DoWork += delegate { CalculateFromWork(from, resolution, offsetX, offsetY); };
            _bw.RunWorkerCompleted += OnCalculateDone;
            _bw.RunWorkerAsync();
        }
        // Run this in alternate thread
        private void CalculateFromWork(NodeData from, int resolution, int offsetX = 0, int offsetY = 0)
        {
            var queue = BuildFrontFromNode(from);

            while (queue.Count > 0)
            {
                var nodes = queue.Dequeue();
                foreach (var node in nodes)
                {
                    node.CalculateNode(resolution, offsetX, offsetY);
                }
            }
        }

        private static bool IsInfiniteLoop(NodeData from)
        {
            var nodes = new List<NodeData> { from };

            var data = FindOutputConnections(nodes).ToList();

            while (data.Count != 0)
            {
                if (data.Contains(from))
                    return true;

                data = FindOutputConnections(data).ToList();
            }

            return false;
        }

        private Stack<List<NodeData>> BuildBackFromOutput()
        {
            var stack = new Stack<List<NodeData>>();

            var output = new List<NodeData> {OutputNode};
            stack.Push(output); // always level 0

            var data = FindConnections(output).ToList();

            while (data.Count != 0)
            {
                foreach (var d in data)
                {
                    foreach (var list in stack)
                    {
                        if (list.Contains(d)) // if a list already contains the member, remove it and put it in the current level
                        {
                            list.Remove(d);
                            break;
                        }
                    }
                }
                stack.Push(data);

                data = FindConnections(data).ToList();
            }

            return stack;
        }
        private Queue<List<NodeData>> BuildFrontFromNode(NodeData from)
        {
            var queue = new Queue<List<NodeData>>();

            var nodes = new List<NodeData> { from };
            queue.Enqueue(nodes);

            var data = FindOutputConnections(nodes).ToList();

            while (data.Count != 0)
            {
                foreach (var d in data)
                {
                    foreach (var list in queue)
                    {
                        if (list.Contains(d)) // if a list already contains the member, remove it and put it in the current level
                        {
                            list.Remove(d);
                            break;
                        }
                    }
                }
                queue.Enqueue(data);

                data = FindOutputConnections(data).ToList();
            }

            return queue;
        }
        private Stack<List<NodeData>> BuildBackFromAll()
        {
            var stack = new Stack<List<NodeData>>();
            var output = new List<NodeData> {OutputNode};

// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var node in Nodes)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                var connections = FindOutputConnections(new List<NodeData> { node }).ToArray();
                if (connections.Length == 0)
                {
                    output.Add(node);
                }
            }

            stack.Push(output); // always level 0

            var data = FindConnections(output).ToList();

            while (data.Count != 0)
            {
                foreach (var d in data)
                {
                    foreach (var list in stack)
                    {
                        if (list.Contains(d)) // if a list already contains the member, remove it and put it in the current level
                        {
                            list.Remove(d);
                            break;
                        }
                    }
                }
                stack.Push(data);

                data = FindConnections(data).ToList();
            }

            return stack;
        }

        // Returns all input connections on a tier
        private static IEnumerable<NodeData> FindConnections(IEnumerable<NodeData> nodes)
        {
            var newNodes = new HashSet<NodeData>();

            foreach (var node in nodes)
            {
                foreach (var input in node.InputConnections)
                {
                    // Check if an input is connected
                    if (input)
                        newNodes.Add(input.From);
                }
            }

            return newNodes;
        }

        private static IEnumerable<NodeData> FindOutputConnections(IEnumerable<NodeData> nodes)
        {
            var newNodes = new HashSet<NodeData>();

            foreach (var node in nodes)
            {
                foreach (var output in node.OutputConnections)
                {
                    // Check if an output is connected
                    if (output)
                        newNodes.Add(output.To);
                }
            }
            return newNodes;
        }

        #region ISerializable
        public MapManager(SerializationInfo info, StreamingContext context)
        {
            Nodes = (List<NodeData>)info.GetValue("Nodes", typeof (List<NodeData>));
            OutputNode = (OutputNode)info.GetValue("OutputNode", typeof(NodeData));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Nodes", Nodes);
            info.AddValue("OutputNode", OutputNode, typeof(NodeData));
        }
        #endregion
    }
}