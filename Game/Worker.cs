using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game
{
    public class Worker : Spatial
    {
        private Point2D _coordinates;
        
        private Point2D _destination;

        public Point2D Destination
        {
            get => _destination;
            set
            {
                _coordinates = _destination;
                _destination = value;
            }
        }

        public Grid Grid { get; set; }

        public override void _Process(float delta)
        {
            if (_coordinates == null) _coordinates = _destination;
            
            var difference = _destination - _coordinates;
            var rotation = difference.ToRotation();
            var degrees = rotation * 57.2958f;
            SetRotationDegrees(new Vector3(0f, degrees, 0f));
            
            var worldDestination = Grid.GetWorldCoordinates(Destination);
            var travelDifference = worldDestination - GetTranslation();
            SetTranslation(GetTranslation() + (travelDifference * 0.1f));
        }
    }
}