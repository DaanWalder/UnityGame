using System;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using ProWorldSDK;

namespace ProWorldEditor
{
    [Serializable]
    public abstract class Node : AreaLayout, ISerializable
    {
        public enum NodeType
        {
            Generator,
            GenMod,
            Modifier,
            Combine,
            Output
        }

        public const int Resolution = 512;

        private const int PreviewSize = 64;

        [NonSerialized] public Texture2D OutputTexture;
        [NonSerialized] private readonly int _windowID;
        [NonSerialized] private MapEditor _mapEditor;
        [NonSerialized] private bool _rebuildTexture;
        //[NonSerialized] private readonly BackgroundWorker _bw = new BackgroundWorker();
        [NonSerialized] private bool _isOptionChanged;
        [NonSerialized] private bool _execute;

        public NodeData Data;
        public LinkGUI[] Links;

        public Rect WindowRect;
        public string Title = string.Empty;

        public bool IsHasOutput = true;

        public static implicit operator bool(Node exists)
        {
            return exists != null;
        }

        protected Node(MapEditor mapEditor, NodeData data, string title)
        {
            _windowID = WindowID.ID++;

            _mapEditor = mapEditor;
            Data = data;
            Title = title;

            Links = new LinkGUI[Data.InputData.Length];

            Setup();
            Run();
        }
        public void Set(MapEditor pme)
        {
            _mapEditor = pme;
        }

        private void Setup()
        {
            OutputTexture = Util.BlackTexture(256);
            Data.CalculateDone += OnCalculateDone;
        }

        public override void OnGUI()
        {
            Checks();

            if (_mapEditor.CurrentNode == this)
                GUI.color = Color.red;
            WindowRect = GUILayout.Window(_windowID, WindowRect, DoWindow, Title);
            GUI.color = Color.white;

            WindowRect.x = Mathf.Max(WindowRect.xMin, 0);
            WindowRect.y = Mathf.Max(WindowRect.yMin, 0);
        }
        public override void Clean()
        {
            UnityEngine.Object.DestroyImmediate(OutputTexture);
            _mapEditor = null;

            Data.CalculateDone -= OnCalculateDone;
        }

        public virtual void Options()
        {
            GUILayout.Label("Time Taken: " + Data.TimeTaken.ToString("0") + "ms");
        }

        public void DoWindow(int windowID)
        {
            GUI.color = Color.white;
            if ((Event.current.button == 0) && (Event.current.type == EventType.MouseUp))
            {
                _mapEditor.CurrentNode = this;
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 15));

            GUILayout.BeginHorizontal();
            InputsBarGUI();
            OutputGUI();
            GUILayout.EndHorizontal();
        }

        private void Checks()
        {
            if(_rebuildTexture)
            {
                var d = Util.ResizeArray(Data.OutputData, 256);
                Util.ApplyMapToTexture(ref OutputTexture, d);
                _rebuildTexture = false;
            }
        }

        protected virtual void InputsBarGUI()
        {
            GUI.enabled = !_mapEditor.IsCalculating;

            GUILayout.BeginVertical(GUILayout.Width(15));

            for (var index = 0; index < Data.InputData.Length; index++)
            {
                if (GUILayout.Button(">", EditorStyles.miniButton))
                {
                    _mapEditor.InputLink(this, index);
                }
            }

            GUILayout.EndVertical();

            GUI.enabled = true;
        }

        private void OutputGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Box(OutputTexture, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
            GUILayout.Space(12);
            GUILayout.EndVertical();

            GUI.enabled = !_mapEditor.IsCalculating;

            GUILayout.BeginVertical(GUILayout.Width(15));

            if (IsHasOutput)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("X", EditorStyles.miniButton))
                {
                    _mapEditor.CloseWindow(this);
                }
                GUI.color = Color.white;

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(">", EditorStyles.miniButton))
                {
                    _mapEditor.OutputLink(this);
                }

                GUILayout.Space(5);
            }

            GUILayout.EndVertical();

            GUI.enabled = true;
        }

        public void UpdateOptionsChanged()
        {
            if ((_isOptionChanged && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)) || _execute)
            {
                Run();
                _isOptionChanged = false;
                _execute = false;
            }
            if (Event.current.type == EventType.ExecuteCommand)
            {
                _execute = true;
            }

            // If options has changed, check we are finished changing
        }

        public void CheckOptionChanged()
        {
            

            // Check if options has be used
            if (Event.current.type == EventType.Used)
            {
                _isOptionChanged = true;
            }
        }

        public void Run()
        {
            //Check which method called another method
            if (_mapEditor != null)
            {
                _mapEditor.IsCalculating = true;
            }

            Data.Run();
        }
        private void OnCalculateDone()
        {
            _rebuildTexture = true;
        }
        #region ISerializable

        protected Node(SerializationInfo info, StreamingContext context)
        {
            Data = (NodeData)info.GetValue("Data", typeof(NodeData));
            Links = (LinkGUI[])info.GetValue("Links", typeof(LinkGUI[]));
            Title = info.GetString("Title");
            WindowRect = ((SerializableRect)info.GetValue("WindowRect", typeof(SerializableRect))).ToRect();
            IsHasOutput = info.GetBoolean("IsHasOutput");

            _windowID = WindowID.ID++;

            Setup();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data", Data, typeof(NodeData));
            info.AddValue("Links", Links);
            info.AddValue("Title", Title);
            info.AddValue("WindowRect", new SerializableRect(WindowRect));
            info.AddValue("IsHasOutput", IsHasOutput);
        }
        #endregion
    }
}
