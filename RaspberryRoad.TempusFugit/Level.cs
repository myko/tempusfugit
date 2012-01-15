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
        public PastPlayer pastPlayer;
        public FuturePlayer futurePlayer;
        bool futurePlayerMoving = false;

        // TODO: Don't keep a static list of all our geometry
        private List<Line2> lines;

        ICollection<Entity> entities = new List<Entity>();
        ICollection<PositionalTrigger> positionalTriggers = new List<PositionalTrigger>();
        ICollection<DecisionPoint> decisionPoints = new List<DecisionPoint>();

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
            lines.Add(new Line2(new Vector2(-10, 0), new Vector2(25, 0)));
            lines.Add(new Line2(new Vector2(30, 0), new Vector2(50, 0)));
            lines.Add(new Line2(new Vector2(-10, 4), new Vector2(-10, 0)));
            lines.Add(new Line2(new Vector2(50, 4), new Vector2(-10, 4)));
            lines.Add(new Line2(new Vector2(50, 0), new Vector2(50, 4)));
            lines.Add(new Line2(new Vector2(25, 0), new Vector2(25, -10)));
            lines.Add(new Line2(new Vector2(30, -10), new Vector2(30, 0)));

            PresentPlayer = new PresentPlayer(playerModel);
            PresentPlayer.Position = new Vector2(-5, 2); // -5

            futurePlayer = new FuturePlayer(playerModel);
            futurePlayer.Position = new Vector2(4, 0);
            futurePlayer.Exists = false;

            pastPlayer = new PastPlayer(playerModel);
            pastPlayer.Exists = false;

            var ground = new StaticEntity(groundModel, Matrix.Identity);
            var travelPad = new StaticEntity(travelPadModel, Matrix.CreateTranslation(4, 0, 0));
            var travelPad2 = new StaticEntity(travelPadModel, Matrix.CreateTranslation(32, 0, 0));
            var button = new StaticEntity(buttonModel, Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(10, 1, -1.7f));
            var button2 = new StaticEntity(buttonModel, Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(35, 1, -1.7f));
            
            var door1 = new Door(doorModel);
            door1.Position = new Vector2(0, 2);
            door1.IsOpen = false;
            
            var door2 = new Door(doorModel);
            door2.Position = new Vector2(8, 2);
            door2.IsOpen = true;
            
            var door3 = new Door(doorModel);
            door3.Position = new Vector2(27.5f, 0);
            door3.IsOpen = true;
            door3.Width = 4;
            door3.Height = 1;

            entities.Add(ground);
            entities.Add(travelPad);
            entities.Add(travelPad2);
            entities.Add(button);
            entities.Add(button2);
            entities.Add(door1);
            entities.Add(door2);
            entities.Add(door3);

            Vector2 doorsButtonPosition = new Vector2() { X = 10 };
            Vector2 door3ButtonPosition = new Vector2() { X = 34 };
            Vector2 timeMachinePosition = new Vector2() { X = 4f };
            Vector2 timeMachine2Position = new Vector2() { X = 32 };

            // toggle doors trigger
            positionalTriggers.Add(new PositionalTrigger()
            {
                Position = doorsButtonPosition,
                Actions = {
                    () =>
                    {
                        door1.Toggle();
                        door2.Toggle();
                    }}
            });

            // toggle door 3 trigger
            positionalTriggers.Add(new PositionalTrigger()
            {
                Position = door3ButtonPosition,
                Actions = {
                    () =>
                    {
                        door3.Toggle();
                    }}
            }); 

            var moveFuturePlayerTrigger = new Trigger() { OneTime = true };
            moveFuturePlayerTrigger.Actions.Add(() =>
            {
                futurePlayerMoving = true;
            });

            var removePastPlayerTrigger = new Trigger() { OneTime = true };
            removePastPlayerTrigger.Actions.Add(() =>
            { 
                pastPlayer.Exists = false;
            });

            var spawnFuturePlayerTrigger = new Trigger() { OneTime = false };
            spawnFuturePlayerTrigger.Actions.Add(() =>
            {
                futurePlayer.Exists = true;
                TargetGtc = Time.GlobalTimeCoordinate;
            });

            //var timeTravelDepartureEffectTrigger = new Trigger() { OneTime = true };
            //timeTravelDepartureEffectTrigger.Actions.Add(() =>
            //{
            //    specialEffects.Add(new SpecialEffect(timeTravelSphere, new Vector2() { X = pastPlayer.Position.X }, removePastPlayerTrigger, null, 
            //        t => Matrix.CreateScale(Math.Min(2.5f - t, 1f) * 1.65f),
            //        t => Math.Min(1f, t)));
            //});

            var unlockPlayerTrigger = new Trigger() { OneTime = false };
            unlockPlayerTrigger.Actions.Add(() => 
            {
                lockPlayer = false;
            });

            // timeTravelArrivalEffectTrigger
            positionalTriggers.Add(new PositionalTrigger()
            {
                Position = new Vector2() { X = -2.5f }, 
                OneTime = true,
                Actions = {
                    () =>
                    {
                        futurePlayer.Exists = true;
                        futurePlayer.Position = new Vector2(timeMachinePosition.X, 0);
                        specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachinePosition, spawnFuturePlayerTrigger, moveFuturePlayerTrigger, 
                            t => Matrix.CreateScale(Math.Min(t, 1f) * 1.65f),
                            t => Math.Min(1f, 2.5f - t)));
                    }}
            }); 

            // timeTravelArrivalEffect2Trigger
            positionalTriggers.Add(new PositionalTrigger()
            {
                Position = new Vector2() { X = 23.5f },
                OneTime = true,
                Actions = {
                    () =>
                    {
                        futurePlayer.Exists = true;
                        futurePlayer.Position = new Vector2(timeMachine2Position.X, 0);
                        specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachine2Position, spawnFuturePlayerTrigger, moveFuturePlayerTrigger,
                            t => Matrix.CreateScale(Math.Min(t, 1f) * 1.65f),
                            t => Math.Min(1f, 2.5f - t)));
                    }}
            }); 


            // timeTravelTrigger
            positionalTriggers.Add(new PositionalTrigger()
            {
                Position = timeMachinePosition,
                OneTime = true,
                Actions = {
                    () =>
                    {
                        Time.JumpTo(TargetGtc);
                        pastPlayer.Spawn();
                        door1.IsOpen = false;
                        door2.IsOpen = true;
                        door3.IsOpen = true;
                        futurePlayer.Exists = false;
                        lockPlayer = true;
                        PresentPlayer.Velocity = new Vector2(0, 0);
                        specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachinePosition, null, unlockPlayerTrigger,
                            t => Matrix.CreateScale(1.65f),
                            t => (float)Math.Sin(t / 2.5 * Math.PI)));
                    }}
            }); 

            // timeTravel2Trigger
            positionalTriggers.Add(new PositionalTrigger()
            {
                Position = timeMachine2Position,
                OneTime = true,
                Actions = {
                    () =>
                    {
                        Time.JumpTo(TargetGtc);
                        pastPlayer.Spawn();
                        door1.IsOpen = false;
                        door2.IsOpen = true;
                        door3.IsOpen = true;
                        futurePlayer.Exists = false;
                        lockPlayer = true;
                        PresentPlayer.Velocity = new Vector2(0, 0);
                        specialEffects.Add(new SpecialEffect(timeTravelSphere, timeMachine2Position, null, unlockPlayerTrigger,
                            t => Matrix.CreateScale(1.65f),
                            t => (float)Math.Sin(t / 2.5 * Math.PI)));
                    }}
            });

            decisionPoints.Add(new DecisionPoint() { Position = new Vector2(-2, 0), Condition = () => door1.IsOpen });
            //decisionPoints.Add(new DecisionPoint() { Position = new Vector2(timeMachinePosition.X, 0), Condition = () => true });
            decisionPoints.Add(new DecisionPoint() { Position = new Vector2(24, 0), Condition = () => !door3.IsOpen });
            //decisionPoints.Add(new DecisionPoint() { Position = new Vector2(door3.Position.X, 0), Condition = () => true });
            //decisionPoints.Add(new DecisionPoint() { Position = new Vector2(timeMachine2Position.X, 0), Condition = () => true });
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
                pastPlayer.Record(PresentPlayer, this, Time.GlobalTimeCoordinate);

            //if (Time.GlobalTimeCoordinate > 500 && (PresentPlayer.Position.X < (door1.Position.X + 0.1f) || pastPlayer.Position.X < (door1.Position.X + 0.1f)))
            //    return false;

            if (PresentPlayer.Position.Y < -20)
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
                //timeTravelDepartureEffectTrigger.Fire();
                pastPlayer.Despawn();
        }

        public IEnumerable<Entity> GetEntities()
        {
            return entities;
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
            foreach (var trigger in positionalTriggers)
            {
                if (trigger.IsTriggeredBy(Position, delta))
                    trigger.Fire();
            }
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

        public IEnumerable<DecisionPoint> GetDecisionPoints()
        {
            return decisionPoints;
        }
    }
}
