using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RaspberryRoad.TempusFugit
{
    public class Level
    {
        private PastPlayer pastPlayer;
        private FuturePlayer futurePlayer;
        bool futurePlayerMoving = false;

        // TODO: Don't keep a static list of all our geometry
        private StaticEntity ground;
        private StaticEntity travelPad;
        private StaticEntity button;
        private Door door1;
        private Door door2;

        // TODO: Don't keep a static list of all our "scripting"
        PositionalTrigger toggleDoorsTrigger;
        Trigger spawnFuturePlayerTrigger;
        Trigger moveFuturePlayerTrigger;
        PositionalTrigger timeTravelTrigger;
        PositionalTrigger timeTravelArrivalEffectTrigger;
        Trigger timeTravelDepartureEffectTrigger;
        Trigger removePastPlayerTrigger;
        Trigger unlockPlayerTrigger;

        public int TargetGtc { get; set; }
        public PresentPlayer PresentPlayer { get; set; }
        public Time Time { get; set; }

        private bool lockPlayer = false;
        
        // TODO: Don't take a reference to the list of special effects
        // TODO: Load all the scripts from a file? Or at least make a "Level 1" subclass
        public Level(IList<SpecialEffect> specialEffects, IDictionary<string, Model> models)
        {
            Model timeTravelSphere = models["timetravelsphere"];
            Model playerModel = models["dude"];
            Model groundModel = models["ground"];
            Model doorModel = models["door"];
            Model travelPadModel = models["travelpad"];
            Model buttonModel = models["button"];

            Time = new Time();

            PresentPlayer = new PresentPlayer(playerModel);
            PresentPlayer.Position = new Vector2(-5, 0);

            futurePlayer = new FuturePlayer(playerModel);
            futurePlayer.Position = new Vector2(4, 0);
            futurePlayer.Exists = false;

            pastPlayer = new PastPlayer(playerModel);
            pastPlayer.Exists = false;

            ground = new StaticEntity(groundModel, Matrix.Identity);
            travelPad = new StaticEntity(travelPadModel, Matrix.CreateTranslation(4, 0, 0));
            button = new StaticEntity(buttonModel, Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(10, 1, -1.7f));

            door1 = new Door(doorModel);
            door1.Position = new Vector2(0, 0);
            door1.IsOpen = false;

            door2 = new Door(doorModel);
            door2.Position = new Vector2(8, 0);
            door2.IsOpen = true;

            Vector2 doorsButtonPosition = new Vector2() { X = 10 };
            Vector2 timeMachinePosition = new Vector2() { X = 4f };

            toggleDoorsTrigger = new PositionalTrigger() { Position = doorsButtonPosition };
            toggleDoorsTrigger.Actions.Add(() =>
            {
                door1.Toggle();
                door2.Toggle();
            });

            moveFuturePlayerTrigger = new Trigger() { OneTime = true };
            moveFuturePlayerTrigger.Actions.Add(() =>
            {
                futurePlayerMoving = true;
            });

            removePastPlayerTrigger = new Trigger() { OneTime = true };
            removePastPlayerTrigger.Actions.Add(() => { 
                pastPlayer.Exists = false;
            });

            timeTravelArrivalEffectTrigger = new PositionalTrigger() { Position = new Vector2() { X = -2 }, OneTime = true };
            timeTravelArrivalEffectTrigger.Actions.Add(() =>
            {
                specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachinePosition, spawnFuturePlayerTrigger, moveFuturePlayerTrigger, 
                    t => Matrix.CreateScale(Math.Min(t, 1f) * 1.65f),
                    t => Math.Min(1f, 2.5f - t)));
            });

            timeTravelDepartureEffectTrigger = new Trigger() { OneTime = true };
            timeTravelDepartureEffectTrigger.Actions.Add(() =>
            {
                specialEffects.Add(new SpecialEffect(timeTravelSphere, new Vector2() { X = pastPlayer.Position.X }, removePastPlayerTrigger, null, 
                    t => Matrix.CreateScale(Math.Min(2.5f - t, 1f) * 1.65f),
                    t => Math.Min(1f, t)));
            });

            spawnFuturePlayerTrigger = new Trigger() { OneTime = true };
            spawnFuturePlayerTrigger.Actions.Add(() =>
            {
                futurePlayer.Exists = true;
                TargetGtc = Time.GlobalTimeCoordinate;
            });

            timeTravelTrigger = new PositionalTrigger() { Position = timeMachinePosition, OneTime = true };
            timeTravelTrigger.Actions.Add(() =>
            {
                Time.JumpTo(TargetGtc);
                pastPlayer.Spawn();
                door1.IsOpen = false;
                door2.IsOpen = true;
                futurePlayer.Exists = false;
                lockPlayer = true;
                PresentPlayer.Velocity = new Vector2(0, 0);
                specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachinePosition, null, unlockPlayerTrigger,
                    t => Matrix.CreateScale(1.65f),
                    t => (float)Math.Sin(t / 2.5 * Math.PI)));
            });

            unlockPlayerTrigger = new Trigger() { OneTime = true };
            unlockPlayerTrigger.Actions.Add(() => 
            {
                lockPlayer = false;
            });
        }

        bool holdingJumpDuringFall = false;
        // TODO: Don't pass keyboard state directly, abstract it away
        public bool Update(float dt, KeyboardState state)
        {
            Time.UpdateGameTime(dt);

            float vx = 0f;
            float vy = 0f;

            if (!lockPlayer)
            {
                if (state.IsKeyDown(Keys.Right))
                    vx = 50;

                if (state.IsKeyDown(Keys.Left))
                    vx = -50;

                if (PresentPlayer.IsFalling && state.IsKeyDown(Keys.Up))
                    holdingJumpDuringFall = true;

                if (PresentPlayer.IsGrounded && state.IsKeyUp(Keys.Up))
                    holdingJumpDuringFall = false;

                if (!holdingJumpDuringFall && !PresentPlayer.IsFalling && state.IsKeyDown(Keys.Up))
                    vy = PresentPlayer.IsGrounded ? 900 : 22;
            }

            if (!PresentPlayer.IsGrounded)
                vy -= 58f;

            PresentPlayer.Move(new Vector2(vx, vy), dt, this);

            // TODO: Move this to a "script"
            if (futurePlayer.Exists)
                MoveFuturePlayer(dt);

            if (pastPlayer.Exists)
                MovePastPlayer(dt, Time);

            if (futurePlayer.Exists)
                pastPlayer.Record(PresentPlayer, Time.GlobalTimeCoordinate);

            if (Time.GlobalTimeCoordinate > 500 && (PresentPlayer.Position.X < (door1.Position.X + 0.1f) || pastPlayer.Position.X < (door1.Position.X + 0.1f)))
                return false;

            return true;
        }

        public void MoveFuturePlayer(float delta)
        {
            // TODO: Use some kind of script for "futurePlayerMoving" instead of a bool
            if (futurePlayerMoving)
                futurePlayer.Move(Player.Speed * delta, this);
        }

        public void MovePastPlayer(float deltaTime, Time time)
        {
            if (!pastPlayer.Move(deltaTime, this))
                timeTravelDepartureEffectTrigger.Fire();
        }

        public IEnumerable<Entity> GetEntities()
        {
            yield return ground;
            yield return travelPad;
            yield return button;
            yield return door1;
            yield return door2;
        }

        public IEnumerable<Player> GetActivePlayers()
        {
            if (PresentPlayer.Exists)
                yield return PresentPlayer;
            if (futurePlayer.Exists)
                yield return futurePlayer;
            if (pastPlayer.Exists)
                yield return pastPlayer;
        }

        public bool CanMove(Vector2 position, float delta)
        {
            return CanMove(position, new Vector2(position.X + delta, position.Y));
        }

        public bool CanMove(Vector2 position, Vector2 newPosition)
        {
            return (door1.CanPass(position, newPosition) && door2.CanPass(position, newPosition));
        }

        public void FirePositionalTriggers(Vector2 Position, float delta)
        {
            if (timeTravelArrivalEffectTrigger.IsTriggeredBy(Position, delta))
                timeTravelArrivalEffectTrigger.Fire();

            if (toggleDoorsTrigger.IsTriggeredBy(Position, delta))
                toggleDoorsTrigger.Fire();

            if (timeTravelTrigger.IsTriggeredBy(Position, delta))
                timeTravelTrigger.Fire();
        }

        public Vector2 ModifyVelocity(Vector2 Position, Vector2 Velocity, float dt)
        {
            float vy = Velocity.Y;
            float vx = Velocity.X;
            
            if (!CanMove(Position, vx * dt))
                vx = 0;

            // Left edge
            if ((Position.X + vx * dt) < -30 && vx < 0)
                vx = (-30 - Position.X) / dt;

            // Right edge
            if ((Position.X + vx * dt) > 50 && vx > 0)
                vx = (50 - Position.X) / dt;

            // Ground
            if ((Position.Y + vy * dt) < 0 && vy < 0)
                vy = (0 - Position.Y) / dt;

            return new Vector2(vx, vy);
        }
    }
}
