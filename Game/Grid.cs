using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Refactor1.Game.Common;
using Orientation = Godot.Orientation;

namespace Refactor1.Game
{
    public class Grid
    {
        public class GridEntity
        {
            public EntityType EntityType { get; set; }
            
            public Node GodotEntity { get; set; }
            
            public GridTile CurrentGridTile { get; set; }
            
            public GameOrientation Orientation { get; set; }
        }

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
        
        private readonly Main _main;

        private List<GridTile> _internalGrid;
        
        public Grid(int size, Main main)
        {
            this._size = size;
            _main = main;
            this._internalGrid = new List<GridTile>(size * size);
            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    this._internalGrid.Add(new GridTile(new Point2D(x, z)));
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

            var gridEntity = new GridEntity();
            gridEntity.EntityType = entityType;
            gridEntity.GodotEntity = godotNode;
            gridEntity.Orientation = orientation;
            
            var tile = GetGridTile(position);
            gridEntity.CurrentGridTile = tile;
            tile.GridEntities.Add(gridEntity);
            
            return gridEntity;
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
            return Vector3.Zero;
        }

        public GridTile GetGridTile(Point2D position)
        {
            var gridTile = _internalGrid[position.X * _size + position.Z];
            return gridTile;
        }

        List<GridEntity> FindEntitiesByType(EntityType entityType)
        {
            return _internalGrid.SelectMany(x => x.GridEntities)
                .Where(e => e.EntityType == entityType)
                .ToList();
        }

        public void Step()
        {
            StepWorkerOrientations();
            StepWorkers();
            StepEntrances();
        }

        private void StepWorkerOrientations()
        {
            foreach (var gridTile in _internalGrid)
            {
                var tile = gridTile.GridEntities.FirstOrDefault(e => e.EntityType == EntityType.TILE);
                var worker = gridTile.GridEntities.FirstOrDefault(e => e.EntityType == EntityType.WORKER);
                if (tile == null || worker == null) continue;
                
                worker.Orientation = tile.Orientation;
            }
        }

        private class TempGridTile
        {
            public GridTile RealGridTile { get; set; }
            public List<TempWorker> Entities { get; set; } = new List<TempWorker>();
        }

        private class TempWorker
        {
            public TempGridTile CurrentGridTile { get; set; }
            
            public TempGridTile OldGridTile { get; set; }
            
            public GridEntity RealWorker { get; set; }
        }

        private void StepWorkers()
        {
            var tempGridTiles = new List<TempGridTile>(_internalGrid.Count);
            var tempWorkers = new List<TempWorker>(_internalGrid.Count);
            
            // Put workers in temp workers list
            // Clear workers from grid tile
            foreach (var gridTile in _internalGrid)
            {
                var tempGridTile = new TempGridTile() {RealGridTile = gridTile};
                tempGridTiles.Add(tempGridTile);
                
                var workers = gridTile.GridEntities.Where(e => e.EntityType == EntityType.WORKER)
                    .Select(worker => new TempWorker() { RealWorker = worker, OldGridTile = tempGridTile})
                    .ToList();
                
                gridTile.GridEntities.RemoveAll(e => e.EntityType == EntityType.WORKER);
                tempWorkers.AddRange(workers);
            }

            foreach (var tempWorker in tempWorkers)
            {
                var currentPosition = tempWorker.OldGridTile.RealGridTile.Position;
                var newPosition = currentPosition + tempWorker.RealWorker.Orientation.Direction;

                var workerMovementBlockingEntities = new List<EntityType>() {EntityType.ENTRANCE, EntityType.EXIT};

                var isOutOfBounds = !IsInGridBounds(newPosition);
                var workerBlockedByEntity = !isOutOfBounds && GetGridTile(newPosition).GridEntities
                    .Any(e => workerMovementBlockingEntities.Contains(e.EntityType));

                if (isOutOfBounds || workerBlockedByEntity)
                {
                    tempWorker.CurrentGridTile = tempWorker.OldGridTile;
                    tempWorker.CurrentGridTile.Entities.Add(tempWorker);
                }
                else
                {
                    var newTile = tempGridTiles[newPosition.X * _size + newPosition.Z];
                    tempWorker.CurrentGridTile = newTile;
                    newTile.Entities.Add(tempWorker);
                }
            }

            var invalidStack = new Stack<TempGridTile>();
            foreach (var gridTile in tempGridTiles)
            {
                if (gridTile.Entities.Count > 1) invalidStack.Push(gridTile);
            }


            while (invalidStack.Count > 0)
            {
                var tile = invalidStack.Pop();
                var entityStack = new Stack<TempWorker>(tile.Entities);
                while (entityStack.Count > 1)
                {
                    var topEntity = entityStack.Peek();
                    if (topEntity.CurrentGridTile != topEntity.OldGridTile) entityStack.Pop();
                    topEntity.CurrentGridTile = topEntity.OldGridTile;
                    topEntity.OldGridTile.Entities.Add(topEntity);
                    if (topEntity.OldGridTile.Entities.Count > 1) invalidStack.Push(topEntity.OldGridTile);
                }
                tile.Entities = entityStack.ToList();
            }

            foreach (var tempWorker in tempWorkers)
            {
                var newGridTile = tempWorker.CurrentGridTile.RealGridTile;
                newGridTile.GridEntities.Add(tempWorker.RealWorker);
                tempWorker.RealWorker.CurrentGridTile = newGridTile;

                if (tempWorker.RealWorker.GodotEntity == null) continue;
                var worker = (Worker)tempWorker.RealWorker.GodotEntity;
                worker.Destination = newGridTile.Position;
            }
        }
        
        private void StepEntrances()
        {
            var entrances = FindEntitiesByType(EntityType.ENTRANCE);
            entrances.ForEach(entrance =>
            {
                var orientation = entrance.Orientation;
                var gridTile = entrance.CurrentGridTile;
                if (CanPlaceEntityType(EntityType.WORKER, gridTile.Position))
                {
                    _main.CreateWorker(gridTile.Position, entrance.Orientation);
                }
            });
        }
    }
}