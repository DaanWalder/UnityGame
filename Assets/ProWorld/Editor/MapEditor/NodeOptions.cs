using UnityEngine;

namespace ProWorldEditor
{
    public class NodeOptions : AreaLayout
    {
        private readonly MapEditor _mapEditor;

        public NodeOptions(MapEditor mapEditor)
        {
            _mapEditor = mapEditor;
        }

        public override void OnGUI()
        {
            var width = _mapEditor.Window.position.width;
            var height = _mapEditor.Window.position.height;

            GUILayout.BeginArea(new Rect(0, height - MapEditor.Height, width - MapEditor.OptionWidth - MapEditor.Preview - MapEditor.Preview, MapEditor.Height),
                "Settings", GUI.skin.window);

            if (_mapEditor.CurrentNode)
            {
                GUI.enabled = !_mapEditor.IsCalculating;

                _mapEditor.CurrentNode.UpdateOptionsChanged();
                _mapEditor.CurrentNode.Options();               
                _mapEditor.CurrentNode.CheckOptionChanged();

                GUI.enabled = true;
            }
            GUILayout.EndArea();
        }
    }
}