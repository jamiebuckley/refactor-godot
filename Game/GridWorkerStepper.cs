using System.Collections.Generic;
using System.Linq;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;

namespace Refactor1.Game
{
    public class GridWorkerStepper
    {
        private readonly Grid _grid;

        public GridWorkerStepper(Grid grid)
        {
            _grid = grid;
        }
        
        public void StepWorkerOrientations()
        {
            foreach (var gridTile in _grid.InternalGrid)
            {
                var tile = gridTile.GridEntities.FirstOrDefault(e => e.EntityType == EntityType.TILE) as ArrowTile;
                var worker = gridTile.GridEntities.FirstOrDefault(e => e.EntityType == EntityType.WORKER);
                if (tile == null || worker == null || tile.Enabled == false) continue;
                
                worker.Orientation = tile.Orientation;
            }
        }
        
        public class BlockedWorker
        {
            public GridEntity Worker { get; set; }
            
            public GridEntity BlockedBy { get; set; }
        }
        
        private class TempGridTile
        {
            public Grid.GridTile RealGridTile { get; set; }
            public List<TempWorker> Entities { get; set; } = new List<TempWorker>();
        }

        private class TempWorker
        {
            public TempGridTile CurrentGridTile { get; set; }
            
            public TempGridTile OldGridTile { get; set; }
            
            public GridEntity RealWorker { get; set; }
        }
        
        public List<BlockedWorker> StepWorkers()
        {
            List<BlockedWorker> blockedWorkers = new List<BlockedWorker>();
            var tempGridTiles = new List<TempGridTile>(_grid.InternalGrid.Count);
            var tempWorkers = new List<TempWorker>(_grid.InternalGrid.Count);
            
            // Put workers in temp workers list
            // Clear workers from grid tile
            foreach (var gridTile in _grid.InternalGrid)
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

                var workerMovementBlockingEntities = new List<EntityType>() {EntityType.ENTRANCE, EntityType.EXIT, EntityType.COAL};

                var isOutOfBounds = !_grid.IsInGridBounds(newPosition);
                var blockingEntity = isOutOfBounds
                    ? null
                    : _grid.GetGridTile(newPosition).GridEntities
                        .FirstOrDefault(e => workerMovementBlockingEntities.Contains(e.EntityType));

                if (blockingEntity != null)
                {
                    blockedWorkers.Add(new BlockedWorker { Worker = tempWorker.RealWorker, BlockedBy = blockingEntity });
                }
                
                var workerBlockedByEntity = !isOutOfBounds && blockingEntity != null;
                if (isOutOfBounds || workerBlockedByEntity)
                {
                    tempWorker.CurrentGridTile = tempWorker.OldGridTile;
                    tempWorker.CurrentGridTile.Entities.Add(tempWorker);
                }
                else
                {
                    var newTile = tempGridTiles[newPosition.X * _grid.GetSize() + newPosition.Z];
                    tempWorker.CurrentGridTile = newTile;
                    newTile.Entities.Add(tempWorker);
                }
            }

            var invalidStack = new Stack<TempGridTile>();
            foreach (var gridTile in tempGridTiles)
            {
                if (gridTile.Entities.Count > 1)
                {
                    invalidStack.Push(gridTile);
                }

                foreach (var entity in gridTile.Entities.ToList())
                {
                    if (entity.OldGridTile != entity.CurrentGridTile)
                    {
                        var workerFromThisTile =
                            entity.OldGridTile.Entities.FirstOrDefault(x => x.OldGridTile == entity.CurrentGridTile);
                        
                        if (workerFromThisTile != null)
                        {
                            workerFromThisTile.OldGridTile.Entities.Remove(workerFromThisTile);
                            workerFromThisTile.CurrentGridTile = gridTile;
                            gridTile.Entities.Add(workerFromThisTile);

                            gridTile.Entities.Remove(entity);
                            entity.CurrentGridTile = entity.OldGridTile;
                            entity.CurrentGridTile.Entities.Add(entity);
                            if (entity.CurrentGridTile.Entities.Count() > 1 && !invalidStack.Contains(entity.CurrentGridTile)) 
                                invalidStack.Push(entity.CurrentGridTile);
                        }
                    }
                }
            }


            while (invalidStack.Count > 0)
            {
                var tile = invalidStack.Pop();
                var entityQueue = new Queue<TempWorker>(tile.Entities);
                while (entityQueue.Count > 1)
                {
                    // remove entities down to 1, not moving the entity that came from this tile if it exists
                    
                    var topEntity = entityQueue.Dequeue();
                    if (topEntity.CurrentGridTile == topEntity.OldGridTile)
                    {
                        continue;
                    }
                    topEntity.CurrentGridTile = topEntity.OldGridTile;
                    topEntity.OldGridTile.Entities.Add(topEntity);
                    if (topEntity.OldGridTile.Entities.Count > 1) invalidStack.Push(topEntity.OldGridTile);
                }
                tile.Entities = entityQueue.ToList();
            }

            foreach (var tempWorker in tempWorkers)
            {
                var newGridTile = tempWorker.CurrentGridTile.RealGridTile;
                newGridTile.GridEntities.Add(tempWorker.RealWorker);
                tempWorker.RealWorker.CurrentGridTile = newGridTile;

                if (tempWorker.RealWorker.GodotEntity == null) continue;
                var worker = (WorkerScene)tempWorker.RealWorker.GodotEntity;
                worker.Destination = newGridTile.Position;
            }

            return blockedWorkers;
        }
    }
}