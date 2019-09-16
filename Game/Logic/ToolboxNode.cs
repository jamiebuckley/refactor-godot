using System;
using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game.Logic
{
    public class ToolboxNode : Node2D
    {
        public Area2D Area2d { get; set; }
        
        public LogicNodeType Type { get; set; }
        
        public LogicNode LogicNode { get; set; }

        public void OnTextChanged(string newtext, string key)
        {
            LogicNode.Tags[key] = newtext;
        }

        public void OnItemChanged(int index)
        {
            var values = Enum.GetValues(typeof(InventoryItem));
            LogicNode.Tags[LogicNode.InventoryItemTag] = values.GetValue(index).ToString();
        }
    }
}