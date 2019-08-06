using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Refactor1.Game;
using Refactor1.Game.Common;
using Array = Godot.Collections.Array;
using Orientation = Godot.Orientation;

namespace Refactor1
{
    public class Main : Spatial
    {
        private const float TileSize = 1.0f;

        private float _pulseTimer;

        private Spatial _picker;

        private Grid _grid;

        private EntityType _selectedEntityType = EntityType.NONE;

        private Grid.GridEntity _selectedEntity;

        private Dictionary<EntityType, PackedScene> _entityTypeToPackedScenes;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _grid = new Grid(20, this);
            GD.Print("Refactor::Main::Ready");

            var buttons = GetNode("/root/RootSpatial/UI").GetTree().GetNodesInGroup("BuildOptionButton");
            foreach (Button button in buttons)
            {
                button.Connect("pressed", this, "OnBuildOptionButtonPress", new Array {button});
            }
            
            
            // Load packed scenes
            _entityTypeToPackedScenes = new Dictionary<EntityType, PackedScene>()
            {
                {EntityType.ENTRANCE, ResourceLoader.Load<PackedScene>("res://Prototypes/Entrance.tscn") },
                {EntityType.EXIT, ResourceLoader.Load<PackedScene>("res://Prototypes/Exit.tscn") },
                {EntityType.TILE, ResourceLoader.Load<PackedScene>("res://Prototypes/DirectionalTile.tscn") },
                {EntityType.WORKER, ResourceLoader.Load<PackedScene>("res://Prototypes/Worker.tscn") },
                {EntityType.LOGIC, ResourceLoader.Load<PackedScene>("res://Prototypes/LogicTile.tscn") },
            };
            
            var pickerScene = ResourceLoader.Load<PackedScene>("res://Prototypes/Picker.tscn");
            _picker = (Spatial) pickerScene.Instance();
            AddChild(_picker);
        }

        public void OnBuildOptionButtonPress(Button button)
        {
            var lookupMap = new Dictionary<String, EntityType>()
            {
                {"BOptDirectionalTileButton", EntityType.TILE},
                {"BOptEntranceButton", EntityType.ENTRANCE},
                {"BOptExitButton", EntityType.EXIT},
                {"BOptLogicButton", EntityType.LOGIC},
            };
            if (!lookupMap.ContainsKey(button.GetName()))
            {
                GD.Print("Could not find matching entity type for selected entity " + button.GetName());
                return;
            }

            _selectedEntityType = lookupMap[button.GetName()];
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            SetPickerPosition();
            _pulseTimer += delta;
            if (_pulseTimer > 1.0f)
            {
                OnTimer();
                _pulseTimer = Mathf.PosMod(_pulseTimer, 1.0f);
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.GetType() == typeof(InputEventMouseButton))
            {
                HandleMouseClick(@event as InputEventMouseButton);
            }

            if (@event.GetType() == typeof(InputEventKey))
            {
                HandleKey(@event as InputEventKey);
            }
        }

        private void HandleMouseClick(InputEventMouseButton @event)
        {
            var result = GetWorldMousePosition();
            if (!result.Any()) return;

            var position = (Vector3) result["position"];
            var gridCoords = GetGridCoordinates(position);

            if (_selectedEntityType == EntityType.NONE)
            {
                HandleGridSelection(gridCoords);
            }
            else
            {
                HandleGridBuild(gridCoords);
            }
        }

        private void HandleKey(InputEventKey @event)
        {
            if (@event.GetScancode() == (int) KeyList.Escape)
            {
                _selectedEntityType = EntityType.NONE;
                var optionLabel = GetNode("UI").Get("option_label") as Label;
                optionLabel.Text = "";
            }

            if (_selectedEntity == null) return;

            if (@event.GetScancode() == (int) KeyList.P && @event.IsPressed())
                _selectedEntity.Orientation = GameOrientation.ClockwiseOf(_selectedEntity.Orientation);
            
            if (@event.GetScancode() == (int) KeyList.O && @event.IsPressed())
                _selectedEntity.Orientation = GameOrientation.AntiClockwiseOf(_selectedEntity.Orientation);
        }

        private void HandleGridSelection(Point2D gridCoords)
        {
            GD.Print("Refactor1::Main::Selection");
            if (!_grid.IsInGridBounds(gridCoords)) return;

            var gridTile = _grid.GetGridTile(gridCoords);
            var firstEntity = gridTile.GridEntities.FirstOrDefault(entity => entity.EntityType == EntityType.TILE ||
                                                                             entity.EntityType == EntityType.LOGIC);
            if (firstEntity == null) return;
            
            _selectedEntity?.GodotEntity.Call("_set_unselected");
            _selectedEntity = firstEntity;
            _selectedEntity.GodotEntity.Call("_set_selected");
            
            if (_selectedEntity.EntityType == EntityType.LOGIC)
                GetNode("/root/RootSpatial/UI").Call("show_logic_modal");
        }

