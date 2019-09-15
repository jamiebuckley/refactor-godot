using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game
{
    public class WorkerScene : Spatial
    {
        private Point2D _coordinates;
        
        private Point2D _destination;

        private Sprite3D _speechBubble;

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

        private int speechBubbleFlashTimer = 0;
        private int speechBubbleFlash = 0;

        public override void _Ready()
        {
            _speechBubble = GetNode<Sprite3D>("SpeechBubble");
            _speechBubble.Visible = false;
        }

        public override void _Process(float delta)
        {
            if (_speechBubble.Visible)
            {
                if (speechBubbleFlashTimer > speechBubbleFlash)
                {
                    _speechBubble.Visible = false;
                    speechBubbleFlashTimer = 0;
                }
                else
                {
                    speechBubbleFlashTimer++;
                }
            }
            
            if (_coordinates == null) _coordinates = _destination;
            
            var difference = _destination - _coordinates;
            var rotation = difference.ToRotation();
            var degrees = rotation * 57.2958f;
            SetRotationDegrees(new Vector3(0f, degrees, 0f));
            
            var worldDestination = Grid.GetWorldCoordinates(Destination);
            var travelDifference = worldDestination - GetTranslation();
            SetTranslation(GetTranslation() + (travelDifference * 0.1f));
        }

        public void SpeechBubbleFlash(int time)
        {
            speechBubbleFlashTimer = 0;
            speechBubbleFlash = time;
            _speechBubble.Visible = true;
        }
    }
}