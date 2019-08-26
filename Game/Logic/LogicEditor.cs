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

        private List<GraphicalLogicNode.GhostConnection> _ghosts = new List<GraphicalLogicNode.GhostConnection>();

        private GraphicalLogicNode.GhostConnection _currentSnappedGhost = null;

        private Dictionary<LogicNode, GraphicalLogicNode> _logicNodeGraphics = new Dictionary<LogicNode, GraphicalLogicNode>();

        public override void _Ready()
        {
            _logicNodeCreator = new LogicNodeCreator();
            _logicNodeCreator.Initialise();

            var saveButton = GetTree().GetRoot().FindNode("LogicSaveButton", true, false) as Button;
            saveButton.Connect("pressed", this, "OnSaveButtonPressed");
            
            var logicToolbox = GetTree().GetRoot().FindNode("LogicToolbox", true, false);
            if (logicToolbox == null) throw new InvalidOperationException("Cannot initialise toolbox as cannot find logicToolbox node in scene");

            Enumeration.GetAll<LogicNodeType>().ToList().ForEach(type =>
            {
                var graphicalNode = _logicNodeCreator.CreateNode(type);
                graphicalNode.SetScale(new Vector2(0.2f, 0.2f));

                var binds = new Godot.Collections.Array { graphicalNode };
                graphicalNode.Area2d.Connect("input_event", this, "OnToolboxNodeSelected", binds);
                var control = new Control();
                control.AddChild(graphicalNode);
                control.SetCustomMinimumSize(new Vector2(150, 150));
                logicToolbox.AddChild(control);
            });


            var graphicalRootNode = _logicNodeCreator.CreateNode(LogicNodeType.Root);
            graphicalRootNode.SetScale(new Vector2(0.2f, 0.2f));
            AddChild(graphicalRootNode);

            var root = new LogicNode
            {
                LogicNodeType = LogicNodeType.Root
            };
            
            _roots.Add(root);
            
            _logicNodeGraphics.Add(root, new GraphicalLogicNode()
            {
                GraphicalNode = graphicalRootNode   
            });
            UpdateGhosts();
        }

        private void UpdateGhosts()
        {
            var validGhosts = new List<GraphicalLogicNode.GhostConnection>();
            
            var stack = new Stack<LogicNode>(_roots);
            while (stack.Any())
            {
                var node = stack.Pop();
                for (var i = 0; i < 2; i++)
                {
                    if (!node.IsConnectionEnabled(i)) continue;
                    
                    if (!node.HasChild(i))
                    {
                        if (GetGhost(node, i) == null)
                        {
                            var ghostNode = _logicNodeCreator.CreateGhostNode();
                            ghostNode.SetScale(new Vector2(0.2f, 0.2f));
                            ghostNode.Position = _logicNodeGraphics[node].GraphicalNode.Position + new Vector2(120, i * 100);
                            AddChild(ghostNode);

                            var ghostConnection = new GraphicalLogicNode.GhostConnection
                            {
                                GhostNode = ghostNode,
                                Owner = node,
                                ChildIndex = i
                            };
                            _ghosts.Add(ghostConnection);
                            validGhosts.Add(ghostConnection);
                            SetGhost(node, ghostConnection, i);
                        }
                        else
                        {
                            validGhosts.Add(GetGhost(node, i));
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
                SetGhost(ghostConnection.Owner, null, ghostConnection.ChildIndex);
                GD.Print("removing ghost");
            }

            _ghosts = validGhosts;
        }

        private bool IsValidConnection(GraphicalLogicNode.GhostConnection ghostConnection, ToolboxNode toolboxNode)
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
                };

                _logicNodeGraphics.Add(node, new GraphicalLogicNode()
                {
                    GraphicalNode = _draggedNode
                });
                
                _draggedNode.LogicNode = node;
                
                AddChild(_draggedNode);
            }
        }
        
        private void SetGhost(LogicNode logicNode, GraphicalLogicNode.GhostConnection ghostConnection, int index)
        {
            var graphics = _logicNodeGraphics[logicNode];
            if (index == 0)  graphics.GraphicalGhostNode1 = ghostConnection;
            else if (index == 1) graphics.GraphicalGhostNode2 = ghostConnection;
            else throw new ArgumentException($"Cannot set ghost connection {index}");
        }

        public GraphicalLogicNode.GhostConnection GetGhost(LogicNode logicNode, int index)
        {
            var graphics = _logicNodeGraphics[logicNode];
            if (index == 0) return graphics.GraphicalGhostNode1;
            if (index == 1) return graphics.GraphicalGhostNode2;
            throw new ArgumentException($"Cannot get ghost connection {index}");
        }

        public void OnSaveButtonPressed()
        {
            // write tree to main state
            GD.Print("save");
        }
    }
}
