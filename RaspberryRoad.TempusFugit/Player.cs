using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;
using System;
using Microsoft.Xna.Framework;

namespace RaspberryRoad.TempusFugit
{
    public class Position
    {
        public float X { get; set; }
    }

    public class Player
    {
        public Position Position { get; set; }
        public int Rotation { get; set; }
        public bool Exists { get; set; }

        public Model Model { get; set; }
        public AnimationPlayer AnimationPlayer { get; set; }

        public Player(Model model)
        {
            this.Model = model;
            Exists = true;

            Position = new Position();
            Rotation = -1;

            // Look up our custom skinning information.
            SkinningData skinningData = model.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            AnimationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            AnimationPlayer.StartClip(clip);
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
            AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(delta)), true, Matrix.Identity);

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
                Position.X += delta;
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
            AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(delta)), true, Matrix.Identity);

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
                Exists = false;
                departureTrigger.Fire();
            }
            else
            {
                float delta = pastPositions[currentGtc + 1].X - pastPositions[currentGtc].X;
                if (delta < 0)
                    Rotation = 1;
                if (delta > 0)
                    Rotation = -1;

                AnimationPlayer.Update(TimeSpan.FromSeconds(Math.Abs(3f * delta / 25f)), true, Matrix.Identity);

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
