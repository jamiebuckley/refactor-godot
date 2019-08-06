using System;
using Godot;

namespace Refactor1.Game.Common
{
    public class Point2D
    {
        public Point2D(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }
        public int Z { get; }

        public static Point2D operator +(Point2D a, Point2D b)
        {
            return new Point2D(a.X + b.X, a.Z + b.Z);
        }
        
        public static Point2D operator -(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Z - b.Z);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, 0, Z);
        }

        public float ToRotation()
        {
            if (X == 0f && Z == 1f) {
                return 0.0f;
            }
            else if(X == 1f && Z == 0f) {
                return (float) (Math.PI / 2.0f);
            }
            else if (X == 0f && Z == -1f) {
                return (float) Math.PI;
            }
            else if (X == -1f && Z == 0f) {
                return (float) (- Math.PI / 2);
            }
            return 0;
        }
        
    }
}