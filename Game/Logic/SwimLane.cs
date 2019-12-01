using Godot;

namespace Refactor1.Game.Logic
{
    public class SwimLane : Node2D
    {
        private Control parentContainer;

        private float scale = 1.0f;

        public override void _Ready()
        {
            parentContainer = GetParent() as Control;
        }

        public void SetScaleFloat(float scale)
        {
            SetScale(new Vector2(scale, scale));
        }
    }
}