using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RaspberryRoad.Xna.Collision;

namespace RaspberryRoad.TempusFugit
{
    public class Player
    {
        public const float Speed = 3f;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public int Rotation { get; protected set; }
        public bool Exists { get; set; }

        public AnimatedModel Model { get; private set; }

        public Player(Model model)
        {
            this.Model = new AnimatedModel(model);
            Exists = true;

            Position = new Vector2();
            Velocity = new Vector2();
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
        public bool IsGrounded { get; set; }
        public bool IsFalling { get; set; }

        public PresentPlayer(Model model)
            :base(model)
        {
            IsGrounded = true;
        }

        public Rectangle2 GetRectangle()
        {
            return new Rectangle2(Position - new Vector2(0.2f, -2), Position + new Vector2(0.2f, 0));
        }

        public void Move(Vector2 acceleration, float dt, Level level)
        {
            Vector2 dragCoefficient = new Vector2(2.3f, 0.1f);
            Vector2 drag = new Vector2(dragCoefficient.X * Velocity.X * Math.Abs(Velocity.X), dragCoefficient.Y * Velocity.Y * Math.Abs(Velocity.Y));
            if (IsGrounded)
                drag *= 1.75f;
            Velocity = Velocity + acceleration * dt - drag * dt;
            
            if (IsGrounded)
                Model.AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(Velocity.X * 0.66f * dt)), true, Matrix.Identity);

            if (Velocity.X < 0)
                Rotation = 1;
            if (Velocity.X > 0)
                Rotation = -1;
            if (Velocity.Length() < 0.50f)
                Velocity = Vector2.Zero;

            level.FirePositionalTriggers(Position, Velocity.X * dt);

            var v = level.GetFinalVelocity(GetRectangle(), Velocity * dt);

            // Reduce jiggle
            if (Math.Abs(v.Y) < 0.001f)
                v.Y = 0;
            if (Math.Abs(v.X) < 0.001f)
                v.X = 0;

            // Reduce jiggle
            if (Math.Abs(Velocity.Y) < 0.3f)
                Velocity = new Vector2(Velocity.X, 0);
            if (Math.Abs(Velocity.X) < 0.3f)
                Velocity = new Vector2(0, Velocity.Y);

            Position = Position + v;

            var crv = level.GetCollisionResolvingVector(GetRectangle());
            Position = Position + crv;

            IsGrounded = level.IsGrounded(GetRectangle()) && !(Velocity.Y > 0);

            IsFalling = !IsGrounded && Velocity.Y < 0;
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
                Position = Position + new Vector2(delta, 0);
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
        private Dictionary<int, Vector2> pastPositions = new Dictionary<int, Vector2>();
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
            Position = pastPositions[currentGtc] * a + pastPositions[Math.Min(currentGtc + 1, pastPositions.Keys.Max())] * (1 - a);

            return true;
        }

        public void Record(PresentPlayer present, int gtc)
        {
            if (pastPositions.ContainsKey(gtc))
                pastPositions[gtc] = present.Position;
            else
                pastPositions.Add(gtc, present.Position);
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
