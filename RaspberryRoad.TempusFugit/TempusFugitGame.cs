using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RaspberryRoad.TempusFugit
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TempusFugitGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model playerModel;
        Model groundModel;
        Model timeTravelSphereModel;
        Model doorModel;
        SpriteFont font;

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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Kootenay");

            doorModel = Content.Load<Model>("door");
            playerModel = Content.Load<Model>("dude");
            groundModel = Content.Load<Model>("ground");
            timeTravelSphereModel = Content.Load<Model>("timetravelsphere");

            ResetWorld();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.F1))
                ResetWorld();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (!level.Update(dt, keyboardState))
                ResetWorld();

            foreach (var effect in specialEffects)
                effect.Update(dt);

            specialEffects.RemoveAll(x => !x.IsActive());

            base.Update(gameTime);
        }

        private void ResetWorld()
        {
            specialEffects = new List<SpecialEffect>();

            level = new Level();
            // TODO: Don't pass all the models the level needs through here
            level.Reset(specialEffects, timeTravelSphereModel, playerModel, groundModel, doorModel);
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
            spriteBatch.DrawString(font, level.Time.GlobalTimeCoordinate.ToString() + ", " + level.TargetGtc.ToString(), Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawWorld()
        {
            // TODO: Abstract away into a Camera class
            float aspectRatio = graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(level.PresentPlayer.Position.X, 2, 30), new Vector3(level.PresentPlayer.Position.X, 2, 0), Vector3.Up);

            foreach (var player in level.GetActivePlayers())
                DrawAnimatedModel(player.Model, projection, view, player.GetMatrix(), player.GetColor());

            foreach (var entity in level.GetEntities())
                DrawModel(entity.Model, projection, view, entity.GetMatrix(), Vector3.Zero, 1);

            EnableTransparency();

            foreach (var effect in specialEffects)
                DrawModel(effect.GetModel(), projection, view, effect.GetMatrix(), Vector3.One, effect.GetAlpha());

            DisableTransparency();
        }

        private void EnableTransparency()
        {
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add; // add source and dest results
        }

        private void DisableTransparency()
        {
            graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
        }

        private void DrawModel(Model model, Matrix projection, Matrix view, Matrix world, Vector3 ambientColor, float alpha)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Alpha = alpha;
                    effect.LightingEnabled = true;
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = new Vector3(0.15f, 0.1f, 0.2f);
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.Direction = new Vector3(0.5f, -1, -0.2f);
                    effect.DirectionalLight0.DiffuseColor = new Vector3(1, 0.9f, 0.6f);
                    effect.DirectionalLight0.SpecularColor = new Vector3(1, 1, 1);
                    effect.DirectionalLight1.Enabled = true;
                    effect.DirectionalLight1.Direction = new Vector3(-0.3f, 1, 0.1f);
                    effect.DirectionalLight1.DiffuseColor = new Vector3(1, 0.4f, 0.1f);
                    effect.DirectionalLight1.SpecularColor = new Vector3(1, 1, 1);
                    effect.EmissiveColor = ambientColor;
                    effect.SpecularColor = new Vector3(0.01f, 0.01f, 0.1f);
                    effect.SpecularPower = 175;
                    effect.Projection = projection;
                    effect.View = view;
                    effect.World = world;
                }
                mesh.Draw();
            }
        }

        private void DrawAnimatedModel(AnimatedModel model, Matrix projection, Matrix view, Matrix world, Vector3 ambientColor)
        {
            Matrix[] bones = model.AnimationPlayer.GetSkinTransforms();

            foreach (ModelMesh mesh in model.Model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["Bones"].SetValue(bones);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["DiffuseColor"].SetValue(ambientColor);
                }
                mesh.Draw();
            }
        }
    }
}
