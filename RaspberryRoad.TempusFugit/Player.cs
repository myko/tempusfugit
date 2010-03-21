using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RaspberryRoad.TempusFugit
{
    public class Player
    {
        public const float Speed = 3f;

        public Position Position { get; set; }
        public Position Velocity { get; set; }
        public int Rotation { get; protected set; }
        public bool Exists { get; set; }

        public AnimatedModel Model { get; private set; }

        public Player(Model model)
        {
            this.Model = new AnimatedModel(model);
            Exists = true;

            Position = new Position();
            Velocity = new Position();
            Rotation = -1;
        }

        public Matrix GetMatrix()
        {
            return Matrix.CreateRotationY((float)(Math.PI / 2.0 * Rotation)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(Position.X, Position.Y, 0);
        }

        public virtual Vector3 GetColor()
        {
            return new Vector3(1, 1, 1);
        }
    }

    public class PresentPlayer : Player
    {
        public PresentPlayer(Model model)
            :base(model)
        {
        }

        public void Move(Position acceleration, float dt, Level level)
        {
            Velocity = new Position(Math.Max(-Player.Speed, Math.Min(Player.Speed, Velocity.X + acceleration.X * dt - Velocity.X * dt * 2)), Velocity.Y + acceleration.Y * dt);

            Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(Velocity.X * dt)), true, Matrix.Identity);

            if (acceleration.X < 0)
                Rotation = 1;
            if (acceleration.X > 0)
                Rotation = -1;

            level.FirePositionalTriggers(Position, Velocity.X * dt);

            if (level.CanMove(Position, Velocity.X * dt))
                Position = new Position(Position.X + Velocity.X * dt, Position.Y + Velocity.Y * dt);
            else
                Velocity = new Position(0, 0);
        }
    }

    public class FuturePlayer : Player
    {
        public FuturePlayer(Model model)
            : base(model)
        {
        }

        public void Move(float delta, Level level)
        {
            if (level.CanMove(Position, delta))
            {
                Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(delta)), true, Matrix.Identity);
                Position = new Position(Position.X + delta, Position.Y);
            }

            level.FirePositionalTriggers(Position, delta);
        }

        public override Vector3 GetColor()
        {
            return new Vector3(0.5f, 0.7f, 1);
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

        public bool Move(float deltaTime, Level level)
        {
            if (!pastPositions.Any())
                return true;

            if (currentGtc >= pastPositions.Keys.Max())
                return false;


            if ((level.CanMove(pastPositions[currentGtc], pastPositions[currentGtc + 1])))
            {
                float delta = pastPositions[currentGtc + 1].X - pastPositions[currentGtc].X;
                if (delta < 0)
                    Rotation = 1;
                if (delta > 0)
                    Rotation = -1;

                Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(9f * delta / 25f)), true, Matrix.Identity);

                currentFloatGtc += deltaTime * 25f;
                currentGtc = (int)currentFloatGtc;
            }

            float a = 1f - (currentFloatGtc - currentGtc);
            Position = new Position() { X = pastPositions[currentGtc].X * a + pastPositions[Math.Min(currentGtc + 1, pastPositions.Keys.Max())].X * (1 - a) };

            return true;
        }

        public void Record(PresentPlayer present, int gtc)
        {
            if (pastPositions.ContainsKey(gtc))
                pastPositions[gtc] = new Position(present.Position.X, present.Position.Y);
            else
                pastPositions.Add(gtc, new Position(present.Position.X, present.Position.Y));
        }

        public void Spawn()
        {
            Exists = true;
            currentFloatGtc = currentGtc = pastPositions.Keys.First();
        }

        public override Vector3 GetColor()
        {
            return new Vector3(0.5f, 1, 0.7f);
        }
    }
}
