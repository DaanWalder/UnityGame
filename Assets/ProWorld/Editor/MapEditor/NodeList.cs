using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProWorldEditor
{
    public class NodeList : AreaLayout
    {
        private class NodeListData
        {
            public string Title;
            public Type NodeType;
        }

        private readonly MapEditor _mapEditor;
        private readonly List<NodeListData> _generators = new List<NodeListData>();
        private readonly List<NodeListData> _genMod = new List<NodeListData>();
        private readonly List<NodeListData> _modifiers = new List<NodeListData>();
        private readonly List<NodeListData> _combine = new List<NodeListData>();

        public NodeList(MapEditor mapEditor)
        {
            _mapEditor = mapEditor;

            // Build a list of classes that use the base class Node
            var nodes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Node).IsAssignableFrom(t)).ToList();
            nodes.Remove(typeof(Node)); // Remove the base class from this list

            // Loop through all these nodes
            foreach (var node in nodes)
            {
                // Get the various pieces of data we need from these temp classes
                var nld = new NodeListData();
                var pf = node.GetField("Title", BindingFlags.Public | BindingFlags.Static);
                if (pf != null) nld.Title = (string)pf.GetValue(null);
                nld.NodeType = node;

                pf = node.GetField("Type", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (pf != null)
                {
                    var type = (Node.NodeType)pf.GetValue(null);

                    switch (type)
                    {
                        case Node.NodeType.Generator:
                            _generators.Add(nld);
                            break;
                        case Node.NodeType.GenMod:
                            _genMod.Add(nld);
                            break;
                        case Node.NodeType.Modifier:
                            _modifiers.Add(nld);
                            break;
                        case Node.NodeType.Combine:
                            _combine.Add(nld);
                            break;
                    }
                }
            }
        }

        public override void OnGUI()
        {
            var width = _mapEditor.Window.position.width;
            var height = _mapEditor.Window.position.height;

            GUILayout.BeginArea(new Rect(width - MapEditor.OptionWidth, 0, MapEditor.OptionWidth, height), "Nodes", GUI.skin.window);

            Group("Generate", _generators);
            Group("GenModifier", _genMod);
            Group("Modify", _modifiers);
            Group("Combine", _combine);

            GUILayout.EndArea();
        }

        private void Group(string title, IEnumerable<NodeListData> nodes)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            foreach (var node in nodes)
            {
                if (GUILayout.Button(node.Title))
                {
                    var instance = (Node)Activator.CreateInstance(node.NodeType, _mapEditor);
                    _mapEditor.AddNode(instance);
                }
            }
        }
    }
}