using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Refactor1.Game;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;
using Refactor1.Game.Events;
using Refactor1.Game.Logic;
using Refactor1.Game.Mechanics;
using Array = Godot.Collections.Array;

namespace Refactor1
{
    public class Main : Spatial, GodotInterface
    {
        private const float TileSize = 1.0f;

        private float _pulseTimer;

        private Spatial _picker;

        private Grid _grid;

        private GridBuilder _gridBuilder;

        private EntityType _selectedEntityType = EntityType.NONE;

        private GridEntity _selectedEntity;

        private Dictionary<EntityType, PackedScene> _entityTypeToPackedScenes = new Dictionary<EntityType, PackedScene>();
        
        private UserInterface _userInterface;

        private List<Goal> _goals = new List<Goal>();

        private Camera _camera;

        private CameraController _cameraController;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _grid = new Grid(20, TileSize, this);
            _gridBuilder = new GridBuilder(_grid);
            _userInterface = GetNode("UI") as UserInterface;
            _camera = GetNode<Camera>("Camera");
            _cameraController = new CameraController(_camera);
            
            ConnectBuildOptionButtons();
            PreparePackedScenes();
            SetupPicker();
            SetupLevel();
            LogicEditorDebug();
        }

        private void ConnectBuildOptionButtons()
        {
            var ui = GetNode(Constants.UIPath);
            foreach (Button button in ui.GetTree().GetNodesInGroup("BuildOptionButton"))
            {
                button.Connect("pressed", this, "OnBuildOptionButtonPress", new Array {button});
            }
        }

        private void PreparePackedScenes()
        {
            foreach (var entityTypeResourcePath in Constants.EntityTypeResourcePaths)
            {
                _entityTypeToPackedScenes[entityTypeResourcePath.Key] =
                    ResourceLoader.Load<PackedScene>(entityTypeResourcePath.Value);
            }
        }

        private void SetupPicker()
        {
            var pickerScene = ResourceLoader.Load<PackedScene>(Constants.PickerSceneResourcePath);
            _picker = (Spatial) pickerScene.Instance();
            AddChild(_picker);
        }

        private void SetupLevel()
        {
            var coalEntity = _entityTypeToPackedScenes[EntityType.COAL].Instance() as Spatial;
            coalEntity.Translation = _grid.GetWorldCoordinates(new Point2D(5, 5));
            AddChild(coalEntity);
            _grid.AddEntity(coalEntity, EntityType.COAL, new Point2D(5, 5),
                GameOrientation.North);

            var goalItem = _userInterface.AddGoal("Collect 5 coal", "Collect 5 coal in 1 day");
            CreateGoal(new Goal(goalItem)
            {
                MakeActive = (goal) =>
                {
                    int current = 0;
                    var subscription = GameEvents.Instance.SubscribeTo(typeof(WorkerExitedEvent), evt =>
                    {
                        WorkerExitedEvent workerExitedEvent = (WorkerExitedEvent) evt;
                        var carryingCoal = workerExitedEvent.Worker.InventoryItems.Where(x => x == InventoryItem.COAL)
                            .Count();
                        current += carryingCoal;
                        
                        goal.GoalItem.SetDescription($"Collect {5 - current} coal in 1 day");
                        if (current >= 5)
                        {
                            goal.Complete();
                        }
                    });
                    goal.Subscriptions.Add(subscription);
                }
            });
        }

        private void CreateGoal(Goal goal)
        {
            _goals.Add(goal);
            goal.MakeActive(goal);
        }
        
