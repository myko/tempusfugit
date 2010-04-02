using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RaspberryRoad.TempusFugit
{
    public class Trigger
    {
        protected int triggerCount = 0;
        public List<Action> Actions { get; set; }
        public bool OneTime { get; set; }

        public Trigger()
        {
            Actions = new List<Action>();
        }

        public void Fire()
        {
            if (OneTime && triggerCount > 0)
                return;

            triggerCount++;

            foreach (var action in Actions)
            {
                action();
            }
        }
    }

    public class PositionalTrigger : Trigger
    {
        public Vector2 Position { get; set; }

        public PositionalTrigger()
        {
        }

        public bool IsTriggeredBy(Vector2 objectPosition, float delta)
        {
            return ((objectPosition.X < Position.X) && (objectPosition.X + delta > Position.X)) || ((objectPosition.X > Position.X) && (objectPosition.X + delta < Position.X));
        }
    }
}
