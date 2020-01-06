using System;
using System.Linq;
using Refactor1.Game.Common;
using Refactor1.Game.Events;

namespace Refactor1.Game.Mechanics
{
    public class SellItemAmountGoal
    {
        private readonly InventoryItem _inventoryItem;
        private readonly int _amount;
        private int currentlySold = 0;

        public SellItemAmountGoal(InventoryItem inventoryItem, int amount)
        {
            _inventoryItem = inventoryItem;
            _amount = amount;
        }
        
        private bool active;

        public void MakeActive()
        {
            this.active = true;
            GameEvents.Instance.SubscribeTo(typeof(WorkerExitedEvent), OnWorkerExited);
        }

        public bool IsComplete()
        {
            return currentlySold >= _amount;
        }

        public String GetDescription()
        {
            return $"Sell {_amount} {_inventoryItem}s";
        }

        private void OnWorkerExited(GameEvent gameEvent)
        {
            if (!active) return;
            if (gameEvent is WorkerExitedEvent workerExitedEvent)
            {
                var matchingItems = workerExitedEvent.Worker.InventoryItems.Where(i => i == _inventoryItem).Count();
                currentlySold += matchingItems;
            }
        }
    }
}