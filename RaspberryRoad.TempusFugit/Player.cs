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

    public enum PresentPlayerMoveResult
    {
        None,
        TimeTravel,
        SpawnFuture,
        TriggerDoors
    }

    public class PresentPlayer : Player
    {
        private bool timeTravelled = false;

        public PresentPlayerMoveResult Move(float delta, Door door1, Door door2, Player future)
        {
            if ((Position.X < 0) && (Position.X + delta > 0) && !door1.IsOpen)
                return PresentPlayerMoveResult.None;

            if ((Position.X < 8) && (Position.X + delta > 8) && !door2.IsOpen)
                return PresentPlayerMoveResult.None;

            Position.X += delta;

            if ((Position.X < -2) && (Position.X + delta > -2) && !future.Exists)
            {
                return PresentPlayerMoveResult.SpawnFuture;
            }

            if ((Position.X < 10) && (Position.X + delta > 10))
            {
                return PresentPlayerMoveResult.TriggerDoors;
            }

            if ((Position.X < 2) && (Position.X + delta > 2) && !timeTravelled)
            {
                timeTravelled = true;
                return PresentPlayerMoveResult.TimeTravel;
            }

            return PresentPlayerMoveResult.None;
        }
    }

    public class FuturePlayer : Player
    {
        public PresentPlayerMoveResult Move(float delta, Door door1, Door door2)
        {
            if ((Position.X < 0) && (Position.X + delta > 0) && !door1.IsOpen)
                return PresentPlayerMoveResult.None;

            if ((Position.X < 8) && (Position.X + delta > 8) && !door2.IsOpen)
                return PresentPlayerMoveResult.None;

            Position.X += delta;
 
            if ((Position.X < 10) && (Position.X + delta > 10))
            {
                return PresentPlayerMoveResult.TriggerDoors;
            }

            return PresentPlayerMoveResult.None;
        }
    }
}
