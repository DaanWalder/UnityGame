using ProWorldSDK;
using UnityEditor;
using UnityEngine;

namespace ProWorldEditor
{
    public sealed class ImportAssets : WindowLayout
    {
        private Vector2 _textureScroll;
        private Vector2 _treeScroll;

        private float _width;
        private float _height;

        public ImportAssets()
        {
            Title = "Assets";
        }

        public override void OnGUI()
        {
            _width = ProWorld.Window.position.width / 2f;
            _height = ProWorld.Window.position.height - 20;

            GUILayout.BeginArea(new Rect(0, 0, _width, _height), "Textures", GUI.skin.window);
            TextureBar();
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(_width, 0, _width, _height), "Entities", GUI.skin.window);
            EntityBar();
            GUILayout.EndArea();
        }

        private void TextureBar()
        {
            var style = new GUIStyle(GUIStyle.none) { stretchHeight = true, stretchWidth = true, margin = new RectOffset(2, 2, 2, 2) };
            //var textures = ProWorld.Data.World.Textures;
            var data = ProWorld.Data;
            var textures = ProWorld.Data.Textures;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(150)))
            {
                var tes = data.AddTexture(new TextureSplat());
                TextureSplatProperties.CreateTDP(tes);
            }
            /*if (GUILayout.Button("Remove Last", EditorStyles.miniButtonRight, GUILayout.Width(150)))
            {
                var count = textures.Count;
                if (count != 0)
                    textures.RemoveAt(count - 1);
            }*/
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _textureScroll = GUILayout.BeginScrollView(_textureScroll); // Begin Scroll

            var x = 13;
            GUILayout.BeginHorizontal(); // Begin Hor 2
            foreach (var t in textures)
            {
                var text = t.Splat.Texture ? t.Splat.Texture : Util.White;

                x += 66;
                if (x > _width)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    x = 66;
                }

                if (GUILayout.Button(text, style, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    TextureSplatProperties.CreateTDP(t);
                }
            }
            GUILayout.EndHorizontal(); // End Hor 2
            GUILayout.EndScrollView(); // End Scroll
        }

        private void EntityBar()
        {
            var style = new GUIStyle(GUIStyle.none) { stretchHeight = true, stretchWidth = true, margin = new RectOffset(2, 2, 2, 2) };
            var data = ProWorld.Data;
            var entities = ProWorld.Data.Entities;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(150)))
            {
                var eed = data.AddEntity(new EntityData());
                EntityDataProperties.CreateTDP(eed);
            }
            /*if (GUILayout.Button("Remove Last", EditorStyles.miniButtonRight, GUILayout.Width(150)))
            {
                var count = entities.Count;
                if (count != 0)
                    entities.RemoveAt(count - 1);

            }*/
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _treeScroll = GUILayout.BeginScrollView(_treeScroll); // Begin Scroll
            var x = 13;
            GUILayout.BeginHorizontal(); // Begin Hor 2

            foreach (var t in entities)
            {
                /*
#if UNITY_4_0
                var text = t.Entity.Prefab ? AssetPreview.GetAssetPreview(t.Entity.Prefab) : Util.White;
#else
                var text = t.Entity.Prefab ? EditorUtility.GetAssetPreview(t.Entity.Prefab) : Util.White;
#endif
                 */

                var text = t.PreviewTexture ? t.PreviewTexture : Util.White;

                x += 66;
                if (x > _width)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    x = 66;
                }

                if (GUILayout.Button(text, style, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    EntityDataProperties.CreateTDP(t);
                }
            }
            GUILayout.EndHorizontal(); // End Hor 2
            GUILayout.EndScrollView(); // End Scroll
        }
    }
}