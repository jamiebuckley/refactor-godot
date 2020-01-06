using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Refactor1.Game.Common;
using Array = Godot.Collections.Array;

namespace Refactor1.Game.Logic
{
    public class LogicEditor : Control
    {
        private static class LogicEditorDimensions
        {
            private static readonly int BaseUnit = 8;
            
            /// <summary>
            /// How far from the top left of a node a line is drawn, for a top and bottom line
            /// </summary>
            public static readonly List<Vector2> LineOffsets = new List<Vector2>()
            {
                new Vector2(21 * BaseUnit, 3 * BaseUnit),
                new Vector2(21 * BaseUnit, 19 * BaseUnit)
            };

            /// <summary>
            /// How far from the top left of a node a child is drawn, for a top and bottom child
            /// </summary>
            public static readonly List<Vector2> ChildOffsets = new List<Vector2>()
            {
                new Vector2(21 * BaseUnit + 16 * BaseUnit, 0),
                new Vector2(21 * BaseUnit + 16 * BaseUnit, 16 * BaseUnit)
            };
        }

        private LogicEditorPackedScenes _packedScenes;
        
        private Node2D _logicNodeChoiceBox;
        
        private LogicNode _editingLogicNode;
        private int _logicNodeChoiceBoxGhostIndex;

        private float _scale = 0.5f;
        private Vector2 _lastMousePosition;
        private bool _isDragging;
        
        private List<LogicNode> _roots = new List<LogicNode>();
        
        private readonly Dictionary<LogicNode, Node> _graphicalNodes = new Dictionary<LogicNode, Node>();
        private readonly Dictionary<LogicNode, Dictionary<int, Node>> _ghostNodes = new Dictionary<LogicNode, Dictionary<int, Node>>();
        private readonly Dictionary<LogicNode, Dictionary<int, Node>> _lines = new Dictionary<LogicNode, Dictionary<int, Node>>();

        private SwimLane _swimLane;

        public override void _Ready()
        {
            _packedScenes = new LogicEditorPackedScenes();

            SetRoots(new List<LogicNode> { DebugTree() });
            var swimLane = GetNode("SwimLane1") as SwimLane;
            swimLane?.SetScaleFloat(_scale);
        }

        private LogicNode DebugTree()
        {
            var root = new LogicNode(LogicNodeType.Root);
            root.SetChildAt(new LogicNode(LogicNodeType.ToggleIf), 0);
            root.Child1.SetChildAt(new LogicNode(LogicNodeType.And), 0);
            return root;
        }

        public void SetRoots(List<LogicNode> logicNodes)
        {
            // clear current swimlanes
            _roots = logicNodes;
            _roots.ForEach(BuildTree);
        }

        private void BuildTree(LogicNode root)
        {
            if (_swimLane == null)
            {
                _swimLane = new SwimLane { Name = "SwimLane1" };
                AddChild(_swimLane);
            }

            AddNode(_swimLane, root, new Vector2());
        }

        private int AddNode(Node2D swimLane, LogicNode logicNode, Vector2 position)
        {
            var node = GetOrCreateGraphicalNodeFor(logicNode);
            node.UpdateVisuals(logicNode.LogicNodeType.Name, new Color(logicNode.LogicNodeType.Colour), position);
            
            var previousColumnSpacing = 0;
            // for each potential child
            for (var i = 0; i <= 1; i++)
            {
                if (!logicNode.IsConnectionEnabled(i)) continue;
                
                // calculate the position of this node
                var multiColumnSpacing = previousColumnSpacing * new Vector2(0, 150);
                var nodePosition = position + LogicEditorDimensions.ChildOffsets[i] + multiColumnSpacing;

                // draw line to the child/ghost
                var line = GetOrCreateLine(logicNode, i);
                line.SetPosition(position + LogicEditorDimensions.LineOffsets[i]);
                
                var target = new Vector2(nodePosition.x, nodePosition.y + LogicEditorDimensions.LineOffsets[0].y);
                line.SetLineFromTo(position + LogicEditorDimensions.LineOffsets[i], target);
                    
                var ghostNode = GetGhostNodeIfExists(logicNode, i);
                if (logicNode.HasChild(i))
                {
                    if (ghostNode != null)
                    {
                        DeleteGhostNode(logicNode, i);
                    }
                    previousColumnSpacing += AddNode(swimLane, logicNode.ChildAt(i), nodePosition);
                }
                else
                {
                    if (ghostNode == null)
                    {
                        ghostNode = _packedScenes.GhostNodePackedScene.Instance() as GraphicalLogicNode;
                        ghostNode.GhostInformation = new GhostIndexInformation() {ChildIndex = i, LogicNode = logicNode};
                        ghostNode.OnPressed += OnGraphicalNodePressed;
                        swimLane.AddChild(ghostNode);
                    }

                    ghostNode.Position = nodePosition;

                    if (!_ghostNodes.ContainsKey(logicNode)) _ghostNodes[logicNode] = new Dictionary<int, Node>();
                    _ghostNodes[logicNode][i] = ghostNode;
                }
            }

            return logicNode.IsConnectionEnabled(1) ? previousColumnSpacing + 1 : previousColumnSpacing;
        }

