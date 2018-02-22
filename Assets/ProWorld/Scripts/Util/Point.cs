using System;
using UnityEngine;

namespace ProWorldSDK
{
    [Serializable]
    public class Point
    {
        public int X = 0;
        public int Y = 0;

        public Point()
        {
            
        }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Point(Vector2 v2)
        {
            X = (int) v2.x;
            Y = (int) v2.y;
        }
        /// <summary>
        /// Converts vector3 to Point
        /// </summary>
        /// <param name="v3">Uses x/z position</param>
        public Point(Vector3 v3)
        {
            X = (int)v3.x;
            Y = (int)v3.z;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static Point operator +(Point p1, int constant)
        {
            return new Point(p1.X + constant, p1.Y + constant);
        }
        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static Point operator -(Point p1, int constant)
        {
            return new Point(p1.X - constant, p1.Y - constant);
        }
        public static Point operator /(Point p, int constant)
        {
            return new Point(p.X / constant, p.Y / constant);
        }
        public static Point operator *(Point p, int constant)
        {
            return new Point(p.X * constant, p.Y * constant);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var p = obj as Point;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (X == p.X) && (Y == p.Y);
        }

        public bool Equals(Point p)
        {
            // If parameter is null return false:
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (X == p.X) && (Y == p.Y);
        }

        public override int GetHashCode()
        {
// ReSharper disable NonReadonlyFieldInGetHashCode
            return X ^ Y;
// ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public override string ToString()
        {
            return "Point (" + X + "," + Y + ")";
        }
    }
}