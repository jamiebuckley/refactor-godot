using System;
using Godot;
using Godot.Collections;

namespace Refactor1.Game.Logic
{
    public class LogicNode
    {
        
        public static readonly string NumericalValueTag = "NumericalValue";

        public static readonly string InventoryItemTag = "InventoryItem";

        public LogicNodeType LogicNodeType { get; set; }

        public LogicNode Parent { get; set; }
        
        public int ChildIndex { get; set; }

        public LogicNode Child1 { get; set; }

        public LogicNode Child2 { get; set; }

        public Dictionary<string, string> Tags { get; } = new Dictionary<string, string>();

        public LogicNode()
        {
            
        }

        public LogicNode(LogicNodeType logicNodeType, LogicNode parent = null)
        {
            this.LogicNodeType = logicNodeType;
            this.Parent = parent;
        }

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

        public LogicNode ChildAt(int index)
        {
            if (index == 0) return Child1;
            if (index == 1) return Child2;
            throw new ArgumentException($"Cannot get child {index}");
        }

        public void SetChildAt(LogicNode logicNode, int index)
        {
            if (index == 0)
                Child1 = logicNode;
            else if (index == 1)
                Child2 = logicNode;
            else throw new ArgumentException($"Cannot set logic node at {index}");
            
            logicNode.ChildIndex = index;
            logicNode.Parent = this;
        }
    }
}