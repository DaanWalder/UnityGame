using System;
using System.Collections.Generic;
using System.Linq;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    [Serializable]
    public class MapEditorData : IMapEditorData
    {
        public readonly MapManager Map;
        public readonly List<Node> Nodes = new List<Node>();
        public OutputNodeGUI Output;

        public MapEditorData(MapManager map)
        {
            Map = map;
            Output = new OutputNodeGUI(null, Map.OutputNode)
                         {
                             WindowRect = new Rect(980, 140, 100, 93)
                         };
        }
    }

    public class MapEditor : WindowLayout
    {
        public delegate void HeightHandler(float[,] height);

        public const int OptionWidth = 150;
        public const int Height = 300;
        public const int Preview = Height - 10;

        public EditorWindow Window;

        private readonly LinkGUI _newLink = new LinkGUI();
        private Node _to;
        private int _toIndex;
        public Node CurrentNode;
        public bool IsCalculating;

        private readonly List<AreaLayout> _windows = new List<AreaLayout>();

        public MapEditorData Data;

        private bool _isCalculateDone;
        private readonly HeightHandler _calculate;

        public MapEditor(MapEditorData data, EditorWindow editorWindow, HeightHandler calculateDone)
        {
            Title = "Node Editor";

            Window = editorWindow;

            Data = data;

            foreach(var n in data.Nodes)
                n.Set(this);
            data.Output.Set(this);

            _windows.Add(new NodeList(this));
            _windows.Add(new NodeOptions(this));
            _windows.Add(new NodePreview(this));
            _windows.Add(new NodeOutput(this));

            NodeData.GlobalRange = 1;

            Data.Map.CalculateAll(NodeData.Resolution);

            _calculate = calculateDone;

            Data.Map.CalculateDone += CalculateDone;
        }

        public MapEditor(MapEditorData data, EditorWindow editorWindow)
            : this(data, editorWindow, null) { }

        public override void OnGUI()
        {
            if (_isCalculateDone && _calculate != null)
            {
                _calculate(Data.Output.Data.OutputData);
                _isCalculateDone = false;
            }

            IsCalculating = Data.Map.IsCalculating();

            var width = Window.position.width;
            var height = Window.position.height;

            // Draw windows
            foreach (var window in _windows)
            {
                window.OnGUI();
            }

            GUILayout.BeginArea(new Rect(0, 0, width - OptionWidth, height - Height));
            // CURVES
            foreach (var node in Data.Nodes)
            {
                foreach (var connection in node.Links)
                {
                    if (connection)
                        CurveFromTo(connection, Color.red);
                }
            }
            // Output link
            if (Data.Output.Links[0])
                CurveFromTo(Data.Output.Links[0], Color.red);

            // Draw nodes
            Window.BeginWindows();
            foreach (var node in Data.Nodes)
            {
                node.OnGUI();
            }
            Data.Output.OnGUI();

            Window.EndWindows();
            GUILayout.EndArea();

            AttachingNode();
            Events();
        }

        public void AddNode(Node node)
        {
            Data.Nodes.Add(node);
            Data.Map.Nodes.Add(node.Data);
        }
        public void CloseWindow(Node node)
        {
            foreach (var input in node.Data.InputConnections.Where(input => input))
            {
                input.From.OutputConnections.Remove(input);
            }

            // Remove output links
            foreach (var output in node.Data.OutputConnections)
            {
                output.To.InputConnections[output.ToIndex] = null;
            }

            // remove GUI links
            var nodes = new List<Node>(Data.Nodes) { Data.Output };

            foreach (var nodeList in nodes)
            {
                for (int index = 0; index < nodeList.Links.Length; index++)
                {
                    var link = nodeList.Links[index];
                    if (link && link.From == node)
                    {
                        nodeList.Links[index] = null;
                    }
                }
            }

            if (CurrentNode == node)
                CurrentNode = null;

            Data.Nodes.Remove(node);
            Data.Map.Nodes.Remove(node.Data);
        }

        public void OutputLink(Node node)
        {
            _newLink.From = node;

            if (!_newLink.To) return;

            Link();
        }

        public void InputLink(Node node, int index)
        {
            if (node.Links[index])
            {
                var ol = node.Data.InputConnections[index];
                ol.From.OutputConnections.Remove(ol);
                node.Data.InputConnections[index] = null;
                node.Links[index] = null;
                node.Run();
            }

            _newLink.To = node;
            _newLink.ToIndex = index;

            if (!_newLink.From) return;

            Link();
        }

        private void Link()
        {
            var linked = Data.Map.LinkNodes(_newLink.From.Data, _newLink.To.Data, _newLink.ToIndex, Node.Resolution);

            if (linked)
            {
                _newLink.To.Data.Run();
                _newLink.To.Links[_newLink.ToIndex] = new LinkGUI(_newLink);
            }

            _newLink.From = null;
            _newLink.To = null;
        }

        private void AttachingNode()
        {
            if (_newLink.From)
                CurveFromTo(_newLink.From.WindowRect, Event.current.mousePosition, Color.red);
            else if (_newLink.To)
                CurveFromTo(_newLink.To.WindowRect, _newLink.ToIndex, Event.current.mousePosition, Color.red);
        }

        private void Events()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                _newLink.From = null;
                _newLink.To = null;
            }
        }

        private static void CurveFromTo(LinkGUI link, Color color)
        {
            var index = link.ToIndex;
            var from = new Vector2(link.From.WindowRect.x + link.From.WindowRect.width,
                               link.From.WindowRect.y + link.From.WindowRect.height - 10);
            var to = new Vector2(link.To.WindowRect.x, link.To.WindowRect.y + 25 + index * 20);

            Drawing.BezierLine(
                from,
                from + new Vector2(Mathf.Abs(to.x - from.x) / 2f, 0),
                to,
                to - new Vector2(Mathf.Abs(to.x - from.x) / 2f, 0),
                color, 2, true, 20);
        }
        private static void CurveFromTo(Rect wr, Vector2 point, Color color)
        {
            Drawing.BezierLine(
                new Vector2(wr.x + wr.width, wr.y + wr.height - 10),
                new Vector2(wr.x + wr.width + Mathf.Abs(point.x - (wr.x + wr.width)) / 2, wr.y + wr.height - 10),
                new Vector2(point.x, point.y),
                new Vector2(point.x - Mathf.Abs(point.x - (wr.x + wr.width)) / 2, point.y),
                color, 2, true, 20);
        }

        private static void CurveFromTo(Rect wr2, int index, Vector2 point, Color color)
        {
            wr2.y += 25 + index * 20;

            Drawing.BezierLine(
                new Vector2(point.x, point.y),
                new Vector2(point.x + Mathf.Abs(wr2.x - point.x) / 2, point.y),
                new Vector2(wr2.x, wr2.y),
                new Vector2(wr2.x - Mathf.Abs(wr2.x - point.x) / 2, wr2.y),
                color, 2, true, 20);
        }
       
        private void CalculateDone(object sender, EventArgs e)
        {
            _isCalculateDone = ProWorld.Data.IsRealTime;
        }

        public override void Clean()
        {
            Data.Map.CalculateDone -= CalculateDone;
        }
    }
}