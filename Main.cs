using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Refactor1.Game;
using Refactor1.Game.Common;

namespace Refactor1
{
    public class Main : Spatial
    {
        private float _pulseTimer;

        private Spatial _picker;

        private Grid _grid;

        private const float TileSize = 1.0f;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            GD.Print("hi");
            var pickerScene = ResourceLoader.Load<PackedScene>("res://Prototypes/Picker.tscn");
            _picker = (Spatial) pickerScene.Instance();
            AddChild(_picker);
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
        }

        private void HandleMouseClick(InputEventMouseButton @event)
        {
            var result = GetWorldMousePosition();
            if (result.Any())
            {
                Vector3 position = (Vector3) result["position"];
                var gridCoords = GetGridCoordinates(position);
            }
        }

        private void OnTimer()
        {
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
                
                var xToTile = Mathf.Floor(position.x / TileSize) * TileSize  + (TileSize / 2);
                var zToTile = Mathf.Floor(position.z / TileSize) * TileSize  + (TileSize / 2);
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
            return new Point2D (
                Mathf.FloorToInt(offsetWorldX / TileSize), 
                    Mathf.FloorToInt(offsetWorldZ / TileSize)
            );
        }
    }
}