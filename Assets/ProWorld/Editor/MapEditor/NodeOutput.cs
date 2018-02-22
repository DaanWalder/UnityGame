using UnityEngine;

namespace ProWorldEditor
{
    public class NodeOutput : AreaLayout
    {
        private readonly MapEditor _mapEditor;

        public NodeOutput(MapEditor mapEditor)
        {
            _mapEditor = mapEditor;
        }

        public override void OnGUI()
        {
            var width = _mapEditor.Window.position.width;
            var height = _mapEditor.Window.position.height;
            var node = _mapEditor.Data.Output;

            GUILayout.BeginArea(new Rect(width - MapEditor.OptionWidth - MapEditor.Preview, height - MapEditor.Height,
                         MapEditor.Preview, MapEditor.Height), "Output", GUI.skin.window);

            if (node)
            {
                var dimensions = MapEditor.Preview - 12;
                if (MapEditor.Height - 22 < dimensions)
                    dimensions = MapEditor.Height - 22;

                GUILayout.Box(node.OutputTexture, GUILayout.Width(dimensions), GUILayout.Height(dimensions));
            }
            GUILayout.EndArea();
        }
    }
}