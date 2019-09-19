using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;
using Orientation = Godot.Orientation;

namespace Refactor1.Game
{
    public class Grid
    {

        public class GridTile
        {
            public Point2D Position { get; set; }
            public List<GridEntity> GridEntities { get; } = new List<GridEntity>();
            
            public GridTile(Point2D position)
            {
                Position = position;
            }
        }

        private readonly int _size;

        private readonly float _tileSize;
        
        private readonly GodotInterface _main;

        public List<GridTile> InternalGrid { get; }
        
        private GridWorkerStepper _gridWorkerStepper;

        private GridLogicStepper _gridLogicStepper;

        private GridEntranceStepper _gridEntranceStepper;

        private GridExitStepper _gridExitStepper;

        public Grid(int size, float tileSize, GodotInterface main)
        {
            _size = size;
            _tileSize = tileSize;
            _main = main;
            InternalGrid = new List<GridTile>(size * size);
            _gridWorkerStepper = new GridWorkerStepper(this);
            _gridLogicStepper = new GridLogicStepper(this);
            _gridEntranceStepper = new GridEntranceStepper(main, this);
            _gridExitStepper = new GridExitStepper(main, this);
            
            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    InternalGrid.Add(new GridTile(new Point2D(x, z)));
                }
            }
        }


        public int GetSize()
        {
            return this._size;
        }
 
        public GridEntity AddEntity(Node godotNode, EntityType entityType, Point2D position, GameOrientation orientation)
        {
            if (!CanPlaceEntityType(entityType, position))
            {
                throw new ArgumentException($"Cannot place entityType {entityType} at position ({position.X}, {position.Z})");
            }

            GridEntity gridEntity;
            if (entityType == EntityType.LOGIC)
            {
                gridEntity = new LogicTile();
            }
            else if (entityType == EntityType.TILE)
            {
                gridEntity = new ArrowTile();
            }
            else if (entityType == EntityType.WORKER)
            {
                gridEntity = new Worker();
            }
            else
            {
                gridEntity = new GridEntity();
            }
            
            gridEntity.EntityType = entityType;
            gridEntity.GodotEntity = godotNode;
            gridEntity.Orientation = orientation;
            
            var tile = GetGridTile(position);
            gridEntity.CurrentGridTile = tile;
            tile.GridEntities.Add(gridEntity);
            
            return gridEntity;
        }

        public void RemoveEntity(GridEntity entity)
        {
            entity.CurrentGridTile.GridEntities.Remove(entity);
            entity.GodotEntity.GetParent().RemoveChild(entity.GodotEntity);
        }

        public bool CanPlaceEntityType(EntityType entityType, Point2D position)
        {
            if (!IsInGridBounds(position)) return false;
            
            var gridTile = GetGridTile(position);
            switch (entityType)
            {
                case EntityType.ENTRANCE:
                case EntityType.EXIT:
                    return !gridTile.GridEntities.Any();
                case EntityType.TILE:
                case EntityType.LOGIC:
                {
                    var blocksTiles = new List<EntityType>
                        {EntityType.TILE, EntityType.LOGIC, EntityType.EXIT, EntityType.ENTRANCE};
                    return !gridTile.GridEntities.Any(e => blocksTiles.Contains(e.EntityType));
                }

                case EntityType.WORKER:
                {
                    var blocksWorker = new List<EntityType>{ EntityType.EXIT, EntityType.WORKER };
                    return !gridTile.GridEntities.Any(e => blocksWorker.Contains(e.EntityType));
                }

                case EntityType.COAL:
                {
                    // todo: This needs to have at least some checking
                    return true;
                }

                default:
                    return false;
            }
        }

        public bool IsInGridBounds(Point2D position)
        {
            return !(position.X < 0 || position.X >= _size || position.Z < 0 || position.Z >= _size);
        }

        public Vector3 GetWorldCoordinates(Point2D position)
        {
            return new Vector3((position.X * _tileSize) - (GetSize() * _tileSize) / 2 + (_tileSize / 2),
                0.05f,
                (position.Z * _tileSize) - (GetSize() * _tileSize) / 2 + (_tileSize / 2));
        }

        public GridTile GetGridTile(Point2D position)
        {
            var gridTile = InternalGrid[position.X * _size + position.Z];
            return gridTile;
        }

        public List<GridEntity> FindEntitiesByType(EntityType entityType)
        {
            return InternalGrid.SelectMany(x => x.GridEntities)
                .Where(e => e.EntityType == entityType)
                .ToList();
        }

        public void Step(out List<GridWorkerStepper.BlockedWorker> blockedWorkers)
        {
            _gridLogicStepper.StepLogic();
            _gridWorkerStepper.StepWorkerOrientations();
             blockedWorkers = _gridWorkerStepper.StepWorkers();
            _gridEntranceStepper.StepEntrances();
            _gridExitStepper.Step();
        }

        public List<GridEntity> GetSurroundingEntities(Point2D gridCoords)
        {
            List<GridEntity> results = new List<GridEntity>();
            if (gridCoords.X != _size) results.AddRange(GetGridTile(new Point2D(gridCoords.X + 1, gridCoords.Z)).GridEntities);
            if (gridCoords.X != 0) results.AddRange(GetGridTile(new Point2D(gridCoords.X - 1, gridCoords.Z)).GridEntities);
            if (gridCoords.Z != _size) results.AddRange(GetGridTile(new Point2D(gridCoords.X, gridCoords.Z + 1)).GridEntities);
            if (gridCoords.Z != 0) results.AddRange(GetGridTile(new Point2D(gridCoords.X, gridCoords.Z - 1)).GridEntities);
            return results;
        }

        public Point2D GetCoordinates(Vector3 position)
        {
            var offsetWorldX = position.x + (GetSize() * _tileSize) / 2;
            var offsetWorldZ = position.z + (GetSize() * _tileSize) / 2;
            return new Point2D(
                Mathf.FloorToInt(offsetWorldX / _tileSize),
                Mathf.FloorToInt(offsetWorldZ / _tileSize)
            );
        }
    }
}