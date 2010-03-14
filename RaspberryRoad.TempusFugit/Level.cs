using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RaspberryRoad.TempusFugit
{
    public class Level
    {
        public Door Door1 { get; set; }
        public Door Door2 { get; set; }

        public PastPlayer PastPlayer { get; set; }
        public PresentPlayer PresentPlayer { get; set; }
        public FuturePlayer FuturePlayer { get; set; }

        PositionalTrigger toggleDoorsTrigger;
        Trigger spawnFuturePlayerTrigger;
        PositionalTrigger timeTravelTrigger;
        PositionalTrigger timeTravelArrivalEffectTrigger;
        Trigger timeTravelDepartureEffectTrigger;
        public int TargetGtc { get; set; }
        
        public void Reset(List<SpecialEffect> specialEffects, Model timeTravelSphere, Model model, Time time)
        {
            PresentPlayer = new PresentPlayer(model);
            PresentPlayer.Position.X = -5;

            FuturePlayer = new FuturePlayer(model);
            FuturePlayer.Position.X = 4;
            FuturePlayer.Exists = false;

            PastPlayer = new PastPlayer(model);
            PastPlayer.Exists = false;

            Door1 = new Door();
            Door1.Position.X = 0;
            Door1.IsOpen = false;

            Door2 = new Door();
            Door2.Position.X = 8;
            Door2.IsOpen = true;

            toggleDoorsTrigger = new PositionalTrigger() { Position = new Position() { X = 10 } };
            toggleDoorsTrigger.Actions.Add(() =>
            {
                Door1.Toggle();
                Door2.Toggle();
            });

            timeTravelArrivalEffectTrigger = new PositionalTrigger() { Position = new Position() { X = -2 }, OneTime = true };
            timeTravelArrivalEffectTrigger.Actions.Add(() =>
            {
                specialEffects.Add(new SpecialEffect(timeTravelSphere, new Position() { X = 4f }, spawnFuturePlayerTrigger,
                    t => Matrix.CreateScale(Math.Min(t, 1f) * 2f)));
            });

            timeTravelDepartureEffectTrigger = new Trigger() { OneTime = true };
            timeTravelDepartureEffectTrigger.Actions.Add(() =>
            {
                specialEffects.Add(new SpecialEffect(timeTravelSphere, new Position() { X = PastPlayer.Position.X }, timeTravelTrigger,
                    t => Matrix.CreateScale(Math.Min(1.5f - t, 1f) * 2f)));
            });

            spawnFuturePlayerTrigger = new Trigger() { OneTime = true };
            spawnFuturePlayerTrigger.Actions.Add(() =>
            {
                FuturePlayer.Exists = true;
                TargetGtc = time.GlobalTimeCoordinate;
            });

            timeTravelTrigger = new PositionalTrigger() { Position = new Position() { X = 4 }, OneTime = true };
            timeTravelTrigger.Actions.Add(() =>
            {
                time.JumpTo(TargetGtc);
                PastPlayer.Spawn();
                Door1.IsOpen = false;
                Door2.IsOpen = true;
                FuturePlayer.Exists = false;
            });
        }

        public void MovePlayer(float delta)
        {
            PresentPlayer.Move(delta, Door1, Door2, FuturePlayer, toggleDoorsTrigger, timeTravelArrivalEffectTrigger, timeTravelTrigger);
        }

        public void MoveFuturePlayer(float delta)
        {
            FuturePlayer.Move(delta, Door1, Door2, toggleDoorsTrigger);
        }

        public void MovePastPlayer(float deltaTime, Time time)
        {
            PastPlayer.Move(deltaTime, time.GlobalTimeCoordinate, Door1, timeTravelDepartureEffectTrigger);
        }
    }
}
