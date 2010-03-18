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
using SkinnedModel;

namespace RaspberryRoad.TempusFugit
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TempusFugitGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model model;
        Model ground;
        Model doorFrame;
        Model timeTravelSphere;
        Model door;
        SpriteFont font;

        Time time;

        Level level;
        
        List<SpecialEffect> specialEffects;

        public TempusFugitGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
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

            door = Content.Load<Model>("door");
            model = Content.Load<Model>("dude");
            ground = Content.Load<Model>("ground");
            doorFrame = Content.Load<Model>("doorframe");
            font = Content.Load<SpriteFont>("Kootenay");

            timeTravelSphere = Content.Load<Model>("timetravelsphere");

            ResetWorld();
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
            time.UpdateGameTime(dt);

            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.F1))
            {
                ResetWorld();
            }

            if (state.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (!level.Update(dt, time, state))
                ResetWorld();

            foreach (var effect in specialEffects)
            {
                effect.Update(dt);
            }
            specialEffects.RemoveAll(x => !x.IsActive());

            base.Update(gameTime);
        }

        private void ResetWorld()
        {
            time = new Time();

            specialEffects = new List<SpecialEffect>();

            level = new Level();
            level.Reset(specialEffects, timeTravelSphere, model, time);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawWorld();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.DrawString(font, time.GlobalTimeCoordinate.ToString() + ", " + level.TargetGtc.ToString(), Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawWorld()
        {
            float aspectRatio = graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(level.PresentPlayer.Position.X, 2, 30), new Vector3(level.PresentPlayer.Position.X, 2, 0), Vector3.Up);

            DrawPlayer(level.PresentPlayer, projection, view);
            DrawPlayer(level.FuturePlayer, projection, view);
            DrawPlayer(level.PastPlayer, projection, view);

            // TODO: Don't hard code stuff in a level to draw
            DrawModel(ground, projection, view, Matrix.Identity, Vector3.Zero, 1f);
            DrawModel(door, projection, view, level.Door1.GetMatrix(), Vector3.Zero, 1);
            DrawModel(door, projection, view, level.Door2.GetMatrix(), Vector3.Zero, 1);

            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add; // add source and dest results

            foreach (var effect in specialEffects)
            {
                DrawModel(effect.GetModel(), projection, view, effect.GetMatrix(), new Vector3(1, 1, 1), effect.GetAlpha());
            }

            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

        }

        private void DrawModel(Model model, Matrix projection, Matrix view, Matrix world, Vector3 ambientColor, float alpha)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Alpha = alpha;
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = ambientColor;
                    effect.Projection = projection;
                    effect.View = view;
                    effect.World = world;
                }
                mesh.Draw();
            }
        }

        private void DrawPlayer(Player player, Matrix projection, Matrix view)
        {
            Matrix[] bones = player.AnimationPlayer.GetSkinTransforms();
            
            if (player.Exists)
            {
                foreach (ModelMesh mesh in player.Model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.Parameters["Bones"].SetValue(bones);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["World"].SetValue(Matrix.CreateRotationY((float)(Math.PI / 2.0 * player.Rotation)) * Matrix.CreateScale(0.025f) * Matrix.CreateTranslation(player.Position.X, 0, 0));
                    }
                    mesh.Draw();
                }
            }
        }
    }
}
