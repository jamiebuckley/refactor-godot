using Godot;

namespace Refactor1.Game.Logic
{
    public class ToolboxNode : Node2D
    {
        public Area2D Area2d { get; set; }
        
        public LogicNodeType Type { get; set; }
        
        public LogicNode LogicNode { get; set; }
    }
}