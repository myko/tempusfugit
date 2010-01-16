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
        Vector3 position = new Vector3(0, 20, 0);
        Vector3 velocity = new Vector3(0, 0, 0);
        Vector3 lineStart = new Vector3(-5, 0, 0);
        Vector3 lineEnd = new Vector3(5, 0, 0);
        BasicEffect cubeEffect;
        BasicShape cube;
        BasicShape cube2;

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
            // TODO: Add your initialization logic here

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

            cube = new BasicShape(new Vector3(1, 1, 1), new Vector3(0, 0, 0));
            cube.shapeTexture = Content.Load<Texture2D>("head");
            cube2 = new BasicShape(new Vector3(1, 1, 1), new Vector3(2, 1, 0));
            cube2.shapeTexture = Content.Load<Texture2D>("pants");

            model = Content.Load<Model>("dude");
            // TODO: use this.Content to load your game content here
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Right))
                velocity.X = 1f;

            if (state.IsKeyDown(Keys.Left))
                velocity.X = -1f;

            if (state.IsKeyDown(Keys.F1))
            {
                position = new Vector3(0, 5, 0);
                velocity = Vector3.Zero;
            }

            velocity = velocity - (velocity * 0.95f * (float)gameTime.ElapsedGameTime.TotalSeconds);

            var newPosition = position + velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            BoundingBox b = new BoundingBox(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));
            BoundingBox b2 = new BoundingBox(new Vector3(1f, 0f, -1f), new Vector3(3f, 2f, 1f));
            BoundingSphere s = new BoundingSphere(newPosition + new Vector3(0, 0.01f, 0), 0.04f);


            if (b.Intersects(s) || b2.Intersects(s))
            {
                velocity.Y = 0;
                if (state.IsKeyDown(Keys.Up))
                    velocity.Y = 5f;
            }
            else
            {
                position = newPosition;
                velocity = velocity + new Vector3(0f, -9.82f, 0f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawModel();

            base.Draw(gameTime);
        }

        private void DrawModel()
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            float aspectRatio = graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            //projection = Matrix.CreateOrthographic(10, 10, 1, 10000);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 2, 10), new Vector3(0, 2, 0), Vector3.Up);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = Matrix.CreateRotationY((float)(Math.PI / 2.0)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(position) * transforms[mesh.ParentBone.Index];
                }
                mesh.Draw();
            }

            cubeEffect.World = Matrix.Identity;
            cubeEffect.View = view;
            cubeEffect.Projection = projection;
            cubeEffect.TextureEnabled = true;

            cubeEffect.Begin();

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                cubeEffect.Texture = cube.shapeTexture;
                cube.RenderShape(GraphicsDevice);

                pass.End();
            }

            cubeEffect.End();

            cubeEffect.Begin();

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                cubeEffect.Texture = cube2.shapeTexture;
                cube2.RenderShape(GraphicsDevice);

                pass.End();
            }

            cubeEffect.End();
        }
    }
}
