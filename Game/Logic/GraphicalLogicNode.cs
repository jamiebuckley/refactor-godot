using Godot;

namespace Refactor1.Game.Logic
{
    public class GraphicalLogicNode : Node2D
    {
        public Area2D Area2d { get; set; }
        
        public LogicNodeType Type { get; set; }
    }
}