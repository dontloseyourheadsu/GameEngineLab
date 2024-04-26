using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoDinoGrr.Physics;
using MonoDinoGrr.WorldObjects;
using System;
using System.Collections.Generic;

namespace MonoDinoGrr.Rendering
{
    public class Render
    {
        public SpriteBatch SpriteBatch;
        public Camera Camera;
        public Texture2D goalTexture;
        public SpriteFont font;
        public Texture2D platform;
        public Texture2D blankTexture;
        public Texture2D heartTexture;
        public Texture2D brokenHeartTexture;
        public MonoCamera monoCamera;
        public int Width;
        public int Height;

        public Render(SpriteBatch spriteBatch, Camera camera, ContentManager Content)
        {
            SpriteBatch = spriteBatch;
            Camera = camera;
            goalTexture = Content.Load<Texture2D>("goal");
            font = Content.Load<SpriteFont>("File");
            platform = Content.Load<Texture2D>("platform");
            blankTexture = Content.Load<Texture2D>("white");
            heartTexture = Content.Load<Texture2D>("dino-heart");
            brokenHeartTexture = Content.Load<Texture2D>("dino-heart-broken");
            monoCamera = new MonoCamera(Vector2.Zero);
        }

        public void DrawParticle(Particle particle)
        {
            var worldPosition = new Point((int)(particle.Position.X - particle.Mass), (int)(particle.Position.Y - particle.Mass));
            var viewPosition = Camera.TranslateToView(worldPosition);

            if (Camera.IsVisible(worldPosition))
            {
                // Using a placeholder texture for the particle
                Texture2D texture = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
                texture.SetData(new[] { Color.Orange });

                // Drawing particle as a scaled ellipse (circle for simplicity)
                SpriteBatch.Draw(texture, new Rectangle(viewPosition.X, viewPosition.Y, (int)(particle.Mass * 2 * Camera.Scale), (int)(particle.Mass * 2 * Camera.Scale)), Color.Orange);
            }
        }

        public void DrawStick(Stick stick)
        {
            var viewPositionA = Camera.TranslateToView(new Point((int)stick.A.Position.X, (int)stick.A.Position.Y));
            var viewPositionB = Camera.TranslateToView(new Point((int)stick.B.Position.X, (int)stick.B.Position.Y));

            // Drawing a line, note MonoGame doesn't support drawing lines directly so we use a small rectangle or a 1px texture stretched
            Texture2D texture = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Orange });
            float angle = (float)Math.Atan2(viewPositionB.Y - viewPositionA.Y, viewPositionB.X - viewPositionA.X);
            float length = Vector2.Distance(new Vector2(viewPositionA.X, viewPositionA.Y), new Vector2(viewPositionB.X, viewPositionB.Y));

