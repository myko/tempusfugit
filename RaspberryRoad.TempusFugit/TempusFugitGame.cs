using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RaspberryRoad.TempusFugit
{
    public class TempusFugitGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        SpriteFont font;
        Dictionary<string, Model> models = new Dictionary<string, Model>();

        Level level;
        
        List<SpecialEffect> specialEffects;

        BasicEffect lineEffect;

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
            lineEffect = new BasicEffect(graphics.GraphicsDevice);
            lineEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);

            base.Initialize();
        }

        private void LoadModel(string name)
        {
            models.Add(name, Content.Load<Model>(name));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Kootenay");

            LoadModel("door");
            LoadModel("dude");
            LoadModel("ground");
            LoadModel("timetravelsphere");
            LoadModel("travelpad");
            LoadModel("button");

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

            level = new Level(specialEffects, models);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawWorld();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, level.Time.GlobalTimeCoordinate.ToString() + ", " + level.TargetGtc.ToString(), Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, level.PresentPlayer.Position.ToString(), new Vector2(0, 16f), Color.White);
            spriteBatch.DrawString(font, level.PresentPlayer.Velocity.ToString(), new Vector2(0, 32f), Color.White);
            spriteBatch.DrawString(font, (level.PresentPlayer.IsGrounded ? "IsGrounded " : ""), new Vector2(0, 48f), Color.White);
            spriteBatch.DrawString(font, (level.PresentPlayer.IsFalling ? "IsFalling" : ""), new Vector2(0, 64f), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawWorld()
        {
            // TODO: Abstract away into a Camera class
            float aspectRatio = graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(level.PresentPlayer.Position.X, 1, 15), new Vector3(level.PresentPlayer.Position.X, 1, 0), Vector3.Up);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var player in level.GetActivePlayers())
                DrawAnimatedModel(player.Model, projection, view, player.GetMatrix(), player.GetColor());

            foreach (var entity in level.GetEntities())
                DrawModel(entity.Model, projection, view, entity.GetMatrix(), Vector3.Zero, 1);

            EnableTransparency();

            foreach (var effect in specialEffects)
                DrawModel(effect.GetModel(), projection, view, effect.GetMatrix(), Vector3.One, effect.GetAlpha());

            DrawCollisionGeometry(projection, view);

            DisableTransparency();
        }

        private void DrawCollisionGeometry(Matrix projection, Matrix view)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            lineEffect.World = Matrix.Identity;
            lineEffect.View = view;
            lineEffect.Projection = projection;

            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var lines = level.GetLineVertices().ToArray();

                graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.LineList, lines, 0, lines.Length / 2);
            }

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void EnableTransparency()
        {
            graphics.GraphicsDevice.BlendState = BlendState.Additive;
        }

        private void DisableTransparency()
        {
            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
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
