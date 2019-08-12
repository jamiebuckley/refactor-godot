using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicEditorViewport : ViewportContainer
    {
        public override void _Input(InputEvent @event)
        {
            GetNode("LogicMinigame")._UnhandledInput(@event);
        }
    }
}