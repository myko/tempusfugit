using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaspberryRoad.TempusFugit
{
    public class Trigger
    {
        private int triggerCount = 0;

        public Position Position { get; set; }
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

        public bool IsTriggeredBy(Position objectPosition, float delta)
        {
            return ((objectPosition.X < Position.X) && (objectPosition.X + delta > Position.X));
        }
    }
}
