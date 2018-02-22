using System;

namespace ProWorldEditor
{
    [Serializable]
    public class LinkGUI
    {
        public Node To;
        public int ToIndex;

        public Node From;

        public static implicit operator bool(LinkGUI exists)
        {
            return exists != null;
        }

        public LinkGUI()
        {

        }

        public LinkGUI(Node from, Node to, int toIndex)
        {
            To = to;
            ToIndex = toIndex;
            From = from;
        }
        public LinkGUI(LinkGUI link)
        {
            To = link.To;
            ToIndex = link.ToIndex;
            From = link.From;
        }
    }
}
