using System;
using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicNode
    {
        public class GhostConnection
        {
            public Node2D GhostNode { get; set; }
            
            public LogicNode Owner { get; set; }
            
            public int ChildIndex { get; set; }
        }
        
        public LogicNodeType LogicNodeType { get; set; }

        public LogicNode Parent { get; set; }
        
        public int ChildIndex { get; set; }

        public LogicNode Child1 { get; set; }
        
        public GhostConnection GraphicalGhostNode1 { get; set; }

        public LogicNode Child2 { get; set; }
        
        public GhostConnection GraphicalGhostNode2 { get; set; }

        public Node2D GraphicalNode { get; set; }

        public bool HasChild(int childIndex)
        {
            if (childIndex == 0) return Child1 != null;
            if (childIndex == 1) return Child2 != null;
            throw new ArgumentException($"Cannot check for existence of child {childIndex}");
        }

        public bool IsConnectionEnabled(int connectionIndex)
        {
            if (connectionIndex == 0) return LogicNodeType.ConnectionsIn.Count > 0;
            if (connectionIndex == 1) return LogicNodeType.ConnectionsIn.Count == 2;
            throw new ArgumentException($"Cannot check for state of connection {connectionIndex}");
        }

        public void SetGhost(GhostConnection ghostConnection, int index)
        {
            if (index == 0) GraphicalGhostNode1 = ghostConnection;
            else if (index == 1) GraphicalGhostNode2 = ghostConnection;
            else throw new ArgumentException($"Cannot set ghost connection {index}");
        }

        public GhostConnection GetGhost(int index)
        {
            if (index == 0) return GraphicalGhostNode1;
            if (index == 1) return GraphicalGhostNode2;
            throw new ArgumentException($"Cannot get ghost connection {index}");
        }

        public LogicNode ChildAt(int index)
        {
            if (index == 0) return Child1;
            if (index == 1) return Child2;
            throw new ArgumentException($"Cannot get child {index}");
        }

        public void SetChildAt(LogicNode logicNode, int index)
        {
            if (index == 0) Child1 = logicNode;
            else if (index == 1) Child2 = logicNode;
            else throw new ArgumentException($"Cannot set logic node at {index}");
        }
    }
}