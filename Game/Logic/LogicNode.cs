using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicNode
    {
        public LogicNodeType LogicNodeType { get; set; }

        public LogicNode Parent { get; set; }

        public LogicNode Child1 { get; set; }

        public LogicNode Child2 { get; set; }

        public Node2D GraphicalNode { get; set; }
    }
}