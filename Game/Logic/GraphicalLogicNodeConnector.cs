using System;
using Godot;

namespace Refactor1.Game.Logic
{
    public class GraphicalLogicNodeConnector : Node2D
    {
        private Sprite _horizontalLeft;
        private Sprite _horizontalRight;
        private TextureRect vertical;
        
        public override void _Ready()
        {
            _horizontalLeft = GetNode("horiz_left") as Sprite;
            _horizontalRight = GetNode("horiz_right") as Sprite;
            vertical = GetNode("vertical") as TextureRect;
        }

        public void SetLineFromTo(Vector2 origin, Vector2 destination)
        {
            if (Math.Abs(destination.y - origin.y) < 1)
            {
                vertical.Visible = false;
                _horizontalRight.Position = new Vector2(_horizontalRight.Position.x, _horizontalLeft.Position.y);
            }
            else
            {
                vertical.Visible = true;
                _horizontalRight.Position = new Vector2(_horizontalRight.Position.x, destination.y - origin.y);
                vertical.SetSize(new Vector2(vertical.GetSize().x, destination.y - origin.y - 20));
            }
        }
    }
}