        private void LogicEditorDebug()
        {
            //var windowDialog = GetTree().GetRoot().FindNode("LogicDialog", true, false) as WindowDialog;
            //windowDialog.PopupCentered();
            
            var logicEditor = GetTree().GetRoot().FindNode("LogicEditor", true, false) as LogicEditor;
            
            var root = new LogicNode(LogicNodeType.Root, null);
            
            var toggleIf = new LogicNode(LogicNodeType.OnIf, root);
            root.Child1 = toggleIf;

            var workerHas = new LogicNode(LogicNodeType.WorkerHas, toggleIf);
            toggleIf.Child1 = workerHas;
            
            var numericalComparison = new LogicNode(LogicNodeType.NumericalGreaterThan, workerHas);
            workerHas.Child1 = numericalComparison;

            var number = new LogicNode(LogicNodeType.Number, numericalComparison);
            number.Tags[LogicNode.NumericalValueTag] = "2";
            numericalComparison.Child1 = number;

            var inventoryItem = new LogicNode(LogicNodeType.InventoryItem, workerHas);
            inventoryItem.Tags[LogicNode.InventoryItemTag] = InventoryItem.COAL.ToString();
            workerHas.Child2 = inventoryItem;

            var logicEntity = CreateEntityType(EntityType.LOGIC);
            logicEntity.Translation = _grid.GetWorldCoordinates(new Point2D(3, 5));
            AddChild(logicEntity);
            
            var gridLogicEntity = _grid.AddEntity(logicEntity, EntityType.LOGIC, new Point2D(3, 5), GameOrientation.North) as LogicTile;
            gridLogicEntity.Roots.Add(root);
            
            var tileEntity = CreateEntityType(EntityType.TILE);
            tileEntity.Translation = _grid.GetWorldCoordinates(new Point2D(4, 5));
            AddChild(tileEntity);
            
            var tileGridEntity = _grid.AddEntity(tileEntity, EntityType.TILE, new Point2D(4, 5), GameOrientation.North) as LogicTile;
            
            logicEditor.LoadTree(new List<LogicNode>() { root });
        }

        // ReSharper disable once UnusedMember.Global
        public void OnBuildOptionButtonPress(Button button)
        {
            if (!Constants.BuildOptionEntityTypes.ContainsKey(button.GetName()))
            {
                throw new ArgumentException(
                    $"Could not find matching entity type for selected entity {button.GetName()}");
            }
            _selectedEntityType = Constants.BuildOptionEntityTypes[button.GetName()];
            
            var buildModal = (WindowDialog)GetNode(Constants.BuildModalPath);
            buildModal.Hide();

            var buildTextLabel = (Label)GetNode(Constants.CurrentBuildOptionLabel);
            buildTextLabel.SetText(_selectedEntityType.ToString());
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            _cameraController.Process(delta, GetViewport().GetMousePosition());
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
            if (_cameraController.HandleEvent(@event))
            {
                return;
            }
            if (@event is InputEventMouseButton mouseEvent) HandleMouseClick(mouseEvent);
            if (@event is InputEventKey keyEvent) HandleKey(keyEvent);
        }

        private void HandleMouseClick(InputEventMouseButton @event)
        {
            if (@event.IsPressed()) return;
            var coordinates = _grid.GetCoordinates(_picker.Translation);

            if (_selectedEntityType == EntityType.NONE)
                HandleGridSelection(coordinates);
            else
                HandleGridBuild(coordinates);
        }

        private void HandleKey(InputEventKey @event)
        {
            // unset build choice
            if (@event.GetScancode() == (int) KeyList.Escape)
            {
                _selectedEntityType = EntityType.NONE;
                _userInterface.SetOptionLabelText("");
            }

            GD.Print($"event {@event.Scancode} {@event.Pressed}");
            if (_selectedEntity == null) return;
            
            // rotate selected entity
            if (@event.GetScancode() == (int) KeyList.P && !@event.Pressed)
                _selectedEntity.Orientation = GameOrientation.ClockwiseOf(_selectedEntity.Orientation);
            
            if (@event.GetScancode() == (int) KeyList.O && !@event.Pressed)
                _selectedEntity.Orientation = GameOrientation.AntiClockwiseOf(_selectedEntity.Orientation);
            
            ((Spatial)(_selectedEntity.GodotEntity)).SetRotation(new Vector3(0,_selectedEntity.Orientation.ToRotation(), 0));
        }

