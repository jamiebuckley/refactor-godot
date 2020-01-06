using Refactor1.Game.Entity;
using Refactor1.Game.Mechanics;

namespace Refactor1.Game.Events
{
    public abstract class GameEvent
    {
        
    }

    public class WorkerExitedEvent : GameEvent
    {
        public Worker Worker { get; set; }
    }
    

    public class GoalCompletedEvent : GameEvent
    {
        public Goal Goal { get; set; }
    }
}