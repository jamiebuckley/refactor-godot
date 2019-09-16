using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Refactor1.Game.Common;

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
        
        private Point2D _coordinates;

        private VBoxContainer logicToolboxVBox;

        public override void _Ready()
        {
            _logicNodeCreator = new LogicNodeCreator();
            _logicNodeCreator.Initialise();

            ConnectSaveButton();
            SetupToolbox();
            InitializeRoots();
            UpdateGhosts();
            
            logicToolboxVBox = GetTree().GetRoot().FindNode("LogicToolboxVBox", true, false) as VBoxContainer;
        }

        private void ConnectSaveButton()
        {
            var saveButton = GetTree().GetRoot().FindNode("LogicSaveButton", true, false) as Button;
            saveButton.Connect("pressed", this, "OnSaveButtonPressed");
        }

        private void SetupToolbox()
        {
            var logicToolbox = GetTree().GetRoot().FindNode("LogicToolbox", true, false);
            if (logicToolbox == null) throw new InvalidOperationException("Cannot initialise toolbox as cannot find logicToolbox node in scene");

            Enumeration.GetAll<LogicNodeType>().ToList().ForEach(type =>
            {
                var graphicalNode = _logicNodeCreator.CreateNode(type, false);
                graphicalNode.SetScale(new Vector2(0.2f, 0.2f));

                var binds = new Godot.Collections.Array { graphicalNode };
                graphicalNode.Area2d.Connect("input_event", this, "OnToolboxNodeSelected", binds);
                var control = new Control();
                control.AddChild(graphicalNode);
                control.SetCustomMinimumSize(new Vector2(150, 150));
                logicToolbox.AddChild(control);
            });
        }

        private void InitializeRoots()
        {
            var graphicalRootNode = _logicNodeCreator.CreateNode(LogicNodeType.Root, true);
            graphicalRootNode.SetScale(new Vector2(0.2f, 0.2f));
            AddChild(graphicalRootNode);

            var root = new LogicNode { LogicNodeType = LogicNodeType.Root };
            _roots.Add(root);
            _logicNodeGraphics.Add(root, new GraphicalLogicNode { GraphicalNode = graphicalRootNode });
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
            if (!_draggedNode.Area2d.IsConnected("input_event", this, "OnEditorNodeSelected"))
            {
                _draggedNode.Area2d.Connect("input_event", this, "OnEditorNodeSelected", binds);
            }
            
            var rect = logicToolboxVBox.GetRect();
            if (rect.HasPoint(GetGlobalMousePosition()))
            {
                RemoveChild(_draggedNode);
            }

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
                _draggedNode = _logicNodeCreator.CreateNode(graphicalNode.Type, true);
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
            GD.Print("save");
            var main = GetTree().GetRoot().FindNode("RootSpatial", true, false) as Main;
            main.SaveLogicTile(_coordinates, _roots);
        }

        public void SetCoordinates(Point2D gridCoords)
        {
            this._coordinates = gridCoords;
        }

        public void LoadTree(List<LogicNode> logicNodes)
        {
            this._roots = logicNodes;
            LoadTreeNode(null, logicNodes.ElementAt(0), 0);
            UpdateGhosts();
        }

        public void LoadTreeNode(ToolboxNode parent, LogicNode thisNode, int child1)
        {
            var graphics = _logicNodeCreator.CreateNode(thisNode.LogicNodeType, true);
            graphics.LogicNode = thisNode;
            graphics.SetScale(new Vector2(0.2f, 0.2f));
            
            var binds = new Godot.Collections.Array { graphics };
            graphics.Area2d.Connect("input_event", this, "OnEditorNodeSelected", binds);
            AddChild(graphics);

            if (parent != null)
            {
                if (child1 == 1)
                {
                    graphics.SetPosition(parent.GetPosition() + new Vector2(120, 0));
                }

                if (child1 == 2)
                {
                    graphics.SetPosition(parent.GetPosition() + new Vector2(120, 100));
                }
            }
            
            _logicNodeGraphics.Add(thisNode, new GraphicalLogicNode()
            {
                GraphicalNode = graphics,
            });
            
            if (thisNode.HasChild(0)) LoadTreeNode(graphics, thisNode.Child1, 1);
            if (thisNode.HasChild(1)) LoadTreeNode(graphics, thisNode.Child2, 2);
        }
    }
}
