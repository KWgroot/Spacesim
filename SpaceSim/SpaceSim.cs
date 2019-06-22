#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace SpaceSim
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceSim : Game
    {
        GraphicsDeviceManager graphDev;
        Color background = new Color(2, 0, 6);
        public static SpaceSim World;
        Random random = new Random();
        Vector3 cameraPosition = new Vector3(0f, 30f, 80f);
        Vector3 cameraLookAt = new Vector3(0f, 0f, 0f);
        Matrix cameraOrientationMatrix = Matrix.Identity, rotMatrix;
        public Matrix View;
        public Matrix Projection;
        public static GraphicsDevice Graphics;
        Vector2 distanceFromCenter;

        List<Sphere> spheres;
        List<Bullet> bullets;

        Sphere sun, earth, mars, jupiter, saturn, uranus, moon;

        Spaceship spaceship;
        Vector3 spaceshipPosition = new Vector3(0f, 28f, 77f);
        Matrix spaceshipOrientationMatrix = Matrix.CreateFromYawPitchRoll(0f, -0.17f, 0f);
        Vector3 spaceshipFollowPoint = new Vector3(0f, 0.09f, 0.2f);
        Vector3 spaceshipLookAtPoint = new Vector3(0f, 0.05f, 0f);
        Vector3 bulletSpawnPosition = new Vector3(0f, 0f, -0.1f);

        Skybox skybox;

        SpriteBatch spriteBatch;
        Texture2D reticle, controls;
        Point mousePosition;
        bool wKeyDown, aKeyDown, sKeyDown, dKeyDown;
        bool mouseButton, mouseDown, lastMouseButton;
        bool bulRemove;
        int bulIndex;
        float reticleHalfWidth, reticleHalfHeight, rollFactor = 0f, shipVelocity, dragFactor = 0.8f;

        Vector2 screenCenter;

        public SpaceSim()
            : base()
        {
            Content.RootDirectory = "Content";

            World = this;
            graphDev = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            Graphics = GraphicsDevice;

#if DEBUG
            graphDev.PreferredBackBufferWidth = 1600;
            graphDev.PreferredBackBufferHeight = 900;
            graphDev.IsFullScreen = false;
#else
            graphDev.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphDev.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphDev.IsFullScreen = true;
#endif
            graphDev.ApplyChanges();

            screenCenter = new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);

            SetupCamera(true);
            Window.Title = "HvA - Simulation & Physics - Opdracht 6 - SpaceSim";
            spriteBatch = new SpriteBatch(Graphics);

            spheres = new List<Sphere>();
            bullets = new List<Bullet>();

            spheres.Add(sun = new Sphere(Matrix.Identity, Color.Yellow, 30, 0, 2));
            spheres.Add(earth = new Sphere(Matrix.Identity, Color.DeepSkyBlue, 30, 16, 1, random.Next(0, 360), 0.15f));
            spheres.Add(mars = new Sphere(Matrix.Identity, Color.Red, 30, 21, 0.6f, random.Next(0, 360), 0.2375f));
            spheres.Add(jupiter = new Sphere(Matrix.Identity, Color.Orange, 30, 27, 1.7f, random.Next(0, 360), 0.325f));
            spheres.Add(saturn = new Sphere(Matrix.Identity, Color.Khaki, 30, 36, 1.6f, random.Next(0, 360), 0.4125f));
            spheres.Add(uranus = new Sphere(Matrix.Identity, Color.Cyan, 30, 43, 1.5f, random.Next(0, 360), 0.5f));
            spheres.Add(moon = new Sphere(earth.Transform, Color.LightGray, 30, 2, 0.5f, 0, 1.5f));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            spaceship = new Spaceship(spaceshipOrientationMatrix * Matrix.CreateTranslation(spaceshipPosition), Content);
            skybox = new Skybox(Matrix.CreateScale(1000f) * Matrix.CreateTranslation(cameraPosition), Content);
            reticle = Content.Load<Texture2D>("Reticle");
            reticleHalfWidth = reticle.Width / 2f;
            reticleHalfHeight = reticle.Height / 2f;
            controls = Content.Load<Texture2D>("Controls");

            IsMouseVisible = false;
        }

        private void SetupCamera(bool initialize = false)
        {
            View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraOrientationMatrix.Up);
            if (initialize) Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SpaceSim.World.GraphicsDevice.Viewport.AspectRatio, 0.1f, 2000.0f);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(background);

            SetupCamera();

            skybox.Draw();

            foreach (Sphere sphere in spheres)
            {
                sphere.Draw();
            }

            foreach (Sphere bullet in bullets)
            {
                bullet.Draw();
            }

            spaceship.Draw();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(reticle, new Vector2(mousePosition.X - reticleHalfWidth, mousePosition.Y - reticleHalfHeight), Color.White);
            spriteBatch.Draw(controls, new Vector2(10f, 10f), Color.White);
            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            cameraPosition = Vector3.Transform(spaceshipFollowPoint, spaceship.Transform);
            cameraLookAt = Vector3.Transform(spaceshipLookAtPoint, spaceship.Transform);
            cameraOrientationMatrix = spaceshipOrientationMatrix;

            // Helpers for input
            KeyboardState keyboard = Keyboard.GetState();
            wKeyDown = keyboard.IsKeyDown(Keys.W);
            aKeyDown = keyboard.IsKeyDown(Keys.A);
            sKeyDown = keyboard.IsKeyDown(Keys.S);
            dKeyDown = keyboard.IsKeyDown(Keys.D);
            if (keyboard.IsKeyDown(Keys.Escape)) Exit();
            MouseState mouse = Mouse.GetState();
            mousePosition = mouse.Position;
            mouseButton = mouse.LeftButton == ButtonState.Pressed;
            mouseDown = mouseButton && !lastMouseButton;
            lastMouseButton = mouseButton;

            skybox.Transform = Matrix.CreateScale(1000f) * Matrix.CreateTranslation(cameraPosition);

            distanceFromCenter.X = (screenCenter.X - mousePosition.X) / 15000;
            distanceFromCenter.Y = (screenCenter.Y - mousePosition.Y) / 15000;

            if (aKeyDown)
            {
                rollFactor -= .5f;
            }
            if (dKeyDown)
            {
                rollFactor += .5f;
            }
            rollFactor *= dragFactor;
            MathHelper.Clamp(rollFactor, -200, 200);

            if (wKeyDown)
            {
                shipVelocity += 2f;
            }
            if (sKeyDown)
            {
                shipVelocity -= 2f;
            }
            shipVelocity *= dragFactor;
            MathHelper.Clamp(shipVelocity, -100, 100);

            spaceship.Transform *= shipVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            spaceshipPosition += spaceship.Transform.Forward;

            RotateOrientationMatrixByYawPitchRoll(ref spaceshipOrientationMatrix, distanceFromCenter.X, distanceFromCenter.Y, rollFactor * (float)gameTime.ElapsedGameTime.TotalSeconds);
            spaceship.Transform = spaceshipOrientationMatrix * Matrix.CreateTranslation(spaceshipPosition);

            foreach (Sphere sphere in spheres)
            {
                if (sphere != moon)
                {
                    rotMatrix = Matrix.Identity;
                    rotMatrix *= Matrix.CreateRotationY(MathHelper.ToRadians(sphere.rotSpeed));
                    sphere.Transform *= rotMatrix;
                }
                else
                {
                    rotMatrix = Matrix.Identity;
                    rotMatrix = Matrix.CreateScale(0.5f);
                    rotMatrix *= Matrix.CreateTranslation(2, 0, 0);
                    rotMatrix *= Matrix.CreateRotationY(sphere.rotSpeed * (float)gameTime.TotalGameTime.TotalSeconds);
                    rotMatrix *= Matrix.CreateRotationZ(MathHelper.PiOver4);
                    rotMatrix *= earth.Transform;
                    sphere.Transform = rotMatrix;
                }
            }

            bulletSpawnPosition = Vector3.Transform(spaceshipLookAtPoint, spaceship.Transform);
            if (mouseDown && mouseButton)
            {
                bullets.Add(new Bullet(bulletSpawnPosition, spaceship.Transform.Forward, spaceship.Transform.Down));
            }

            foreach (Bullet thisBullet in bullets)
            {
                thisBullet.Transform *= Matrix.CreateTranslation(spaceship.Transform.Forward / 5);
                if (thisBullet.startPos.Length() - new Vector3(thisBullet.Transform.M41, thisBullet.Transform.M42, thisBullet.Transform.M43).Length() > 200)
                {
                    bulIndex = bullets.IndexOf(thisBullet);
                    bulRemove = true;
                }
            }

            if (bulRemove)
            {
                bullets.RemoveAt(bulIndex);
                bulRemove = false;
            }


            base.Update(gameTime);
        }

        static void RotateOrientationMatrixByYawPitchRoll(ref Matrix matrix, float yawChange, float pitchChange, float rollChange)
        {
            if (rollChange != 0f || yawChange != 0f || pitchChange != 0f)
            {
                Vector3 pitch = matrix.Right * pitchChange;
                Vector3 yaw = matrix.Up * yawChange;
                Vector3 roll = matrix.Forward * rollChange;

                Vector3 overallOrientationChange = pitch + yaw + roll;
                float overallAngularChange = overallOrientationChange.Length();
                Vector3 overallRotationAxis = Vector3.Normalize(overallOrientationChange);
                Matrix orientationChange = Matrix.CreateFromAxisAngle(overallRotationAxis, overallAngularChange);
                matrix *= orientationChange;
            }
        }
    }
}
