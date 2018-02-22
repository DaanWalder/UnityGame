using ProWorldSDK;
using UnityEngine;

namespace ProWorldEditor
{
    public sealed class EntityWindow : LayerPreview
    {
        private const int DensityMapSize = 256;
        private readonly EntityGenerator _eg;
        private Texture2D _densityMap;

        public EntityWindow()
        {
            Title = "Entity";

            Refresh();

            _eg = ProWorld.Data.World.EntityData.Generator;
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var position = ProWorld.Window.position;

            GUILayout.BeginArea(new Rect(523, LayerPreviewSize + 24, DensityMapSize + 11, DensityMapSize + 24), "Density Map", GUI.skin.window);
            if (GUILayout.Button(_densityMap, GUIStyle.none, GUILayout.Width(DensityMapSize), GUILayout.Height(DensityMapSize)))
            {
                ProWorld.NewWindow(new MapEditor(ProWorld.Data.EntityDensityMap, ProWorld.Window));
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 200, 0, 200, position.height - 20), "Options", GUI.skin.window);
            
            GUILayout.Label("Default Quality");
            GUILayout.BeginHorizontal();
            _eg.PlacementQuality = (EntityGenerator.TreePlacementQuality)MyGUI.EnumButton(_eg.PlacementQuality);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        protected override void SceneChange()
        {
            GenerateTextures();
            GenerateTerrain();
            GenerateEntities();
        }

        protected override void ApplyTexture()
        {
            IsReapplyTexture = false;

            ApplyAreaTexture();
            ApplyDensityMap();
            ApplyPreviewMap();
        }

        private void ApplyDensityMap()
        {
            _densityMap = new Texture2D(DensityMapSize, DensityMapSize);
            var colors = new Color[DensityMapSize*DensityMapSize];

            var resized = Util.ResizeArray(ProWorld.Data.World.EntityData.Generator.Densisty, DensityMapSize);

            for (var y = 0; y < DensityMapSize; y++)
            {
                for (var x = 0; x < DensityMapSize; x++)
                {
                    colors[y * DensityMapSize + x] = new Color(0, resized[y, x], 0.25f);
                }
            }

            _densityMap.SetPixels(colors);
            _densityMap.Apply();
        }
        private void ApplyPreviewMap()
        {
            var size = ProWorld.Data.World.TerrainWidth;
            var world = ProWorld.Data.World.EntityData.Generator.Densisty;
            var resolution = world.GetLength(0);

            var color = new Color[resolution * resolution];

            var nfactor = resolution / (float)size;

            foreach (var layer in ProWorld.Data.Entity)
            {
                //layer.Layer.Entities // TODO START USING

                foreach (var group in layer.Layer.Groups)
                {
                    foreach (var box in group.Boxes)
                    {
                        foreach (var tree in box.Objects)
                        {
                            var x = (int) (tree.Position.x*nfactor);
                            var y = (int) (tree.Position.z*nfactor);

                            color[y*resolution + x] = Color.green;
                        }
                    }
                }
            }

            PreviewTexture.SetPixels(color);
            PreviewTexture.Apply();
        }
        protected override void LayerSelected(int layer)
        {
            ProWorld.NewWindow(new EntityLayerWindow(layer));
        }
    }
}