using System.Collections.Generic;
using Godot;
using Refactor1.Game.Common;

namespace Refactor1
{
    public class Constants
    {
        public static Dictionary<EntityType, string> EntityTypeResourcePaths = new Dictionary<EntityType, string>()
        {
            {EntityType.ENTRANCE, "res://Prototypes/Entrance.tscn" },
            {EntityType.EXIT, "res://Prototypes/Exit.tscn" },
            {EntityType.TILE, "res://Prototypes/DirectionalTile.tscn" },
            {EntityType.WORKER, "res://Prototypes/Worker.tscn" },
            {EntityType.COAL, "res://Prototypes/Coal.tscn" },
            {EntityType.LOGIC, "res://Prototypes/LogicTile.tscn" },
        };
        
        public static Dictionary<string, EntityType> BuildOptionEntityTypes = new Dictionary<string, EntityType>()
        {
            {"BOptDirectionalTileButton", EntityType.TILE},
            {"BOptEntranceButton", EntityType.ENTRANCE},
            {"BOptExitButton", EntityType.EXIT},
            {"BOptLogicButton", EntityType.LOGIC},
        };

        public static string PickerSceneResourcePath = "res://Prototypes/Picker.tscn";
        
        public static string UIPath = "/root/RootSpatial/UI";

        public static string BuildModalPath = "/root/RootSpatial/UI/WindowDialog";

        public static string CurrentBuildOptionLabel =
            "/root/RootSpatial/UI/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/BuildLabel";
    }
}