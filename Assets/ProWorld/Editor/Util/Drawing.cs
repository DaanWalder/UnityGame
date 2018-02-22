using UnityEngine;

namespace ProWorldEditor
{
    public class Drawing
    {
        public static Texture2D AaLineTex;
        public static Texture2D LineTex;

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {
            var savedColor = GUI.color;
            var savedMatrix = GUI.matrix;

            if (!LineTex)
            {
                LineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                LineTex.SetPixel(0, 1, Color.white);
                LineTex.Apply();
            }
            if (!AaLineTex)
            {
                AaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
                AaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
                AaLineTex.SetPixel(0, 1, Color.white);
                AaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
                AaLineTex.Apply();
            }
            if (antiAlias) width *= 3;
            var angle = Vector3.Angle(pointB - pointA, Vector2.right)*(pointA.y <= pointB.y ? 1 : -1);
            var m = (pointB - pointA).magnitude;
            if (m > 0.01f)
            {
                var dz = new Vector3(pointA.x, pointA.y, 0);

                GUI.color = color;
                GUI.matrix = TranslationMatrix(dz)*GUI.matrix;
                GUIUtility.ScaleAroundPivot(new Vector2(m, width), new Vector3(-0.5f, 0, 0));
                GUI.matrix = TranslationMatrix(-dz)*GUI.matrix;
                GUIUtility.RotateAroundPivot(angle, Vector2.zero);
                GUI.matrix = TranslationMatrix(dz + new Vector3(width/2, -m/2)*Mathf.Sin(angle*Mathf.Deg2Rad))*
                             GUI.matrix;

                GUI.DrawTexture(new Rect(0, 0, 1, 1), !antiAlias ? LineTex : AaLineTex);
            }
            GUI.matrix = savedMatrix;
            GUI.color = savedColor;
        }

        public static void BezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color,
                                      float width, bool antiAlias, int segments)
        {
            Vector2 lastV = CubeBezier(start, startTangent, end, endTangent, 0);
            for (int i = 1; i <= segments; ++i)
            {
                Vector2 v = CubeBezier(start, startTangent, end, endTangent, i/(float) segments);

                DrawLine(
                    lastV,
                    v,
                    color, width, antiAlias);
                lastV = v;
            }
        }

        private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float rt = 1 - t;
            float rtt = rt*t;
            return rt*rt*rt*s + 3*rt*rtt*st + 3*rtt*t*et + t*t*t*e;
        }

        private static Matrix4x4 TranslationMatrix(Vector3 v)
        {
            return Matrix4x4.TRS(v, Quaternion.identity, Vector3.one);
        }
    }

}