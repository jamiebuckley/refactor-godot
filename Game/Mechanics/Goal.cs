using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Refactor1.Game.Common;
using Refactor1.Game.Entity;
using Refactor1.Game.Events;

namespace Refactor1.Game.Mechanics
{
    public class Goal : IDisposable
    {
        public GoalItem GoalItem { get; }

        public List<Action> Subscriptions { get; } = new List<Action>();

        public Goal(GoalItem goalItem)
        {
            GoalItem = goalItem;
        }

        public Action<Goal> MakeActive;

        public void Complete()
        {
            GameEvents.Instance.Emit(new GoalCompletedEvent {Goal = this});
        }

        public void Dispose()
        {
            Subscriptions.ForEach(a => a());
        }
    }
}