using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicEditorViewport : Viewport
    {
        public override void _Input(InputEvent @event)
        {
            _UnhandledInput(@event);
        }
    }
}