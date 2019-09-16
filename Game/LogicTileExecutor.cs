using System;
using System.Collections.Generic;
using System.Linq;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;
using Refactor1.Game.Logic;

namespace Refactor1.Game
{
    public class LogicTileExecutor
    {
        public enum ExecutionResultAction
        {
            EMPTY,
            TOGGLE_IF,
            ON_IF
        }
        
        public class ExecutionResult
        {
            public ExecutionResultAction Action { get; set; }
            
            public bool Result { get; set; }
        }

        public class ExecutionResultCalculation
        {
            private readonly LogicNode _root;
            
            private readonly Grid.GridTile _grid;
            
            private static readonly ExecutionResult NullResult = new ExecutionResult
            {
                Action = ExecutionResultAction.EMPTY,
                Result = false,
            };

            private static readonly Dictionary<LogicNodeType, ExecutionResultAction> ActionLookup =
                new Dictionary<LogicNodeType, ExecutionResultAction>
                {
                    {LogicNodeType.ToggleIf, ExecutionResultAction.TOGGLE_IF},
                    {LogicNodeType.OnIf, ExecutionResultAction.ON_IF}
                };

            public ExecutionResultCalculation(LogicNode root, Grid.GridTile grid)
            {
                _root = root;
                _grid = grid;
            }

            public ExecutionResult GetResult()
            {
                if (!_root.HasChild(0)) return NullResult;
                return Action(_root.Child1);
            }
            
            private ExecutionResult Action(LogicNode node)
            {
                return new ExecutionResult()
                {
                    Action = ActionLookup[node.LogicNodeType],
                    Result = node.HasChild(0) ? Condition(node.Child1) : false
                };
            }

            private bool Condition(LogicNode node)
            {
                if (node.LogicNodeType == LogicNodeType.And && node.HasChild(0) && node.HasChild(1))
                {
                    return Condition(node.Child1) && Condition(node.Child2);
                }
                if (node.LogicNodeType == LogicNodeType.Or && node.HasChild(0) && node.HasChild(1))
                {
                    return Condition(node.Child1) && Condition(node.Child2);
                }
                if (node.LogicNodeType == LogicNodeType.Xor && node.HasChild(0) && node.HasChild(1))
                {
                    return Condition(node.Child1) && Condition(node.Child2);
                }
                if (node.LogicNodeType == LogicNodeType.Not && node.HasChild(0))
                {
                    return !Condition(node.Child1);
                }
                if (node.LogicNodeType == LogicNodeType.WorkerHas && node.HasChild(0) && node.HasChild(1))
                {
                    return NumericalComparison(node.Child1, InventoryItemCount(node.Child2));
                }

                return false;
            }

            private bool NumericalComparison(LogicNode node, int inventoryItemCount)
            {
                if (!node.HasChild(0)) return false;
                int.TryParse(node.Child1.Tags[LogicNode.NumericalValueTag], out int value);
                if (node.LogicNodeType == LogicNodeType.NumericalEquals)
                {
                    return value == inventoryItemCount;
                }
                if (node.LogicNodeType == LogicNodeType.NumericalGreaterThan)
                {
                    return inventoryItemCount > value;
                }
                if (node.LogicNodeType == LogicNodeType.NumericalLessThan)
                {
                    return inventoryItemCount < value;
                }
                return false;
            }

            private int InventoryItemCount(LogicNode node)
            {
                Enum.TryParse(node.Tags[LogicNode.InventoryItemTag], out InventoryItem item);
                var worker = _grid.GridEntities.FirstOrDefault(x => x.EntityType == EntityType.WORKER) as Worker;
                return worker?.InventoryItems.Count(x => x == item) ?? 0;
            }
        }

        public ExecutionResult Execute(LogicNode root, Grid.GridTile grid)
        {
            return new ExecutionResultCalculation(root, grid).GetResult();
        }
    }
}