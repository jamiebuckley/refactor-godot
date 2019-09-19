using System.Linq;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;
using Refactor1.Game.Events;

namespace Refactor1.Game
{
    public class GridExitStepper
    {
        public void Step(Grid grid)
        {
            grid.InternalGrid.ForEach(gridTile =>
            {
                if (gridTile.GridEntities.Any(e => e.EntityType == EntityType.EXIT))
                {
                    var workers = gridTile.GridEntities.Where(e => e.EntityType == EntityType.WORKER);
                    foreach (var gridEntity in workers)
                    {
                        var worker = (Worker) gridEntity;
                        WorkerEvents.Instance.Value.WorkerExited(worker);
                        grid.RemoveEntity(worker);
                    }
                }
            });
        }
    }
}