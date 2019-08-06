using System;
using Godot;

namespace Refactor1.Game.Common
{
    public class GameOrientation
    {
        public Point2D Direction { get; set; }
        public string Name { get; set; }

        private GameOrientation(Point2D direction, string name)
        {
            this.Direction = direction;
            this.Name = name;
        }

        public static readonly GameOrientation North = new GameOrientation(new Point2D(0, -1), "North");
        public static readonly GameOrientation South = new GameOrientation(new Point2D(0, 1), "South");
        public static readonly GameOrientation East = new GameOrientation(new Point2D(1, 0), "East");
        public static readonly GameOrientation West = new GameOrientation(new Point2D(-1, 0), "West");

        public static GameOrientation ClockwiseOf(GameOrientation orientation)
        {
            if (orientation == North) return East;
            if (orientation == East) return South;
            if (orientation == South) return West;
            if (orientation == West) return North;
            throw new ArgumentException("Could not recognise orientation " + orientation);
        }

        public static GameOrientation AntiClockwiseOf(GameOrientation orientation)
        {
            if (orientation == North) return West;
            if (orientation == West) return South;
            if (orientation == South) return East;
            if (orientation == East) return North;
            throw new ArgumentException("Could not recognise orientation " + orientation);
        }

        public float ToRotation()
        {
            return Direction.ToRotation();
        }
    }
}