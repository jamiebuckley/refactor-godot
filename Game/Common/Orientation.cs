using Godot;

namespace Refactor1.Game.Common
{
    public class Orientation
    {
        private Vector3 Direction { get; set; }
        private string Name { get; set; }

        private Orientation(Vector3 direction, string name)
        {
            this.Direction = direction;
            this.Name = name;
        }

        private static readonly Orientation North = new Orientation(new Vector3(0, 0, -1), "North");
        private static readonly Orientation South = new Orientation(new Vector3(0, 0, 1), "South");
        private static readonly Orientation East = new Orientation(new Vector3(1, 0, 0), "East");
        private static readonly Orientation West = new Orientation(new Vector3(-1, 0, 0), "West");
        
    }
}