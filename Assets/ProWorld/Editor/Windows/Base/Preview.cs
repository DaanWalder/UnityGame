using System;
using System.ComponentModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProWorldEditor
{
    public abstract class Preview : WindowLayout
    {
        public int PreviewSize = 512;

        protected Texture2D PreviewTexture;

        protected static float Zoom = 1f;
        protected static Vector2 Offset = Vector2.zero;
        //protected static Vector2 Offset = Vector2.zero;
        private Rect _textureRect = new Rect(0, 0, 1.0f, 1.0f);

        protected int TextureSize = 512;
        protected bool IsRebuild;
        protected bool IsReapplyTexture;

        protected readonly BackgroundWorker Worker = new BackgroundWorker();

        private readonly GUIStyle _previewText;
        
        // previous
        protected bool IsPreviewChanged;

        protected Preview()
        {
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += Calculate;
            PreviewTexture = new Texture2D(TextureSize, TextureSize);

            _previewText = new GUIStyle(GUIStyle.none)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.red }
            };
        }

        protected void Calculate(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (IsPreviewChanged)
            {
                IsPreviewChanged = false;

                // Update offset
                var data = ProWorld.Data.WorldData;
                Offset.x = (data.Position.x - (Zoom - 1) / 2) / Zoom; // offset modified by scale, zoom based off middle (hence - (1/2))
                Offset.y = (data.Position.y - (Zoom - 1) / 2) / Zoom;

                SceneChange();
            }

            DoCalculate();
            IsRebuild = true;
        }
        protected void DoWork()
        {
            if (!Worker.IsBusy)
                Worker.RunWorkerAsync();
        }

        protected abstract void SceneChange();

        protected void GenerateTerrain()
        {
            ProWorld.Data.Terrain.Generate(Offset, Zoom);
        }
        protected void GenerateTextures()
        {
            var map = ProWorld.Data.WorldData.LayerMasks;
            ProWorld.Data.World.TextureData.Generator.GenerateAllLayerTextures(map, EditorData.TextureSize, Offset.x, Offset.y);
        }
        protected void GenerateEntities()
        {
            ProWorld.Data.World.EntityData.Generator.CreateDensityMap(EditorData.TerrainSize, Offset.x, Offset.y);
            ProWorld.Data.World.EntityData.Generator.GenerateAllEntities(ProWorld.Data.WorldData);
        }
        protected virtual void DoCalculate() {}

        public override void OnGUI()
        {
            if (IsRebuild && !Worker.IsBusy) Rebuild();
            if (IsReapplyTexture && !Worker.IsBusy) ApplyTexture();

            var data = ProWorld.Data.WorldData;

            #region Map
            GUILayout.BeginArea(new Rect(0, 0, PreviewSize + 11, PreviewSize + 24), "Preview", GUI.skin.window);
            var gc = new GUIContent(PreviewTexture);
            var rect = GUILayoutUtility.GetRect(gc, GUIStyle.none, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));

            HandleMapInput(rect);

            if (Event.current.type == EventType.Repaint)
            {
                Graphics.DrawTexture(rect, PreviewTexture, _textureRect, 0, 0, 0, 0);

                var text = new Rect(rect);
                text.x += text.width - 70;
                text.width = 60;
                text.y += 10;
                text.height = 20;

                GUI.Label(text, (1 / Zoom).ToString("0.00") + "x", _previewText);

                text.x -= 30;
                text.width += 30;
                text.y += 20;
                GUI.Label(text, "(" + data.Position.x + "," + data.Position.y + ")", _previewText);
            }
            GUILayout.EndArea();
            #endregion
        }

        private void RunWorker()
        {
            if (Worker.IsBusy)
                return;

            Worker.RunWorkerAsync();
        }

        private void HandleMapInput(Rect rect)
        {
            var data = ProWorld.Data.WorldData;

            var mouse = Event.current.mousePosition;

            var x = Convert.ToInt32(mouse.x);
            var y = Convert.ToInt32(mouse.y);

            if (!Worker.IsBusy)
            {
                // Scroll event
                if (Event.current.type == EventType.ScrollWheel && rect.Contains(mouse))
                {
                    var zoomBefore = Zoom;
                    Zoom *= Event.current.delta.y > 0 ? 2 : 0.5f;
                    Zoom = Mathf.Clamp(Zoom, 0.0625f, 16f);

                    if (Math.Abs(Zoom - zoomBefore) > Mathf.Epsilon)
                    {
                        if (Zoom >= 1 || ProWorld.Data.World.TerrainData.Resolution > (int) (PreviewSize/Zoom))
                        {
                            IsPreviewChanged = true;
                            RunWorker();
                        }
                    }
                }

                // Mouse click event
                if (Event.current.type == EventType.MouseDown)
                {
                    var offsetBefore = data.Position;

                    // Left
                    var clickRect = new Rect(rect);
                    clickRect.width /= 8;
                    if (clickRect.Contains(mouse))
                    {
                        // Right click moves 5, other clicks 0.5f
                        data.Position.x -= Event.current.button == 0 ? Zoom : Mathf.Max(Zoom / 2f, 0.5f);
                    }

                    // Right
                    clickRect = new Rect(rect);
                    clickRect.x += clickRect.width / 8f * 7;
                    clickRect.width /= 8;
                    if (clickRect.Contains(mouse))
                    {
                        data.Position.x += Event.current.button == 0 ? Zoom : Mathf.Max(Zoom / 2f, 0.5f);
                    }

                    // Top
                    clickRect = new Rect(rect);
                    clickRect.height /= 8;
                    if (clickRect.Contains(mouse))
                    {
                        data.Position.y += Event.current.button == 0 ? Zoom : Mathf.Max(Zoom / 2f, 0.5f);
                    }

                    // Bottom
                    clickRect = new Rect(rect);
                    clickRect.y += clickRect.height / 8f * 7;
                    clickRect.height /= 8;
                    if (clickRect.Contains(mouse))
                    {
                        data.Position.y -= Event.current.button == 0 ? Zoom : Mathf.Max(Zoom / 2f, 0.5f);
                    }


                    clickRect = new Rect(rect);
                    clickRect.x += clickRect.height / 8f * 3;
                    clickRect.y += clickRect.height / 8f * 3;
                    clickRect.height /= 4;
                    clickRect.width /= 4;
                    if (clickRect.Contains(mouse) && Event.current.button == 1)
                    {
                        data.Position = Vector2.zero;
                        Zoom = 1f;
                        IsPreviewChanged = true;
                        RunWorker();
                    }

                    if (offsetBefore != data.Position)
                    {
                        IsPreviewChanged = true;
                        RunWorker();
                    }
                }
            }

            // Update texture, based off scroll event above and mouse position
            if (rect.Contains(mouse))
            {
                var xx = x - (int)rect.x;
                var yy = PreviewSize - (y - (int)rect.y);

                var zoom = Mathf.Min(Zoom, 1);

                _textureRect.x = xx / (float)PreviewSize * (1 - zoom);
                _textureRect.y = yy / (float)PreviewSize * (1 - zoom);
                _textureRect.width = zoom;
                _textureRect.height = zoom;
            }
        }

        protected virtual void Rebuild()
        {
            IsRebuild = false;

            Object.DestroyImmediate(PreviewTexture);
            PreviewTexture = new Texture2D(TextureSize, TextureSize);

            ApplyTexture();
        }

        protected abstract void ApplyTexture();
        public override void Clean()
        {
            Object.DestroyImmediate(PreviewTexture);
            Worker.CancelAsync();
        }
        public override void Refresh()
        {
            IsRebuild = false;
            IsReapplyTexture = false;

            if (!Worker.IsBusy)
                Worker.RunWorkerAsync();
        }
    }
}
