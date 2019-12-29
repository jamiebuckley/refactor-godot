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
        public static class LogicEditorDimensions
        {
            public static int BASE_UNIT = 8;
            public static List<Vector2> LINE_OFFSETS = new List<Vector2>()
            {
                new Vector2(21 * BASE_UNIT, 3 * BASE_UNIT),
                new Vector2(21 * BASE_UNIT, 19 * BASE_UNIT)
            };

            public static List<Vector2> CHILD_OFFSETS = new List<Vector2>()
            {
                new Vector2(21 * BASE_UNIT + 16 * BASE_UNIT, 0),
                new Vector2(21 * BASE_UNIT + 16 * BASE_UNIT, 16 * BASE_UNIT)
            };
        }
        
        private Dictionary<LogicNodeType, PackedScene> LogicNodeTypeToPackedScene =
            new Dictionary<LogicNodeType, PackedScene>();

        private PackedScene Connector;
        private PackedScene GhostNode;
        private PackedScene LogicNodeChoiceBox;
        private PackedScene LogicNodeChoiceBoxChoice;
        
        private Node2D _logicNodeChoiceBox;
        private LogicNode _logicNodeChoiceBoxGhostFor;
        private int _logicNodeChoiceBoxGhostIndex;

        private float scale = 0.5f;
        private Vector2 lastMousePosition;
        private bool isDragging;

        
        private List<LogicNode> roots = new List<LogicNode>();
        
        private Dictionary<LogicNode, Node> GraphicalNodes = new Dictionary<LogicNode, Node>();
        private Dictionary<LogicNode, Dictionary<int, Node>> GhostNodes = new Dictionary<LogicNode, Dictionary<int, Node>>();
        private Dictionary<LogicNode, Dictionary<int, Node>> Lines = new Dictionary<LogicNode, Dictionary<int, Node>>();

        private SwimLane swimLane;

        public override void _Ready()
        {
            var allLogicNodeTypes = Enumeration.GetAll<LogicNodeType>().ToList();
            allLogicNodeTypes.ForEach(lnt =>
            {
                if (lnt.ConnectionsIn.Count > 1)
                {
                    LogicNodeTypeToPackedScene[lnt] =
                        ResourceLoader.Load("res://Scenes/LogicEditor/DoubleLogicNode.tscn") as PackedScene;
                }
                else if (lnt.ConnectionsIn.Count > 0)
                {
                    if (lnt.ConnectionOut == LogicNodeConnection.None)
                    {
                        LogicNodeTypeToPackedScene[lnt] =
                            ResourceLoader.Load("res://Scenes/LogicEditor/StartingLogicNode.tscn") as PackedScene;
                    }
                    else
                    {
                        LogicNodeTypeToPackedScene[lnt] =
                            ResourceLoader.Load("res://Scenes/LogicEditor/SingleLogicNode.tscn") as PackedScene;
                    }
                }
                else
                {
                    LogicNodeTypeToPackedScene[lnt] =
                        ResourceLoader.Load("res://Scenes/LogicEditor/SingleLogicNode.tscn") as PackedScene;
                }
            });

            Connector = ResourceLoader.Load("res://Scenes/LogicEditor/Connector.tscn") as PackedScene;
            GhostNode = ResourceLoader.Load("res://Scenes/LogicEditor/GhostNode.tscn") as PackedScene;
            LogicNodeChoiceBox = ResourceLoader.Load("res://Scenes/LogicEditor/LogicNodeChoiceBox.tscn") as PackedScene;
            LogicNodeChoiceBoxChoice = ResourceLoader.Load("res://Scenes/LogicEditor/LogicNodeChoiceBoxChoice.tscn") as PackedScene;

            SetRoots(new List<LogicNode> {DebugTree()});
            (GetNode("SwimLane1") as SwimLane).SetScaleFloat(scale);
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
            roots = logicNodes;
            roots.ForEach(BuildTree);
        }

        public void BuildTree(LogicNode root)
        {
            if (swimLane == null)
            {
                swimLane = new SwimLane();
                swimLane.Name = "SwimLane1";
                AddChild(swimLane);
            }

            AddNode(swimLane, root, new Vector2());
        }

        public int AddNode(Node2D swimLane, LogicNode item, Vector2 position)
        {
            GraphicalLogicNode node;
            if (!GraphicalNodes.ContainsKey(item))
            {
                node = GetNode(item.LogicNodeType) as GraphicalLogicNode;
                node.OnPressed += OnGraphicalNodePressed;
                
                GraphicalNodes[item] = node;
                swimLane.AddChild(node);
            }
            node = GraphicalNodes[item] as GraphicalLogicNode;
            
            node.SetColor(new Color(item.LogicNodeType.Colour));
            node.SetText(item.LogicNodeType.Name);
            node.Position = position;

            int previousColumnSpacing = 0;
            for (int i = 0; i <= 1; i++)
            {
                if (item.IsConnectionEnabled(i))
                {
                    var multiColumnSpacing = previousColumnSpacing * new Vector2(0, 150);
                    var nodePosition = position + LogicEditorDimensions.CHILD_OFFSETS[i] + multiColumnSpacing;

                    // draw line
                    var line = GetLine(item, i);
                    if (line == null)
                    {
                        line = Connector.Instance() as GraphicalLogicNodeConnector;
                        if (!Lines.ContainsKey(item)) Lines[item] = new Dictionary<int, Node>();
                        Lines[item][i] = line;
                        swimLane.AddChild(line);
                    }
                    line.SetPosition(position + LogicEditorDimensions.LINE_OFFSETS[i]);
                    var target = new Vector2(nodePosition.x, nodePosition.y + LogicEditorDimensions.LINE_OFFSETS[0].y);
                    line.SetLineFromTo(position + LogicEditorDimensions.LINE_OFFSETS[i], target);
                    
                    var ghostNode = GetGhostNode(item, i);
                    if (item.HasChild(i))
                    {
                        if (ghostNode != null)
                        {
                            DeleteGhostNode(item, i);
                        }
                        previousColumnSpacing += AddNode(swimLane, item.ChildAt(i), nodePosition);
                    }
                    else
                    {
                        if (ghostNode == null)
                        {
                            ghostNode = GhostNode.Instance() as GraphicalLogicNode;
                            ghostNode.GhostFor = item;
                            ghostNode.GhostIndex = i;
                            ghostNode.OnPressed += OnGraphicalNodePressed;
                            swimLane.AddChild(ghostNode);
                        }

                        ghostNode.Position = nodePosition;

                        if (!GhostNodes.ContainsKey(item)) GhostNodes[item] = new Dictionary<int, Node>();
                        GhostNodes[item][i] = ghostNode;
                    }
                }
            }

            if (item.IsConnectionEnabled(1)) return previousColumnSpacing + 1;
            return previousColumnSpacing;
        }

        private GraphicalLogicNode GetGhostNode(LogicNode item, int index)
        {
            if (!GhostNodes.ContainsKey(item)) return null;
            if (!GhostNodes[item].ContainsKey(index)) return null;
            return GhostNodes[item][index] as GraphicalLogicNode;
        }

        private GraphicalLogicNodeConnector GetLine(LogicNode item, int index)
        {
            if (!Lines.ContainsKey(item)) return null;
            if (!Lines[item].ContainsKey(index)) return null;
            return Lines[item][index] as GraphicalLogicNodeConnector;
        }

        private void DeleteGhostNode(LogicNode item, int index)
        {
            if (!GhostNodes.ContainsKey(item)) return;
            if (!GhostNodes[item].ContainsKey(index)) return;
            
            var ghostNode = GhostNodes[item][index] as GraphicalLogicNode;
            swimLane.RemoveChild(ghostNode);
            ghostNode.QueueFree();

            var ghostNodeForItem = GhostNodes[item];
            ghostNodeForItem.Remove(index);
            if (!ghostNodeForItem.Any())
            {
                GhostNodes.Remove(item);
            }
        }

        private void OnGraphicalNodePressed(GraphicalLogicNode graphicalLogicNode)
        {
            if (_logicNodeChoiceBox != null)
            {
                swimLane.RemoveChild(_logicNodeChoiceBox);
                _logicNodeChoiceBox.QueueFree();
                _logicNodeChoiceBox = null;
            }

            if (graphicalLogicNode.LogicNode != null)
            {
                
            }
            else if (graphicalLogicNode.GhostFor != null)
            {
                var choiceBox = LogicNodeChoiceBox.Instance() as Node2D;
                choiceBox.Position = graphicalLogicNode.Position + new Vector2(0, 100);
                swimLane.AddChild(choiceBox);
                _logicNodeChoiceBox = choiceBox;
                _logicNodeChoiceBoxGhostFor = graphicalLogicNode.GhostFor;
                _logicNodeChoiceBoxGhostIndex = graphicalLogicNode.GhostIndex;

                var container = choiceBox.GetNode("choiceContainer") as Container;
                container.GetChild(0).Connect("gui_input", this, "CloseLogicChoiceBox");
                var connection = graphicalLogicNode.GhostFor.LogicNodeType.ConnectionsIn[graphicalLogicNode.GhostIndex];
                var matchingLogicNodeTypes = Enumeration.GetAll<LogicNodeType>().ToList().Where(x => x.ConnectionOut == connection).ToList();
                matchingLogicNodeTypes.ForEach(type =>
                {
                    var choice = LogicNodeChoiceBoxChoice.Instance();
                    ((Label) choice.GetNode("label")).Text = type.Name;
                    ((TextureRect) choice.GetNode("texture")).Modulate = new Color(type.Colour);
                    container.AddChild(choice);
                    choice.Connect("gui_input", this, "OnLogicChoiceBoxChoiceInput", new Array { type.Id });
                });
            }
        }

        public Node2D GetNode(LogicNodeType logicNodeType)
        {
            if (!LogicNodeTypeToPackedScene.ContainsKey(logicNodeType))
                throw new ArgumentException($"Couldn't create node for {logicNodeType}");
            var scene = LogicNodeTypeToPackedScene[logicNodeType].Instance();
            return scene as Node2D;
        }

        public override void _Process(float delta)
        {
            if (Input.IsMouseButtonPressed((int) ButtonList.Middle))
            {
                if (!isDragging)
                {
                    lastMousePosition = GetLocalMousePosition();
                }

                isDragging = true;
                var difference = GetLocalMousePosition() - lastMousePosition;
                var swimlane = GetNode("SwimLane1") as SwimLane;
                swimlane.SetPosition(swimlane.GetPosition() + difference);
                lastMousePosition = GetLocalMousePosition();
            }
            else
            {
                isDragging = false;
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
                        scale += 0.02f;
                        (GetNode("SwimLane1") as SwimLane).SetScaleFloat(scale);
                    }
                    else if (iemb.ButtonIndex == (int) ButtonList.WheelDown)
                    {
                        scale -= 0.02f;
                        (GetNode("SwimLane1") as SwimLane).SetScaleFloat(scale);
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
                    swimLane.RemoveChild(_logicNodeChoiceBox);
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
                    var logicNode = _logicNodeChoiceBoxGhostFor;
                    logicNode.SetChildAt(new LogicNode(type), _logicNodeChoiceBoxGhostIndex);
                    this.BuildTree(roots[0]);
                }
                DeleteChoiceBox();
            }
        }

        private void DeleteChoiceBox()
        {
            swimLane.RemoveChild(_logicNodeChoiceBox);
            _logicNodeChoiceBox.QueueFree();
            _logicNodeChoiceBox = null;
        }

        public void SetCoordinates(Point2D gridCoords)
        {
            
        }
    }
}