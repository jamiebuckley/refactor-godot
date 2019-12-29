using System;
using Godot;

namespace Refactor1.Game.Logic
{
    public class GraphicalLogicNodeConnector : Node2D
    {
        private TextureRect _horizontalLeft;
        private TextureRect _horizontalRight;
        private TextureRect vertical;
        
        public override void _Ready()
        {
            _horizontalLeft = GetNode("horiz_left") as TextureRect;
            _horizontalRight = GetNode("horiz_right") as TextureRect;
            vertical = GetNode("vertical") as TextureRect;
        }

        public void SetLineFromTo(Vector2 origin, Vector2 destination)
        {
            if (Math.Abs(destination.y - origin.y) < 1)
            {
                vertical.Visible = false;
                _horizontalRight.RectPosition = new Vector2(_horizontalRight.RectPosition.x, _horizontalLeft.RectPosition.y);
            }
            else
            {
                vertical.Visible = true;
                _horizontalRight.RectPosition = new Vector2(_horizontalRight.RectPosition.x, destination.y - origin.y);
                vertical.SetSize(new Vector2(vertical.GetSize().x, (destination.y - origin.y) * 2));
            }
        }
    }
}