        private void HandleGridSelection(Point2D gridCoords)
        {
            GD.Print("Refactor1::Main::Selection " + gridCoords);
            if (!_grid.IsInGridBounds(gridCoords)) return;

            var gridTile = _grid.GetGridTile(gridCoords);
            var firstEntity = gridTile.GridEntities.FirstOrDefault(entity => entity.EntityType == EntityType.TILE ||
                                                                             entity.EntityType == EntityType.LOGIC);
            if (firstEntity == null) return;
            
            _selectedEntity?.GodotEntity.Call("_set_unselected");
            _selectedEntity = firstEntity;
            _selectedEntity.GodotEntity.Call("_set_selected");

            // Show LOGIC modal
            if (_selectedEntity.EntityType == EntityType.LOGIC)
            {
                _userInterface.ShowLogicModal();
                var logicEditor = GetTree().GetRoot().FindNode("LogicEditor", true, false) as LogicEditor;
                if (logicEditor == null)
                {
                    throw new ArgumentException("Failed to find logic editor to set coordinates");
                }
                logicEditor.SetCoordinates(gridCoords);
            }
        }

        private Spatial CreateEntityType(EntityType entityType)
        {
            if (!_entityTypeToPackedScenes.ContainsKey(entityType))
            {
                throw new ArgumentException($"No matching packed scene to build for entity type {_selectedEntityType}");
            }
            return _entityTypeToPackedScenes[entityType].Instance() as Spatial;
        }
        
        
        public void CreateWorker(Point2D position, GameOrientation orientation)
        {
            if (!_gridBuilder.IsValid(EntityType.WORKER, position)) return;
            var worker = _entityTypeToPackedScenes[EntityType.WORKER].Instance() as WorkerScene;
            _gridBuilder.HandleBuild(EntityType.WORKER, position, worker, orientation);
            AddChild(worker);
        }

        private void HandleGridBuild(Point2D gridCoords)
        {
            if (_selectedEntityType == EntityType.NONE) return;
            if (!_gridBuilder.IsValid(_selectedEntityType, gridCoords)) return;
            
            var instance = CreateEntityType(_selectedEntityType);
            _gridBuilder.HandleBuild(_selectedEntityType, gridCoords, instance);
            AddChild(instance);
        }

        private void OnTimer()
        {
            _grid.Step(out List<GridWorkerStepper.BlockedWorker> blockedWorkers);
            
            blockedWorkers.ForEach(blockedWorker =>
            {
                var workerEntity = blockedWorker.Worker as Worker;
                var blockedBy = blockedWorker.BlockedBy;

                if (blockedBy.EntityType == EntityType.COAL)
                {
                    GD.Print("Picking up coal");
                    workerEntity.InventoryItems.Add(InventoryItem.COAL);
                    var scene = (WorkerScene) workerEntity.GodotEntity;
                    scene.SpeechBubbleFlash(50);
                }
            });
        }

        private void SetPickerPosition()
        {
            var result = GetWorldMousePosition();
            if (!result.Any() || !result.ContainsKey("position"))
            {
                _picker.SetVisible(false);
                return;
            }
            
            _picker.SetVisible(true);
            var point2D = _grid.GetCoordinates((Vector3)result["position"]);
            var worldCoordinates = _grid.GetWorldCoordinates(point2D);
            _picker.SetTranslation(new Vector3(worldCoordinates.x, 0.05f, worldCoordinates.z));
        }

        private Godot.Collections.Dictionary GetWorldMousePosition()
        {
            var camera = GetNode<Camera>("Camera");
            var rayFrom = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
            var rayTo = rayFrom + camera.ProjectRayNormal(GetViewport().GetMousePosition()) * 200;

            var spaceState = GetWorld().GetDirectSpaceState();
            var result = spaceState.IntersectRay(rayFrom, rayTo, null, 0x7FFFFFFF, true, true);
            return result;
        }

        public void SaveLogicTile(Point2D coordinates, List<LogicNode> roots)
        {
            if (coordinates == null) throw new ArgumentException("Coordinates cannot be null");
            
            var gridTile = _grid.GetGridTile(coordinates);
            var logicTile = gridTile.GridEntities.First(x => x.EntityType == EntityType.LOGIC) as LogicTile;
            logicTile.Roots = roots;
        }
    }
}