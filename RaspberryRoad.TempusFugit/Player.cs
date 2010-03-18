using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RaspberryRoad.TempusFugit
{
    public struct Position
    {
        public float X;

        public Position(float x)
        {
            this.X = x;
        }
    }

    public class Player
    {
        public const float Speed = 3f;

        public Position Position { get; set; }
        public int Rotation { get; protected set; }
        public bool Exists { get; set; }

        public AnimatedModel Model { get; private set; }

        public Player(Model model)
        {
            this.Model = new AnimatedModel(model);
            Exists = true;

            Position = new Position();
            Rotation = -1;
        }

        public Matrix GetMatrix()
        {
            return Matrix.CreateRotationY((float)(Math.PI / 2.0 * Rotation)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(Position.X, 0, 0);
        }
    }

    public class PresentPlayer : Player
    {
        public PresentPlayer(Model model)
            :base(model)
        {
        }

        public void Move(float delta, Door door1, Door door2, FuturePlayer future, PositionalTrigger toggleDoorsTrigger, PositionalTrigger spawnFuturePlayerTrigger, PositionalTrigger timeTravelTrigger)
        {
            Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(delta)), true, Matrix.Identity);

            if (delta < 0)
                Rotation = 1;
            if (delta > 0)
                Rotation = -1;
            
            if (spawnFuturePlayerTrigger.IsTriggeredBy(Position, delta))
                spawnFuturePlayerTrigger.Fire();

            if (toggleDoorsTrigger.IsTriggeredBy(Position, delta))
                toggleDoorsTrigger.Fire();

            if (timeTravelTrigger.IsTriggeredBy(Position, delta))
                timeTravelTrigger.Fire();

            if (((Position.X + delta) > -10) && ((Position.X + delta) < 14) && door1.CanPass(Position, delta) && door2.CanPass(Position, delta))
                Position = new Position(Position.X + delta);
        }
    }

    public class FuturePlayer : Player
    {
        public FuturePlayer(Model model)
            : base(model)
        {
        }

        public void Move(float delta, Door door1, Door door2, PositionalTrigger doorsTrigger)
        {
            Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(delta)), true, Matrix.Identity);

            if (door1.CanPass(Position, delta) && door2.CanPass(Position, delta))
                Position = new Position(Position.X + delta);

            if (doorsTrigger.IsTriggeredBy(Position, delta))
                doorsTrigger.Fire();
        }
    }

    public class PastPlayer : Player
    {
        private Dictionary<int, Position> pastPositions = new Dictionary<int, Position>();
        private float currentFloatGtc = 0f;
        private int currentGtc = 0;

        public PastPlayer(Model model)
            : base(model)
        {
        }

        public void Move(float deltaTime, int gtc, Door door1, Trigger departureTrigger)
        {
            if (!pastPositions.Any())
                return;

            if (currentGtc >= pastPositions.Keys.Max())
            {
                departureTrigger.Fire();
            }
            else
            {
                float delta = pastPositions[currentGtc + 1].X - pastPositions[currentGtc].X;
                if (delta < 0)
                    Rotation = 1;
                if (delta > 0)
                    Rotation = -1;

                Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(9f * delta / 25f)), true, Matrix.Identity);

                if ((door1.CanPass(pastPositions[currentGtc], pastPositions[currentGtc + 1])))
                {
                    currentFloatGtc += deltaTime * 25f;
                    currentGtc = (int)currentFloatGtc;
                }

                float a = 1f - (currentFloatGtc - currentGtc);
                Position = new Position() { X = pastPositions[currentGtc].X * a + pastPositions[Math.Min(currentGtc + 1, pastPositions.Keys.Max())].X * (1 - a) };
            }
        }

        public void Record(PresentPlayer present, int gtc)
        {
            if (pastPositions.ContainsKey(gtc))
                pastPositions[gtc] = new Position(present.Position.X);
            else
                pastPositions.Add(gtc, new Position(present.Position.X));
        }

        public void Spawn()
        {
            Exists = true;
            currentFloatGtc = currentGtc = pastPositions.Keys.First();
        }
    }
}
