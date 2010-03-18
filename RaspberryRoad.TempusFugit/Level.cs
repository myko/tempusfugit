using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RaspberryRoad.TempusFugit
{
    public class Level
    {
        public Door Door1 { get; set; }
        public Door Door2 { get; set; }

        public PastPlayer PastPlayer { get; set; }
        public PresentPlayer PresentPlayer { get; set; }
        public FuturePlayer FuturePlayer { get; set; }

        bool futurePlayerMoving = false;

        PositionalTrigger toggleDoorsTrigger;
        Trigger spawnFuturePlayerTrigger;
        Trigger moveFuturePlayerTrigger;
        PositionalTrigger timeTravelTrigger;
        PositionalTrigger timeTravelArrivalEffectTrigger;
        Trigger timeTravelDepartureEffectTrigger;
        Trigger removePastPlayerTrigger;
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

            Position doorsButtonPosition = new Position() { X = 10 };
            Position timeMachinePosition = new Position() { X = 4f };

            toggleDoorsTrigger = new PositionalTrigger() { Position = doorsButtonPosition };
            toggleDoorsTrigger.Actions.Add(() =>
            {
                Door1.Toggle();
                Door2.Toggle();
            });

            moveFuturePlayerTrigger = new Trigger() { OneTime = true };
            moveFuturePlayerTrigger.Actions.Add(() =>
            {
                futurePlayerMoving = true;
            });

            removePastPlayerTrigger = new Trigger() { OneTime = true };
            removePastPlayerTrigger.Actions.Add(() => { 
                PastPlayer.Exists = false;
            });

            timeTravelArrivalEffectTrigger = new PositionalTrigger() { Position = new Position() { X = -2 }, OneTime = true };
            timeTravelArrivalEffectTrigger.Actions.Add(() =>
            {
                specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachinePosition, spawnFuturePlayerTrigger, moveFuturePlayerTrigger, 
                    t => Matrix.CreateScale(Math.Min(t, 1f) * 2f),
                    t => Math.Min(1f, 2.5f - t)));
            });

            timeTravelDepartureEffectTrigger = new Trigger() { OneTime = true };
            timeTravelDepartureEffectTrigger.Actions.Add(() =>
            {
                specialEffects.Add(new SpecialEffect(timeTravelSphere, new Position() { X = PastPlayer.Position.X }, removePastPlayerTrigger, null, 
                    t => Matrix.CreateScale(Math.Min(2.5f - t, 1f) * 2f),
                    t => Math.Min(1f, t)));
            });

            spawnFuturePlayerTrigger = new Trigger() { OneTime = true };
            spawnFuturePlayerTrigger.Actions.Add(() =>
            {
                FuturePlayer.Exists = true;
                TargetGtc = time.GlobalTimeCoordinate;
            });

            timeTravelTrigger = new PositionalTrigger() { Position = timeMachinePosition, OneTime = true };
            timeTravelTrigger.Actions.Add(() =>
            {
                time.JumpTo(TargetGtc);
                PastPlayer.Spawn();
                Door1.IsOpen = false;
                Door2.IsOpen = true;
                FuturePlayer.Exists = false;
                specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachinePosition, null, null, t => Matrix.CreateScale((float)Math.Sin(t*40)*0.2f + 1.8f), t => (t < 0.5f ? 1f : 0f)));
            });
        }

        public bool Update(float dt, Time time, KeyboardState state)
        {
            if (state.IsKeyDown(Keys.Right))
                MovePlayer(dt);

            if (state.IsKeyDown(Keys.Left))
                MovePlayer(-dt);
            
            if (FuturePlayer.Exists)
                MoveFuturePlayer(dt);

            if (PastPlayer.Exists)
                MovePastPlayer(dt, time);

            if (FuturePlayer.Exists)
                PastPlayer.Record(PresentPlayer, time.GlobalTimeCoordinate);

            if (time.GlobalTimeCoordinate > 500 && (PresentPlayer.Position.X < (Door1.Position.X + 0.1f) || PastPlayer.Position.X < (Door1.Position.X + 0.1f)))
                return false;

            return true;
        }

        public void MovePlayer(float delta)
        {
            // TODO: Don't pass everything in the level to the present player
            PresentPlayer.Move(Player.Speed * delta, Door1, Door2, FuturePlayer, toggleDoorsTrigger, timeTravelArrivalEffectTrigger, timeTravelTrigger);
        }

        public void MoveFuturePlayer(float delta)
        {
            // TODO: Don't pass everything in the level to the future player
            if (futurePlayerMoving)
                FuturePlayer.Move(Player.Speed * delta, Door1, Door2, toggleDoorsTrigger);
        }

        public void MovePastPlayer(float deltaTime, Time time)
        {
            // TODO: Don't pass everything in the level to the past player
            PastPlayer.Move(deltaTime, time.GlobalTimeCoordinate, Door1, timeTravelDepartureEffectTrigger);
        }
    }
}
