using Refactor1.Game.Logic;

namespace Refactor1.Game
{
    public class LogicTileExecutor
    {
        public enum ExecutionResultAction
        {
            TOGGLE_IF,
            ON_IF
        }
        
        public class ExecutionResult
        {
            public ExecutionResultAction Action { get; set; }
            
            public bool Result { get; set; }
        }
        
        public ExecutionResult Execute(LogicNode root, Grid grid)
        {
            
            return new ExecutionResult()
            {
                Action = ExecutionResultAction.TOGGLE_IF,
                Result = true
            };
        }
    }
}