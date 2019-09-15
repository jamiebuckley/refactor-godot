using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game
{
    /// <summary>
    /// Handles validation rules and setup logic around building entities on the grid
    /// </summary>
    public class GridBuilder
    {
        private readonly Grid _grid;

        public GridBuilder(Grid grid)
        {
            _grid = grid;
        }

        public void HandleBuild(EntityType entityType, Point2D coordinates, Spatial createdSpatial)
        {
            GameOrientation orientation = GameOrientation.North;
            if (entityType == EntityType.ENTRANCE || entityType == EntityType.EXIT)
            {
                // entrances/exits should face the right way
                orientation = GameOrientation.GetEdgeOrientation(coordinates, 0, 19);
            }

            HandleBuild(entityType, coordinates, createdSpatial, orientation);
        }

        public void HandleBuild(EntityType entityType, Point2D coordinates, Spatial createdSpatial,
            GameOrientation orientation)
        {
            if (entityType == EntityType.WORKER)
            {
                Worker worker = createdSpatial as Worker;
                worker.Destination = coordinates;
                worker.Grid = _grid;
            }
            
            createdSpatial.Rotate(Vector3.Up, orientation.ToRotation());
            var gridEntity = _grid.AddEntity(createdSpatial, entityType, coordinates, orientation);
            GD.Print($"Built {gridEntity.EntityType} at {gridEntity.CurrentGridTile.Position}");
            var t = _grid.GetWorldCoordinates(coordinates);
            createdSpatial.SetTranslation(new Vector3(t.x, 0.05f, t.z));
        }
        
        public bool IsValid(EntityType entityType, Point2D coordinates)
        {
            // does the grid say its valid?
            if (!_grid.CanPlaceEntityType(entityType, coordinates)) return false;
            if ((entityType == EntityType.ENTRANCE || entityType == EntityType.EXIT) && !IsOnEdge(coordinates))
            {
                return false;
            }

            return true;
        }

        private bool IsOnEdge(Point2D coordinates)
        {
            int minAxis = 0, maxAxis = 19;
            var validX = (coordinates.X == minAxis || coordinates.X == maxAxis) && (coordinates.Z > minAxis && coordinates.Z < maxAxis);
            var validZ = (coordinates.Z == minAxis || coordinates.Z == maxAxis) && (coordinates.X > minAxis && coordinates.X < maxAxis);
            if (!validX && !validZ)
            {
                return false;
            }

            return true;
        }
    }
}