        private GraphicalLogicNode GetGhostNodeIfExists(LogicNode item, int index)
        {
            if (!_ghostNodes.ContainsKey(item)) return null;
            if (!_ghostNodes[item].ContainsKey(index)) return null;
            return _ghostNodes[item][index] as GraphicalLogicNode;
        }
        
        private GraphicalLogicNode GetOrCreateGraphicalNodeFor(LogicNode logicNode)
        {
            if (_graphicalNodes.ContainsKey(logicNode)) return _graphicalNodes[logicNode] as GraphicalLogicNode;
            if (!_packedScenes.LogicNodeTypeToPackedScene.ContainsKey(logicNode.LogicNodeType))
                throw new ArgumentException($"Couldn't create node for {logicNode.LogicNodeType}");
            var graphicalLogicNode = (GraphicalLogicNode)_packedScenes.LogicNodeTypeToPackedScene[logicNode.LogicNodeType].Instance();
            _graphicalNodes[logicNode] = graphicalLogicNode;
            graphicalLogicNode.OnPressed += OnGraphicalNodePressed;
            _swimLane.AddChild(graphicalLogicNode);
            return graphicalLogicNode;
        }

        private GraphicalLogicNodeConnector GetOrCreateLine(LogicNode item, int index)
        {
            if (!_lines.ContainsKey(item) || !_lines[item].ContainsKey(index))
            {
                var line = _packedScenes.ConnectorPackedScene.Instance() as GraphicalLogicNodeConnector;
                if (!_lines.ContainsKey(item)) _lines[item] = new Dictionary<int, Node>();
                _lines[item][index] = line;
                _swimLane.AddChild(line);
            }
            return _lines[item][index] as GraphicalLogicNodeConnector;
        }

        private void DeleteGhostNode(LogicNode item, int index)
        {
            if (!_ghostNodes.ContainsKey(item)) return;
            if (!_ghostNodes[item].ContainsKey(index)) return;
            
            var ghostNode = _ghostNodes[item][index] as GraphicalLogicNode;
            _swimLane.RemoveChild(ghostNode);
            ghostNode.QueueFree();

            var ghostNodeForItem = _ghostNodes[item];
            ghostNodeForItem.Remove(index);
            if (!ghostNodeForItem.Any())
            {
                _ghostNodes.Remove(item);
            }
        }

