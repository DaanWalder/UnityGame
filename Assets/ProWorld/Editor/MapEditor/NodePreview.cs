using UnityEngine;

namespace ProWorldEditor
{
    public class NodePreview : AreaLayout
    {
        private readonly MapEditor _mapEditor;

        public NodePreview(MapEditor mapEditor)
        {
            _mapEditor = mapEditor;
        }

        public override void OnGUI()
        {
            var node = _mapEditor.CurrentNode;
            var width = _mapEditor.Window.position.width;
            var height = _mapEditor.Window.position.height;

            GUILayout.BeginArea(new Rect(width - MapEditor.OptionWidth - MapEditor.Preview - MapEditor.Preview,
                height - MapEditor.Height, MapEditor.Preview, MapEditor.Height), "Preview", GUI.skin.window);

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