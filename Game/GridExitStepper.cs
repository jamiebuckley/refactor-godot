using System.Linq;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;
using Refactor1.Game.Events;

namespace Refactor1.Game
{
    public class GridExitStepper
    {
        private readonly GodotInterface _main;
        
        private readonly Grid _grid;

        public GridExitStepper(GodotInterface main, Grid grid)
        {
            _main = main;
            _grid = grid;
        }

        public void Step()
        {
            _grid.InternalGrid.ForEach(gridTile =>
            {
                if (gridTile.GridEntities.Any(e => e.EntityType == EntityType.EXIT))
                {
                    var workers = gridTile.GridEntities.Where(e => e.EntityType == EntityType.WORKER).ToList();
                    foreach (var gridEntity in workers)
                    {
                        var worker = (Worker) gridEntity;
                        GameEvents.Instance.Emit(new WorkerExitedEvent { Worker = worker });
                        _grid.RemoveEntity(worker);
                    }
                }
            });
        }
    }
}