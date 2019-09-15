using System.Linq;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;

namespace Refactor1.Game
{
    public class GridLogicStepper
    {
        private readonly Grid _grid;

        public GridLogicStepper(Grid grid)
        {
            _grid = grid;
        }
        
        public void StepLogic()
        {
            foreach (var gridTile in _grid.InternalGrid)
            {
                var logicTile = gridTile.GridEntities.FirstOrDefault(ge => ge.EntityType == EntityType.LOGIC);
                if (logicTile == null) continue;
                
                var logicTileCast = logicTile as LogicTile;
                if (!logicTileCast.Roots.Any()) continue;
                var executionResult = new LogicTileExecutor().Execute(logicTileCast.Roots[0], gridTile);
                if (executionResult.Result == false) continue;

                var surroundingEntities = _grid.GetSurroundingEntities(gridTile.Position);
                var arrowTiles = surroundingEntities.Where(x => x.EntityType == EntityType.TILE).Select(e => e as ArrowTile);
                foreach (var tile in arrowTiles)
                {
                    tile.Enabled = !tile.Enabled;
                    if (tile.Enabled)
                        tile.GodotEntity.Call("_set_enabled");
                    else
                        tile.GodotEntity.Call("_set_disabled");
                }
            }
        }
    }
}