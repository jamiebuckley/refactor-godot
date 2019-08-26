using System.Collections.Generic;
using Refactor1.Game.Logic;

namespace Refactor1.Game.Entity
{
    public class LogicTile : GridEntity
    {
        public List<LogicNode> Roots { get; set; }
    }
}