            SpriteBatch.Draw(texture, new Rectangle(viewPositionA.X, viewPositionA.Y, (int)length, (int)(stick.A.Mass * Camera.Scale)), null, Color.Orange, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        public void DrawDinosaur(Dinosaur dinosaur, List<Texture2D> dinoImages)
        {
            var worldPosition = new Point((int)dinosaur.formKeeper.Center.X, (int)dinosaur.formKeeper.Center.Y);
            var viewPosition = Camera.TranslateToView(worldPosition);

            var image = dinoImages.Find(x => x.Name == dinosaur.image);

            if (Camera.IsVisible(worldPosition))
            {
                var width = image.Width * Camera.Scale / image.Width;
                var height = image.Height * Camera.Scale / image.Height;

                SpriteBatch.Draw(image, new Rectangle(viewPosition.X - width / 2, viewPosition.Y - height / 2, width, height), null, Color.White, 0f, Vector2.Zero, dinosaur.Orientation == Orientation.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
        }


        public void DrawPolygon(Polygon polygon)
        {
            foreach (Stick stick in polygon.sticks)
            {
                DrawStick(stick);
            }
        }

        public void DrawRaycast(Vector2 start, Vector2 end)
        {
            var viewStart = Camera.TranslateToView(new Point((int)start.X, (int)start.Y));
            var viewEnd = Camera.TranslateToView(new Point((int)end.X, (int)end.Y));

            Texture2D texture = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Red });

            float angle = (float)Math.Atan2(viewEnd.Y - viewStart.Y, viewEnd.X - viewStart.X);
            float length = Vector2.Distance(new Vector2(viewStart.X, viewStart.Y), new Vector2(viewEnd.X, viewEnd.Y));

            SpriteBatch.Draw(texture, new Rectangle(viewStart.X, viewStart.Y, (int)length, 1), null, Color.Red, angle, Vector2.Zero, SpriteEffects.None, 0);

            // Drawing the endpoint as a small red circle
            SpriteBatch.Draw(texture, new Rectangle(viewStart.X - 5, viewStart.Y - 5, 10, 10), Color.Red);
        }

        public void DrawBounds(int width, int height)
        {
            Texture2D lineTexture = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
            lineTexture.SetData(new[] { Color.Black });

            var viewTopLeft = Camera.TranslateToView(new Point(0, 0));
            var viewTopRight = Camera.TranslateToView(new Point(width, 0));
            var viewBottomLeft = Camera.TranslateToView(new Point(0, height));
            var viewBottomRight = Camera.TranslateToView(new Point(width, height));

            // Draw each boundary line
            DrawLine(lineTexture, viewTopLeft, viewTopRight);
            DrawLine(lineTexture, viewTopRight, viewBottomRight);
            DrawLine(lineTexture, viewBottomRight, viewBottomLeft);
            DrawLine(lineTexture, viewBottomLeft, viewTopLeft);
        }

        private void DrawLine(Texture2D texture, Point start, Point end)
        {
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
            float length = Vector2.Distance(new Vector2(start.X, start.Y), new Vector2(end.X, end.Y));

            SpriteBatch.Draw(texture, new Rectangle(start.X, start.Y, (int)length, 1), null, Color.Black, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        public void DrawBackground(Background background)
        {
            var viewPosition1 = Camera.TranslateToView(new Point((int)background.l1_X1, 50));
            var backgroundTexture = background.layer1; // Assume layer1 is already a Texture2D

            SpriteBatch.Draw(backgroundTexture, new Rectangle(viewPosition1.X - 200, viewPosition1.Y, backgroundTexture.Width, background.height), Color.White);
        }

        public void DrawProgressBar(int width, int max, float value)
        {
            var viewPosition = Camera.TranslateToView(new Point(width - 200 - 20, 20));
            Texture2D texture = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });

            SpriteBatch.Draw(texture, new Rectangle(viewPosition.X, viewPosition.Y, 200, 20), Color.White);
            
            SpriteBatch.Draw(texture, new Rectangle(viewPosition.X, viewPosition.Y, (int)(200 * value / max), 20), Color.Green);
        }

        public void DrawHearts(bool[] lifeHearts, Texture2D heartTexture, Texture2D brokenHeartTexture)
        {
            int x = 20;
            int y = 20;
            int size = 30; // Adjusted for scaling

            for (int i = 0; i < lifeHearts.Length; i++)
            {
                var viewPosition = Camera.TranslateToView(new Point(x, y));
                SpriteBatch.Draw(lifeHearts[i] ? heartTexture : brokenHeartTexture, new Rectangle(viewPosition.X, viewPosition.Y, size, size), Color.White);
                x += size + 10;
            }
        }

        public void DrawGoal(Goal goal, Texture2D goalTexture)
        {
            var viewPosition = Camera.TranslateToView(new Point((int)goal.Position.X, (int)goal.Position.Y));
            int width = goal.Width * Camera.Scale;
            int height = goal.Height * Camera.Scale;
            SpriteBatch.Draw(goalTexture, new Rectangle(viewPosition.X - width / 2, viewPosition.Y - height / 2, width, height), Color.White);
        }

        public void DrawText(string text, SpriteFont font, int width, int height, bool isWin)
        {
            var viewCenter = Camera.TranslateToView(new Point(width / 2, height / 2));
            var position = new Vector2(viewCenter.X - font.MeasureString(text).X / 2, viewCenter.Y - font.MeasureString(text).Y / 2);
            SpriteBatch.DrawString(font, text, position, isWin ? Color.Green : Color.Red);
        }

        public void DrawPlatform(Texture2D texture, Vector2 position, int width, int height)
        {
            var viewPosition = Camera.TranslateToView(new Point((int)position.X, (int)position.Y));
            int drawWidth = width * Camera.Scale;
            int drawHeight = height * Camera.Scale;
            SpriteBatch.Draw(texture, new Rectangle(viewPosition.X, viewPosition.Y, drawWidth, drawHeight), Color.White);
        }

        public void DrawPlayer(Player player, int cntT, Texture2D[] runningImages, Texture2D standingImage, Texture2D hitImage)
        {
            Texture2D image = standingImage;
            if (player.isDamaged && cntT % 30 < 15)
            {
                image = hitImage;
            }
            else if (player.isRunning)
            {
                image = runningImages[player.runningCntT % runningImages.Length];
                if (cntT % 3 == 0) player.runningCntT++;
            }

            var width = (int)((player.polygon.particles[1].Position.X - player.polygon.particles[0].Position.X) * Camera.Scale);
            var height = (int)((player.polygon.particles[3].Position.Y - player.polygon.particles[0].Position.Y) * Camera.Scale);
            var centerView = Camera.TranslateToView(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y));

            SpriteEffects spriteEffect = player.Orientation == Orientation.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (Camera.IsVisible(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y)))
            {
                SpriteBatch.Draw(image, new Rectangle(centerView.X - width / 2, centerView.Y - height / 2, width, height), null, Color.White, 0, Vector2.Zero, spriteEffect, 0);
            }
        }

        public void DrawOutsideBounds(Texture2D blankTexture, int Width, int Height)
        {
            var spriteBatch = SpriteBatch;
            var camera = Camera;

            var viewTopLeft = camera.TranslateToView(new Point(0, 0));
            var viewTopRight = camera.TranslateToView(new Point(Width, 0));
            var viewBottomLeft = camera.TranslateToView(new Point(0, Height));
            var extension = 200;

            // Drawing the extensions beyond the visible game area
            spriteBatch.Draw(blankTexture, new Rectangle(viewTopLeft.X - extension, viewTopLeft.Y, extension, Height + extension), Color.White);
            spriteBatch.Draw(blankTexture, new Rectangle(viewTopRight.X, viewTopRight.Y, extension, Height + extension), Color.White);
            spriteBatch.Draw(blankTexture, new Rectangle(viewBottomLeft.X - extension, viewBottomLeft.Y, Width + extension * 2, extension), Color.White);
        }

        public void DrawGirl(Player player, int cntT, Texture2D[] runningImages, Texture2D standingImage, Texture2D hitImage)
        {
            Texture2D image = standingImage;
            if (player.isDamaged && cntT % 30 < 15)
            {
                image = hitImage;
            }
            else if (player.isRunning)
            {
                image = runningImages[player.runningCntT % runningImages.Length];
                if (cntT % 3 == 0) player.runningCntT++;
            }

            int width = (int)((player.polygon.particles[1].Position.X - player.polygon.particles[0].Position.X) * Camera.Scale);
            int height = (int)((player.polygon.particles[3].Position.Y - player.polygon.particles[0].Position.Y) * Camera.Scale);
            var centerView = Camera.TranslateToView(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y));

            SpriteEffects spriteEffect = player.Orientation == Orientation.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (Camera.IsVisible(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y)))
            {
                SpriteBatch.Draw(image, new Rectangle(centerView.X - width / 2, centerView.Y - height / 2, width, height), null, Color.White, 0, Vector2.Zero, spriteEffect, 0);
            }
        }

        public void DrawWin(SpriteFont font, int Width, int Height)
        {
            string message = "You Win!";
            var viewCenter = Camera.TranslateToView(new Point(Width / 2, Height / 2));
            Vector2 textSize = font.MeasureString(message);
            Vector2 position = new Vector2(viewCenter.X - textSize.X / 2, viewCenter.Y - textSize.Y / 2);

            SpriteBatch.DrawString(font, message, position, Color.Green);
        }

        public void DrawLoose(SpriteFont font, int Width, int Height)
        {
            string message = "You Lose!";
            var viewCenter = Camera.TranslateToView(new Point(Width / 2, Height / 2));
            Vector2 textSize = font.MeasureString(message);
            Vector2 position = new Vector2(viewCenter.X - textSize.X / 2, viewCenter.Y - textSize.Y / 2);

            SpriteBatch.DrawString(font, message, position, Color.Red);
        }
    }
}
