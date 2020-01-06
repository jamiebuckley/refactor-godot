using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Refactor1.Game.Logic
{
    public class LogicEditorPackedScenes
    {
        public Dictionary<LogicNodeType, PackedScene> LogicNodeTypeToPackedScene { get; } =
            new Dictionary<LogicNodeType, PackedScene>();

        public PackedScene ConnectorPackedScene { get; }
        public PackedScene GhostNodePackedScene { get; }
        public PackedScene LogicNodeChoiceBoxPackedScene { get; }
        public PackedScene LogicNodeChoiceBoxChoicePackedScene { get; }

        public LogicEditorPackedScenes()
        {
            // for each type of logic node
            // create a map to the appropriate graphical node scene
            var getSceneForType = new Func<LogicNodeType, PackedScene>(type =>
                ResourceLoader.Load(GetSceneNameForLogicNodeType(type)) as PackedScene);
            LogicNodeTypeToPackedScene = Enumeration.GetAll<LogicNodeType>().ToList().ToDictionary(x => x, getSceneForType);
            
            ConnectorPackedScene = ResourceLoader.Load("res://Scenes/LogicEditor/Connector.tscn") as PackedScene;
            GhostNodePackedScene = ResourceLoader.Load("res://Scenes/LogicEditor/GhostNode.tscn") as PackedScene;
            LogicNodeChoiceBoxPackedScene = ResourceLoader.Load("res://Scenes/LogicEditor/LogicNodeChoiceBox.tscn") as PackedScene;
            LogicNodeChoiceBoxChoicePackedScene = ResourceLoader.Load("res://Scenes/LogicEditor/LogicNodeChoiceBoxChoice.tscn") as PackedScene;
        }
        
        private String GetSceneNameForLogicNodeType(LogicNodeType logicNodeType)
        {
            if (logicNodeType.ConnectionsIn.Count > 1) return "res://Scenes/LogicEditor/DoubleLogicNode.tscn";
            if (logicNodeType.ConnectionOut == LogicNodeConnection.None)
                return "res://Scenes/LogicEditor/StartingLogicNode.tscn";
            return "res://Scenes/LogicEditor/SingleLogicNode.tscn";
        }
    }
}