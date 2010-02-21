using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaspberryRoad.TempusFugit
{
    public class Position
    {
        public float X { get; set; }
    }

    public class Player
    {
        public Position Position { get; set; }
        public bool Exists { get; set; }

        public Player()
        {
            Position = new Position();
        }
    }

    public class PresentPlayer : Player
    {
        public void Move(float delta, Door door1, Door door2, FuturePlayer future, Trigger toggleDoorsTrigger, Trigger spawnFuturePlayerTrigger, Trigger timeTravelTrigger)
        {
            if (spawnFuturePlayerTrigger.IsTriggeredBy(Position, delta))
                spawnFuturePlayerTrigger.Fire();

            if (toggleDoorsTrigger.IsTriggeredBy(Position, delta))
                toggleDoorsTrigger.Fire();

            if (timeTravelTrigger.IsTriggeredBy(Position, delta))
                timeTravelTrigger.Fire();

            if (door1.CanPass(Position, delta) && door2.CanPass(Position, delta))
                Position.X += delta;
        }
    }

    public class FuturePlayer : Player
    {
        public void Move(float delta, Door door1, Door door2, Trigger doorsTrigger)
        {
            if (door1.CanPass(Position, delta) && door2.CanPass(Position, delta))
                Position.X += delta;

            if (doorsTrigger.IsTriggeredBy(Position, delta))
                doorsTrigger.Fire();
        }
    }

    public class PastPlayer : Player
    {
        private Dictionary<int, Position> pastPositions = new Dictionary<int, Position>();
        private float currentFloatGtc = 0f;
        private int currentGtc = 0;

        public void Move(float deltaTime, int gtc, Door door1)
        {
            if (!pastPositions.Any())
                return;

            if (currentGtc >= pastPositions.Keys.Max())
                Exists = false;
            else
            {
                if ((door1.CanPass(pastPositions[currentGtc], pastPositions[currentGtc + 1])))
                {
                    currentFloatGtc += deltaTime * 25f;
                    currentGtc = (int)currentFloatGtc;
                }

                Position = pastPositions[currentGtc];
            }
        }

        public void Record(PresentPlayer present, int gtc)
        {
            if (pastPositions.ContainsKey(gtc))
                pastPositions[gtc] = new Position() { X = present.Position.X };
            else
                pastPositions.Add(gtc, new Position() { X = present.Position.X });
        }

        public void Spawn()
        {
            Exists = true;
            currentFloatGtc = currentGtc = pastPositions.Keys.First();
        }
    }
}
