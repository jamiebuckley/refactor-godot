using System;
using System.Linq;
using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicEditor : Control
    {
        private LogicNodeCreator _logicNodeCreator;

        public override void _Ready()
        {
            _logicNodeCreator = new LogicNodeCreator();
            _logicNodeCreator.Initialise();
            
            var logicToolbox = GetTree().GetRoot().FindNode("LogicToolbox", true, false);
            if (logicToolbox == null) throw new InvalidOperationException("Cannot initialise toolbox as cannot find logicToolbox node in scene");

            Enumeration.GetAll<LogicNodeType>().ToList().ForEach(type =>
            {
                var graphicalNode = _logicNodeCreator.CreateNode(type);
                graphicalNode.SetScale(new Vector2(0.2f, 0.2f));

                var binds = new Godot.Collections.Array();
                binds.Add(graphicalNode);
                graphicalNode.Area2d.Connect("input_event", this, "OnToolboxNodeSelected", binds);
                var control = new Control();
                control.AddChild(graphicalNode);
                control.SetCustomMinimumSize(new Vector2(150, 150));
                logicToolbox.AddChild(control);
            });
        }

        public override void _Process(float delta)
        {
            
        }

        public void OnToolboxNodeSelected(object viewport, object @event, int shape_idx, GraphicalLogicNode graphicalNode)
        {
            if (@event.GetType() == typeof(InputEventMouseButton))
            {
                GD.Print("Selected"  + graphicalNode.Type.Name);                   
            }
        }
    }
}
