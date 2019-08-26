using Godot;

namespace Refactor1.Game.Logic
{
    public class GraphicalLogicNode
    {
        public class GhostConnection
        {
            public Node2D GhostNode { get; set; }
            
            public LogicNode Owner { get; set; }
            
            public int ChildIndex { get; set; }
        }
        
        public GhostConnection GraphicalGhostNode1 { get; set; }
        
        public GhostConnection GraphicalGhostNode2 { get; set; }
        
        public Node2D GraphicalNode { get; set; }
    }
}