        private void HandleGridBuild(Point2D gridCoords)
        {
            if (_selectedEntityType == EntityType.NONE) return;
            if (!_grid.CanPlaceEntityType(_selectedEntityType, gridCoords)) return;
            
            int minAxis = 0, maxAxis = 19;
            if (_selectedEntityType == EntityType.ENTRANCE || _selectedEntityType == EntityType.EXIT)
            {
                var validX = (gridCoords.X == minAxis || gridCoords.X == maxAxis) && (gridCoords.Z > minAxis && gridCoords.Z < maxAxis);
                var validZ = (gridCoords.Z == minAxis || gridCoords.Z == maxAxis) && (gridCoords.X > minAxis && gridCoords.X < maxAxis);
                if (!validX && !validZ) {
                    // entrance/exit position is not on edge
                    return;
                }
            }

            if (!_entityTypeToPackedScenes.ContainsKey(_selectedEntityType))
            {
                GD.Print("No matching packed scene to build for entity type " + _selectedEntityType.ToString());
                return;
            }

            var packedScene = _entityTypeToPackedScenes[_selectedEntityType];
            var instance = packedScene.Instance() as Spatial;

            if (_selectedEntityType == EntityType.ENTRANCE || _selectedEntityType == EntityType.EXIT)
            {
                var orientation = GetEdgeOrientation(gridCoords, 0, 19);
                instance.Rotate(Vector3.Up, orientation.ToRotation());
                _grid.AddEntity(instance, _selectedEntityType, gridCoords, orientation);
            }
            instance.SetTranslation(_picker.GetTranslation());
            AddChild(instance);

        }

        private GameOrientation GetEdgeOrientation(Point2D gridCoords, int minVal, int maxVal)
        {
            if (gridCoords.X == minVal) return GameOrientation.East;
            else if (gridCoords.X == maxVal) return GameOrientation.West;
            else if (gridCoords.Z == minVal) return GameOrientation.North;
            else if (gridCoords.Z == maxVal) return GameOrientation.South;
            return null;
        }

        private void OnTimer()
        {
            _grid.Step();
            // Step Entrances
            // Step Grid
            // Step Worker Scenes
        }

        private void SetPickerPosition()
        {
            var result = GetWorldMousePosition();
            if (result.Any() && result.ContainsKey("position"))
            {
                _picker.SetVisible(true);
                Vector3 position = (Vector3) result["position"];

                var xToTile = Mathf.Floor(position.x / TileSize) * TileSize + (TileSize / 2);
                var zToTile = Mathf.Floor(position.z / TileSize) * TileSize + (TileSize / 2);
                _picker.SetTranslation(new Vector3(xToTile, position.y + 0.05f, zToTile));
            }
        }

        private Dictionary GetWorldMousePosition()
        {
            var camera = GetNode<Camera>("Camera");
            var rayFrom = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
            var rayTo = rayFrom + camera.ProjectRayNormal(GetViewport().GetMousePosition()) * 200;

            var spaceState = GetWorld().GetDirectSpaceState();
            var result = spaceState.IntersectRay(rayFrom, rayTo, null, 0x7FFFFFFF, true, true);
            return result;
        }

        private Point2D GetGridCoordinates(Vector3 position)
        {
            var offsetWorldX = position.x + (_grid.GetSize() * TileSize) / 2;
            var offsetWorldZ = position.z + (_grid.GetSize() * TileSize) / 2;
            return new Point2D(
                Mathf.FloorToInt(offsetWorldX / TileSize),
                Mathf.FloorToInt(offsetWorldZ / TileSize)
            );
        }

        public Vector3 GetWorldCoordinates(Point2D position)
        {
            return new Vector3((position.X * TileSize) - (_grid.GetSize() * TileSize) / 2 + (TileSize / 2),
                0.0f,
                (position.Z * TileSize) - (_grid.GetSize() * TileSize) / 2 + (TileSize / 2));
        }

        public void CreateWorker(Point2D position, GameOrientation orientation)
        {
            if (!_grid.CanPlaceEntityType(EntityType.WORKER, position)) return;
            var worker = _entityTypeToPackedScenes[EntityType.WORKER].Instance() as Worker;
            var worldCoordinates = GetWorldCoordinates(position);
            worker.SetTranslation(worldCoordinates);
            
            _grid.AddEntity(worker, EntityType.WORKER, position, orientation);
            worker.Game = this;
            worker.Destination = position;
            AddChild(worker);
        }
    }
}