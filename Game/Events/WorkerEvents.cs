using System;
using Refactor1.Game.Entity;

namespace Refactor1.Game.Events
{
    public class WorkerEvents
    {
        public static Lazy<WorkerEvents> Instance = new Lazy<WorkerEvents>(() => new WorkerEvents());
        
        public event EventHandler<Worker> WorkerExitedEvent;

        public void WorkerExited(Worker worker)
        {
            if (WorkerExitedEvent != null)
            {
                WorkerExitedEvent(this, worker);
            }
        }
    }
}