using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RaspberryRoad.Xna.Collision;

namespace RaspberryRoad.TempusFugit
{
    public class Level
    {
        private PastPlayer pastPlayer;
        private FuturePlayer futurePlayer;
        bool futurePlayerMoving = false;

        // TODO: Don't keep a static list of all our geometry
        private List<Line2> lines;
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

            lines = new List<Line2>();
            lines.Add(new Line2(new Vector2(-10, 0), new Vector2(15, 0)));
            lines.Add(new Line2(new Vector2(-10, 4), new Vector2(-10, 0)));
            lines.Add(new Line2(new Vector2(15, 4), new Vector2(-10, 4)));
            lines.Add(new Line2(new Vector2(15, 0), new Vector2(15, 4)));
            
            PresentPlayer = new PresentPlayer(playerModel);
            PresentPlayer.Position = new Vector2(-5, 2);

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

        float? holdingStarted = null;
        // TODO: Don't pass keyboard state directly, abstract it away
        public bool Update(float dt, KeyboardState state)
        {
            Time.UpdateGameTime(dt);

            float vx = 0f;
            float vy = 0f;

            if (!lockPlayer)
            {
                if (state.IsKeyDown(Keys.Right))
                    vx = 75;

                if (state.IsKeyDown(Keys.Left))
                    vx = -75;

                if (state.IsKeyDown(Keys.Up) && !PresentPlayer.IsFalling && (!holdingStarted.HasValue || (Time.Current - holdingStarted < 2)))
                {
                    if (!holdingStarted.HasValue)
                        holdingStarted = Time.Current;
                    vy = 500;
                }
            }

            if (!PresentPlayer.IsGrounded)
                vy -= 58f;

            PresentPlayer.Move(new Vector2(vx, vy), dt, this);
            if (PresentPlayer.IsGrounded && state.IsKeyUp(Keys.Up))
                holdingStarted = null;

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
            return !CollisionDetector.Collides(position, new Vector2(delta, 0), GetCollisionGeometry());
        }

        public bool CanMove(Vector2 position, Vector2 newPosition)
        {
            return !CollisionDetector.Collides(position, newPosition - position, GetCollisionGeometry());
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

        private IEnumerable<Line2> GetCollisionGeometry()
        {
            foreach (var line in lines)
            {
                yield return line;
            }

            foreach (var entity in GetEntities())
            {
                foreach (var line in entity.GetCollisionGeometry())
                {
                    yield return line;
                }
            }
        }

        public Vector2 GetFinalVelocity(Rectangle2 rectangle, Vector2 velocity)
        {
            return CollisionDetector.GetFinalMovement(rectangle, velocity, GetCollisionGeometry());
        }

        public Vector2 GetCollisionResolvingVector(Rectangle2 rectangle)
        {
            return CollisionDetector.GetStaticCollisionResolvingVector(rectangle, GetCollisionGeometry());
        }

        public IEnumerable<VertexPositionNormalTexture> GetLineVertices()
        {
            foreach (var line in GetCollisionGeometry())
            {
                yield return ConvertVertex(line.Start);
                yield return ConvertVertex(line.End);
            }

            var rectangle = PresentPlayer.GetRectangle();

            yield return ConvertVertex(rectangle.Top.Start);
            yield return ConvertVertex(rectangle.Top.End);
            yield return ConvertVertex(rectangle.Left.Start);
            yield return ConvertVertex(rectangle.Left.End);
            yield return ConvertVertex(rectangle.Right.Start);
            yield return ConvertVertex(rectangle.Right.End);
            yield return ConvertVertex(rectangle.Bottom.Start);
            yield return ConvertVertex(rectangle.Bottom.End);

            var groundGeometry = GetCollisionGeometry().Where(x => x.Normal().Y > 0.5f);

            yield return ConvertVertex(rectangle.Center + CollisionDetector.GetStaticCollisionResolvingVector(rectangle, groundGeometry));
            yield return ConvertVertex(rectangle.Center + CollisionDetector.GetStaticCollisionResolvingVector(rectangle, groundGeometry) + new Vector2(0, -0.1f));
        }

        private static VertexPositionNormalTexture ConvertVertex(Vector2 v)
        {
            return new VertexPositionNormalTexture(new Vector3(v.X, v.Y, 0), Vector3.Forward, Vector2.One);
        }

        public bool IsGrounded(Rectangle2 rectangle)
        {
            var groundGeometry = GetCollisionGeometry().Where(x => x.Normal().Y > 0.5f);
            return CollisionDetector.FindCollision(rectangle + CollisionDetector.GetStaticCollisionResolvingVector(rectangle, groundGeometry), new Vector2(0, -0.1f), groundGeometry).HasValue;
        }
    }
}
