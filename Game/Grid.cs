using System;
using System.Collections.Generic;
using Godot;
using Refactor1.Game.Common;
using Orientation = Godot.Orientation;

namespace Refactor1.Game
{
    public class Grid
    {
        class GridEntity
        {
            private EntityType EntityType { get; set; }
            
            private Node GodotEntity { get; set; }
            
            private GridTile CurrentGridTile { get; set; }
            
            private Orientation Orientation { get; set; }
        }
        
        class GridTile
        {
            private Point2D Position { get; set; }
            private List<GridEntity> GridEntities { get; } = new List<GridEntity>();
        }

        public int GetSize()
        {
            throw new NotImplementedException();
        }
 
        GridEntity AddEntity(Node godotNode, EntityType entityType, Point2D position, Orientation orientation)
        {
            throw new NotImplementedException();
        }

        bool CanPlaceEntityType(EntityType entityType, Point2D position)
        {
            throw new NotImplementedException();
        }

        bool IsInGridBounds(Point2D position)
        {
            throw new NotImplementedException();
        }

        Vector3 GetWorldCoordinates(Point2D position)
        {
            throw new NotImplementedException();
        }

        GridTile GetGridTile(Point2D position)
        {
            throw new NotImplementedException();
        }

        void Step()
        {
            throw new NotImplementedException();
        }
    }
}