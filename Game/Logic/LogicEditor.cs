using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicEditor : Control
    {
        private LogicNodeCreator _logicNodeCreator;

        private ToolboxNode _draggedNode;

        private List<LogicNode> _roots = new List<LogicNode>();

        private List<LogicNode.GhostConnection> _ghosts = new List<LogicNode.GhostConnection>();

        private LogicNode.GhostConnection _currentSnappedGhost = null;

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


            var graphicalRootNode = _logicNodeCreator.CreateNode(LogicNodeType.Root);
            graphicalRootNode.SetScale(new Vector2(0.2f, 0.2f));
            AddChild(graphicalRootNode);
            
            _roots.Add(new LogicNode
            {
                LogicNodeType = LogicNodeType.Root,
                GraphicalNode = graphicalRootNode
            });
            UpdateGhosts();
        }

        private void UpdateGhosts()
        {
            var validGhosts = new List<LogicNode.GhostConnection>();
            
            var stack = new Stack<LogicNode>(_roots);
            while (stack.Any())
            {
                var node = stack.Pop();
                for (var i = 0; i < 2; i++)
                {
                    if (!node.IsConnectionEnabled(i)) continue;
                    
                    if (!node.HasChild(i))
                    {
                        if (node.GetGhost(i) == null)
                        {
                            var ghostNode = _logicNodeCreator.CreateGhostNode();
                            ghostNode.SetScale(new Vector2(0.2f, 0.2f));
                            ghostNode.Position = node.GraphicalNode.Position + new Vector2(120, i * 100);
                            AddChild(ghostNode);

                            var ghostConnection = new LogicNode.GhostConnection
                            {
                                GhostNode = ghostNode,
                                Owner = node,
                                ChildIndex = i
                            };
                            _ghosts.Add(ghostConnection);
                            validGhosts.Add(ghostConnection);
                            node.SetGhost(ghostConnection, i);
                        }
                        else
                        {
                            validGhosts.Add(node.GetGhost(i));
                        }
                    }
                    else
                    {
                        stack.Push(node.ChildAt(i));
                    }
                }
            }
            
            // remove invalid ghosts
            var invalidGhosts = _ghosts.Where(g => !validGhosts.Contains(g)).ToList();
            foreach (var ghostConnection in invalidGhosts)
            {
                RemoveChild(ghostConnection.GhostNode);
                ghostConnection.Owner.SetGhost(null, ghostConnection.ChildIndex);
                GD.Print("removing ghost");
            }

            _ghosts = validGhosts;
        }

        private bool IsValidConnection(LogicNode.GhostConnection ghostConnection, ToolboxNode toolboxNode)
        {
            var draggedNodeConnectionOut = toolboxNode.Type.ConnectionOut;

            if (!ghostConnection.Owner.IsConnectionEnabled(ghostConnection.ChildIndex))
            {
                return false;
            }
            
            var parentInputConnection = ghostConnection.Owner.LogicNodeType.ConnectionsIn[ghostConnection.ChildIndex];
            return draggedNodeConnectionOut == parentInputConnection;
        }

        public override void _Process(float delta)
        {
            if (_draggedNode == null) return;
            
            
            // if the node is < 100 from any anchor points, snap
            // check if the anchor point and the snapping point is of matching type
            var closeGhosts = _ghosts
                .Where(x => (x.GhostNode.Position - GetGlobalMousePosition()).Length() < 100)
                .Where(x => IsValidConnection(x, _draggedNode))
                .ToList();
            
            if (closeGhosts.Any())
            {
                var closeGhost = closeGhosts.ElementAt(0);
                _draggedNode.SetPosition(closeGhost.GhostNode.Position);
                _currentSnappedGhost = closeGhost;
            }
            else
            {
                _draggedNode.SetPosition(GetGlobalMousePosition());
                _currentSnappedGhost = null;
            }

            if (Input.IsMouseButtonPressed(1)) return;
            
            
            if (_currentSnappedGhost != null)
            {
                var newParent = _currentSnappedGhost.Owner;
                _draggedNode.LogicNode.Parent = newParent;
                _draggedNode.LogicNode.ChildIndex = _currentSnappedGhost.ChildIndex;
                newParent.SetChildAt(_draggedNode.LogicNode ,_currentSnappedGhost.ChildIndex);
                UpdateGhosts();
            }

            var binds = new Godot.Collections.Array { _draggedNode };
            _draggedNode.Area2d.Connect("input_event", this, "OnEditorNodeSelected", binds);
            _draggedNode = null;
        }

        public void OnEditorNodeSelected(object viewport, object @event, int shape_idx, ToolboxNode graphicalNode)
        {
            if (@event is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.Pressed)
            {
                _draggedNode = graphicalNode;

                if (_draggedNode.LogicNode.Parent != null)
                {
                    // disconnect
                    _draggedNode.LogicNode.Parent.SetChildAt(null, _draggedNode.LogicNode.ChildIndex);
                    
                    _draggedNode.LogicNode.Parent = null;
                    _draggedNode.LogicNode.ChildIndex = 0;
                }
                
                UpdateGhosts();
            }
        }

        public void OnToolboxNodeSelected(object viewport, object @event, int shape_idx, ToolboxNode graphicalNode)
        {
            if (@event.GetType() != typeof(InputEventMouseButton)) return;
            
            if (@event is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.Pressed)
            {
                _draggedNode = _logicNodeCreator.CreateNode(graphicalNode.Type);
                _draggedNode.SetScale(new Vector2(0.2f, 0.2f));
                
                var node = new LogicNode
                {
                    LogicNodeType = _draggedNode.Type,
                    GraphicalNode = _draggedNode
                };
                
                _draggedNode.LogicNode = node;
                
                AddChild(_draggedNode);
            }
        }
    }
}
