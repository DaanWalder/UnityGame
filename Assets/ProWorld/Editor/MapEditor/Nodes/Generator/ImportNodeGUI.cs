using System;
using System.IO;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public class ImportNodeGUI : Node
    {
        public new static string Title = "Import";
        public static NodeType Type = NodeType.Generator;

        private string _path;
        [NonSerialized] private Texture2D _texture;
        [NonSerialized] private Terrain _terrain;
        private bool _isFromImage = true;

        public ImportNodeGUI(MapEditor mapEditor)
            : base(mapEditor, new ImportNode(mapEditor.Data.Map), Title)
        {
        }

        public override void Options()
        {
            var import = (ImportNode)Data;

            GUILayout.BeginHorizontal();

            GUI.color = _isFromImage ? Color.red : Color.white;
            if (GUILayout.Button("Image", EditorStyles.miniButtonLeft, GUILayout.Width(110)))
            {
                _isFromImage = true;
            }

            GUI.color = !_isFromImage ? Color.red : Color.white;
            if (GUILayout.Button("Terrain", EditorStyles.miniButtonRight, GUILayout.Width(110)))
            {
                _isFromImage = false;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            if (_isFromImage)
            {

                var texture =
                    (Texture2D)
                    EditorGUILayout.ObjectField("Heightmap: ", _texture, typeof (Texture2D), false, GUILayout.Width(64),
                                                GUILayout.Height(64));
                GUILayout.Label("Warning: IsReadable will be set to true");

                if (texture != _texture)
                {
                    _texture = texture;
                    _path = AssetDatabase.GetAssetPath(_texture);

                    CheckTexture(_texture);
                    import.Array = Util.ConvertToGrayscale(_texture);

                    Run();
                }
            }

            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Terrain:", GUILayout.Width(100));
                if (GUILayout.Button("Selection", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    ApplyWindow.GetSelection(ref _terrain);
                }
                GUILayout.EndHorizontal();
                _terrain = (Terrain) EditorGUILayout.ObjectField(_terrain, typeof (Terrain), true, GUILayout.Width(200));

                GUILayout.Space(10);

                if (GUILayout.Button("Import", EditorStyles.miniButton, GUILayout.Width(120)))
                {
                    if (_terrain != null)
                    {
                        var td = _terrain.terrainData;
                        import.Array = td.GetHeights(0, 0, td.heightmapResolution, td.heightmapResolution);

                        _path = null;
                        _texture = null;

                        Run();
                    }
                }
            }

            base.Options();
        }

        public static void CheckTexture(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);

            if (File.Exists(path))
            {
                var textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
                if (!textureImporter.isReadable)
                {
                    AssetDatabase.StartAssetEditing();
                    textureImporter.textureType = TextureImporterType.Default;
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(path);
                    AssetDatabase.Refresh();
                    AssetDatabase.StopAssetEditing();
                }
            }
        }

        #region ISerialize
        protected ImportNodeGUI(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _path = info.GetString("Path");

            _texture = (Texture2D)AssetDatabase.LoadAssetAtPath(_path, typeof(Texture2D));
            CheckTexture(_texture);

            _isFromImage = info.GetBoolean("IsFromImage");

            //var import = (ImportNode)Data;
            //import.Array = Util.ConvertToGrayscale(_texture);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Path", _path);
            info.AddValue("IsFromImage", _isFromImage);
        }

        #endregion
    }
}