        private void OnGraphicalNodePressed(GraphicalLogicNode graphicalLogicNode)
        {
            if (_logicNodeChoiceBox != null)
            {
                _swimLane.RemoveChild(_logicNodeChoiceBox);
                _logicNodeChoiceBox.QueueFree();
                _logicNodeChoiceBox = null;
            }
            if (graphicalLogicNode.LogicNode != null)
            {
                
            }
            else if (graphicalLogicNode.GhostInformation != null)
            {
                // create a choice box
                var choiceBox = _packedScenes.LogicNodeChoiceBoxPackedScene.Instance() as Node2D;
                choiceBox.Position = graphicalLogicNode.Position + new Vector2(0, 100);
                _swimLane.AddChild(choiceBox);
                
                // store the choice box and the information about what we are editing
                _logicNodeChoiceBox = choiceBox;
                _editingLogicNode = graphicalLogicNode.GhostInformation.LogicNode;
                _logicNodeChoiceBoxGhostIndex = graphicalLogicNode.GhostInformation.ChildIndex;

                // populate the choice box
                var container = choiceBox.GetNode("choiceContainer") as Container;
                
                // connect the first choice to closing the box
                container.GetChild(0).Connect("gui_input", this, "CloseLogicChoiceBox");
                
                // find all logic node types that are valid for this ghost 
                var connection = graphicalLogicNode.GhostInformation.LogicNode.LogicNodeType.ConnectionsIn[graphicalLogicNode.GhostInformation.ChildIndex];
                var matchingLogicNodeTypes = Enumeration.GetAll<LogicNodeType>().ToList().Where(x => x.ConnectionOut == connection).ToList();
                
                // add them to the container
                matchingLogicNodeTypes.ForEach(type =>
                {
                    var choice = _packedScenes.LogicNodeChoiceBoxChoicePackedScene.Instance();
                    ((Label) choice.GetNode("label")).Text = type.Name;
                    ((TextureRect) choice.GetNode("texture")).Modulate = new Color(type.Colour);
                    container.AddChild(choice);
                    choice.Connect("gui_input", this, "OnLogicChoiceBoxChoiceInput", new Array { type.Id });
                });
            }
        }

        public override void _Process(float delta)
        {
            if (Input.IsMouseButtonPressed((int) ButtonList.Middle))
            {
                if (!_isDragging)
                {
                    _lastMousePosition = GetLocalMousePosition();
                }

                _isDragging = true;
                var difference = GetLocalMousePosition() - _lastMousePosition;
                var swimlane = GetNode("SwimLane1") as SwimLane;
                swimlane.SetPosition(swimlane.GetPosition() + difference);
                _lastMousePosition = GetLocalMousePosition();
            }
            else
            {
                _isDragging = false;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouse && GetRect().HasPoint(GetLocalMousePosition()))
            {
                if (@event is InputEventMouseButton iemb)
                {
                    if (iemb.ButtonIndex == (int) ButtonList.WheelUp)
                    {
                        _scale += 0.02f;
                        (GetNode("SwimLane1") as SwimLane).SetScaleFloat(_scale);
                    }
                    else if (iemb.ButtonIndex == (int) ButtonList.WheelDown)
                    {
                        _scale -= 0.02f;
                        (GetNode("SwimLane1") as SwimLane).SetScaleFloat(_scale);
                    }
                }
            }
        }
        
        // ReSharper disable once UnusedMember.Global
        public void CloseLogicChoiceBox(object @event)
        {
            if (@event is InputEventMouseButton imb && !imb.Pressed && imb.ButtonIndex == (int) ButtonList.Left)
            {
                if (_logicNodeChoiceBox != null)
                {
                    _swimLane.RemoveChild(_logicNodeChoiceBox);
                    _logicNodeChoiceBox.QueueFree();
                    _logicNodeChoiceBox = null;
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public void OnLogicChoiceBoxChoiceInput(object @event, int typeId)
        {
            if (@event is InputEventMouseButton imb && !imb.Pressed && imb.ButtonIndex == (int) ButtonList.Left)
            {
                var type = Enumeration.GetAll<LogicNodeType>().First(x => x.Id == typeId);
                if (_logicNodeChoiceBox != null)
                {
                    var logicNode = _editingLogicNode;
                    logicNode.SetChildAt(new LogicNode(type), _logicNodeChoiceBoxGhostIndex);
                    this.BuildTree(_roots[0]);
                }
                DeleteChoiceBox();
            }
        }

        private void DeleteChoiceBox()
        {
            _swimLane.RemoveChild(_logicNodeChoiceBox);
            _logicNodeChoiceBox.QueueFree();
            _logicNodeChoiceBox = null;
        }

        public void SetCoordinates(Point2D gridCoords)
        {
            
        }
    }
}