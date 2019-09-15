using Refactor1.Game.Common;

namespace Refactor1.Game
{
    public class GridEntranceStepper
    {
        private readonly GodotInterface _main;
        private readonly Grid _grid;

        public GridEntranceStepper(GodotInterface main, Grid grid)
        {
            _main = main;
            _grid = grid;
        }
        
        public void StepEntrances()
        {
            var entrances = _grid.FindEntitiesByType(EntityType.ENTRANCE);
            entrances.ForEach(entrance =>
            {
                var orientation = entrance.Orientation;
                var gridTile = entrance.CurrentGridTile;
                if (_grid.CanPlaceEntityType(EntityType.WORKER, gridTile.Position))
                {
                    _main.CreateWorker(gridTile.Position, entrance.Orientation);
                }
            });
        }
    }
}