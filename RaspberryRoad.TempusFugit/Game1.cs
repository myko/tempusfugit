using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace RaspberryRoad.TempusFugit
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model model;

        float floatGtc;
        float floatPastGtc;
        int gtc;
        int pastGtc;
        Dictionary<int, float> pastPositions = new Dictionary<int,float>();
        int targetGtc;

        Player past;
        PresentPlayer present;
        FuturePlayer future;

        Door door1, door2;

        BasicEffect cubeEffect;
        BasicShape cube;
        BasicShape door;
        SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ResetWorld();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cubeEffect = new BasicEffect(GraphicsDevice, null);

            cube = new BasicShape(new Vector3(100, 1, 1), new Vector3(0, -1, 0));
            cube.shapeTexture = Content.Load<Texture2D>("head");
            door = new BasicShape(new Vector3(0.1f, 2, 1), new Vector3(0, 0, 0));
            door.shapeTexture = Content.Load<Texture2D>("pants");

            model = Content.Load<Model>("dude");
            font = Content.Load<SpriteFont>("Kootenay");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            floatGtc += dt * 25f;
            gtc = (int)floatGtc;

            if (!(past.Exists && pastPositions.Any() && pastPositions[pastGtc] < 0 && pastPositions[pastGtc+1] > 0 && !door1.IsOpen))
            {
                floatPastGtc += dt * 25f;
                pastGtc = (int)floatPastGtc;
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Right))
                MovePlayer(3 * dt);

            if (state.IsKeyDown(Keys.Left))
                MovePlayer(-3 * dt);

            if (future.Exists)
                MoveFuturePlayer(3 * dt);

            if (past.Exists)
                MovePastPlayer();

            if (state.IsKeyDown(Keys.F1))
            {
                ResetWorld();
            }

            if (future.Exists)
            {
                if (pastPositions.ContainsKey(gtc))
                    pastPositions[gtc] = present.Position.X;
                else
                    pastPositions.Add(gtc, present.Position.X);
            }

            if (pastPositions.Keys.Any() && gtc > pastPositions.Keys.Max())
                past.Exists = false;

            base.Update(gameTime);
        }

        private void ResetWorld()
        {
            floatGtc = gtc = 0;
            present = new PresentPlayer();
            present.Position.X = -10;
            future = new FuturePlayer();
            future.Position.X = 2;
            future.Exists = false;
            pastPositions = new Dictionary<int, float>();
            past = new Player();
            past.Exists = false;
            door1 = new Door();
            door1.Position.X = 0;
            door1.IsOpen = false;
            door2 = new Door();
            door2.Position.X = 8;
            door2.IsOpen = true;
        }

        private void MovePlayer(float delta)
        {
            switch (present.Move(delta, door1, door2, future))
            {
                case PresentPlayerMoveResult.None:
                    break;
                case PresentPlayerMoveResult.SpawnFuture:
                    Console.WriteLine("Spawned future player");
                    future.Exists = true;
                    targetGtc = gtc;
                    break;
                case PresentPlayerMoveResult.TimeTravel:
                    Console.WriteLine("Time travelled!");
                    gtc = targetGtc;
                    floatGtc = gtc;
                    past.Exists = true;
                    door1.IsOpen = false;
                    door2.IsOpen = true;
                    future.Exists = false;
                    break;
                case PresentPlayerMoveResult.TriggerDoors:
                    Console.WriteLine("Triggered doors");
                    door1.Toggle();
                    door2.Toggle();
                    break;
            }
        }

        private void MoveFuturePlayer(float delta)
        {
            switch (future.Move(delta, door1, door2))
            {
                case PresentPlayerMoveResult.TriggerDoors:
                    Console.WriteLine("Future player triggered doors.");
                    door1.Toggle();
                    door2.Toggle();
                    break;
            }
        }

        private void MovePastPlayer()
        {

            pastGtc = gtc;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawModel();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.DrawString(font, gtc.ToString() + ", " + targetGtc.ToString(), Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawModel()
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            float aspectRatio = graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(present.Position.X, 8, 18), new Vector3(present.Position.X, 4, 0), Vector3.Up);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = new Vector3(1, 0, 0);

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = Matrix.CreateRotationY((float)(Math.PI / 2.0)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(present.Position.X, 0, 0) * transforms[mesh.ParentBone.Index];
                }
                mesh.Draw();
            }

            if (future.Exists)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.DiffuseColor = new Vector3(0, 1, 0);

                        effect.View = view;
                        effect.Projection = projection;
                        effect.World = Matrix.CreateRotationY((float)(Math.PI / 2.0)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(future.Position.X, 0, 0) * transforms[mesh.ParentBone.Index];
                    }
                    mesh.Draw();
                }
            }

            if (past.Exists)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.DiffuseColor = new Vector3(0, 0, 1);

                        effect.View = view;
                        effect.Projection = projection;
                        effect.World = Matrix.CreateRotationY((float)(Math.PI / 2.0)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(pastPositions[pastGtc], 0, 0) * transforms[mesh.ParentBone.Index];
                    }
                    mesh.Draw();
                }
            }

            cubeEffect.EnableDefaultLighting();
            cubeEffect.World = Matrix.Identity;
            cubeEffect.View = view;
            cubeEffect.Projection = projection;
            cubeEffect.TextureEnabled = true;
            cubeEffect.Texture = cube.shapeTexture;

            cubeEffect.Begin();

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                cube.RenderShape(GraphicsDevice);

                pass.End();
            }

            cubeEffect.End();

            cubeEffect.World = Matrix.CreateTranslation(door1.Position.X, door1.IsOpen ? 4 : 1, 0);
            cubeEffect.Texture = door.shapeTexture;
            cubeEffect.Begin();

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                door.RenderShape(GraphicsDevice);

                pass.End();
            }

            cubeEffect.End();

            cubeEffect.World = Matrix.CreateTranslation(door2.Position.X, door2.IsOpen ? 4 : 1, 0);
            cubeEffect.Texture = door.shapeTexture;
            cubeEffect.Begin();

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                door.RenderShape(GraphicsDevice);

                pass.End();
            }

            cubeEffect.End();
        }
    }
}
