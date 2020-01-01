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
        
        private LogicNode _editingLogicNode;
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

        public int AddNode(Node2D swimLane, LogicNode logicNode, Vector2 position)
        {
            GraphicalLogicNode node;
            if (!GraphicalNodes.ContainsKey(logicNode))
            {
                // add the graphical node if one doesn't exist
                node = GetNode(logicNode.LogicNodeType) as GraphicalLogicNode;
                node.OnPressed += OnGraphicalNodePressed;
                
                GraphicalNodes[logicNode] = node;
                swimLane.AddChild(node);
            }
            
            // update the information, e.g. color, text, position
            node = GraphicalNodes[logicNode] as GraphicalLogicNode;
            node.SetColor(new Color(logicNode.LogicNodeType.Colour));
            node.SetText(logicNode.LogicNodeType.Name);
            node.Position = position;

            var previousColumnSpacing = 0;
            // for each potential child
            for (var i = 0; i <= 1; i++)
            {
                if (!logicNode.IsConnectionEnabled(i)) continue;
                
                // calculate the position of this node
                var multiColumnSpacing = previousColumnSpacing * new Vector2(0, 150);
                var nodePosition = position + LogicEditorDimensions.CHILD_OFFSETS[i] + multiColumnSpacing;

                // draw line to the child/ghost
                var line = GetLine(logicNode, i);
                if (line == null)
                {
                    line = Connector.Instance() as GraphicalLogicNodeConnector;
                    if (!Lines.ContainsKey(logicNode)) Lines[logicNode] = new Dictionary<int, Node>();
                    Lines[logicNode][i] = line;
                    swimLane.AddChild(line);
                }
                
                // set the line's position and target
                line.SetPosition(position + LogicEditorDimensions.LINE_OFFSETS[i]);
                var target = new Vector2(nodePosition.x, nodePosition.y + LogicEditorDimensions.LINE_OFFSETS[0].y);
                line.SetLineFromTo(position + LogicEditorDimensions.LINE_OFFSETS[i], target);
                    
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
                        ghostNode = GhostNode.Instance() as GraphicalLogicNode;
                        ghostNode.GhostInformation = new GhostInformation() {ChildIndex = i, LogicNode = logicNode};
                        ghostNode.OnPressed += OnGraphicalNodePressed;
                        swimLane.AddChild(ghostNode);
                    }

                    ghostNode.Position = nodePosition;

                    if (!GhostNodes.ContainsKey(logicNode)) GhostNodes[logicNode] = new Dictionary<int, Node>();
                    GhostNodes[logicNode][i] = ghostNode;
                }
            }

            return logicNode.IsConnectionEnabled(1) ? previousColumnSpacing + 1 : previousColumnSpacing;
        }

        private GraphicalLogicNode GetGhostNodeIfExists(LogicNode item, int index)
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
            else if (graphicalLogicNode.GhostInformation != null)
            {
                // create a choice box
                var choiceBox = LogicNodeChoiceBox.Instance() as Node2D;
                choiceBox.Position = graphicalLogicNode.Position + new Vector2(0, 100);
                swimLane.AddChild(choiceBox);
                
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
                    var logicNode = _editingLogicNode;
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