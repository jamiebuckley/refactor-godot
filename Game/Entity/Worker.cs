using System.Collections.Generic;
using Refactor1.Game.Common;

namespace Refactor1.Game.Entity
{
    public class Worker : GridEntity
    {
        public List<InventoryItem> InventoryItems { get; } = new List<InventoryItem>();
    }
}