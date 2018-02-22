using System;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class SerializableRect
    {
        public float Left;
        public float Right;
        public float Width;
        public float Height;

        public SerializableRect()
        {
            
        }

        public SerializableRect(Rect rect)
        {
            Left = rect.x;
            Right = rect.y;
            Width = rect.width;
            Height = rect.height;
        }

        public Rect ToRect()
        {
            return new Rect(Left, Right, Width, Height);
        }
    }

    [Serializable]
    public class SerializableVector3
    {
        public float X;
        public float Y;
        public float Z;

        public SerializableVector3()
        {
            
        }

        public SerializableVector3(Vector3 vec3)
        {
            X = vec3.x;
            Y = vec3.y;
            Z = vec3.z;
        }

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float GetLength()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public float GetL2()
        {
            return (X * X + Y * Y + Z * Z);
        }

        public float GetQuadratic()
        {
            return (X * X + Y * Y + Z * Z + X * Y + X * Z + Y * Z);
        }
        public float GetMinkowski4()
        {
            return (float)Math.Pow(X * X * X * X + Y * Y * Y * Y + Z * Z * Z * Z, 0.25);
        }
        public float GetMinkowski5()
        {
            const float p = 0.5f;
            return (float)Math.Pow(Math.Pow(Math.Abs(X), p) + Math.Pow(Math.Abs(Y), p) + Math.Pow(Math.Abs(Z), Math.E), 1 / p);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    [Serializable]
    public class SerializableVector2
    {
        public float X;
        public float Y;

        public SerializableVector2()
        {
            
        }

        public SerializableVector2(Vector2 vec2)
        {
            X = vec2.x;
            Y = vec2.y;
        }

        public SerializableVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }

    [Serializable]
    public class SerializableColor
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public SerializableColor()
        {
            
        }
        public SerializableColor(Color color)
        {
            R = color.r;
            G = color.g;
            B = color.b;
            A = color.a;
        }

        public SerializableColor(float r, float g, float b, float a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color ToColor()
        {
            return new Color(R,G,B,A);
        }

        public static SerializableColor operator /(SerializableColor c, float f)
        {
            return new SerializableColor(c.R / f, c.G / f, c.B / f);
        }
        public static SerializableColor operator +(SerializableColor c1, Color c2)
        {
            return new SerializableColor(c1.R + c2.r, c1.G + c2.g, c1.B + c2.b);
        }
    }
}