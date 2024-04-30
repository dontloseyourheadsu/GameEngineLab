using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoDinoGrr.Physics;
using MonoDinoGrr.Rendering;
using MonoDinoGrr.WorldGen;
using MonoDinoGrr.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoDinoGrr
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        WorldGenerator worldGenerator;
        Camera camera;
        Song song;

        public Texture2D goalTexture;
        public SpriteFont font;
        public Texture2D blankTexture;
        public Texture2D heartTexture;
        public Texture2D brokenHeartTexture;
        public Texture2D border;
        public Texture2D mountains;
        public Texture2D plantsBackground;

        public Texture2D standingImage;
        public Texture2D[] runningImages;
        public Texture2D hitImage;

        List<Texture2D> dinosaurTextures = new List<Texture2D>();

        int cntT = 0;
        int windowWidth;
        int windowHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;

            int w = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int h = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            int div = 1;

            _graphics.PreferredBackBufferWidth = w / div;
            _graphics.PreferredBackBufferHeight = h / div;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            windowWidth = GraphicsDevice.Viewport.Width;
            windowHeight = GraphicsDevice.Viewport.Height;

            worldGenerator = new WorldGenerator();
            var levelDinosaurs = worldGenerator.GetLevelDinosaurs();
            var levelPlatforms = worldGenerator.GetLevelPlatforms();
            var levelGoal = worldGenerator.GetLevelGoal();
            var levelPlayer = worldGenerator.GetLevelPlayer();
            var worldWidth = worldGenerator.GetLevelWidth();
            var worldHeight = windowHeight;

            var platforms = new List<Platform>();
            for (int i = 0; i < levelPlatforms.Count; i++)
            {
                LevelPlatform? levelPlatform = levelPlatforms[i];
                platforms.Add(new Platform(new Vector2(levelPlatform.X, levelPlatform.Y), levelPlatform.Width, levelPlatform.Height));
            }

            var goal = new Goal(new Vector2(levelGoal.X, levelGoal.Y));

            var player = new Player(new Vector2(levelPlayer.X, levelPlayer.Y));

            var dinosaurs = new List<Dinosaur>();
            for (int i = 0; i < levelDinosaurs.Count; i++)
            {
                LevelDinosaur? levelDinosaur = levelDinosaurs[i];
                dinosaurs.Add(new Dinosaur(levelDinosaur.X, levelDinosaur.Y, levelDinosaur.Width, levelDinosaur.Height, levelDinosaur.Dino, player));
            }

            var background = new Background(windowWidth, windowHeight, Content);
            
            camera = new Camera(player, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), 1);
            
            worldGenerator.physicWorld = new PhysicWorld(worldWidth, worldHeight, dinosaurs, platforms, player, goal, background, camera);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            song = Content.Load<Song>("song");
            MediaPlayer.Play(song);

            goalTexture = Content.Load<Texture2D>("goal");
            font = Content.Load<SpriteFont>("File");
            blankTexture = Content.Load<Texture2D>("white");
            heartTexture = Content.Load<Texture2D>("dino-heart");
            brokenHeartTexture = Content.Load<Texture2D>("dino-heart-broken");
            border = Content.Load<Texture2D>("border");
            mountains = Content.Load<Texture2D>("mountains");
            plantsBackground = Content.Load<Texture2D>("plants-background");

            standingImage = Content.Load<Texture2D>("madoka-standing-0");
            runningImages = new Texture2D[8]
            {
                Content.Load<Texture2D>("madoka-running-0"),
                Content.Load<Texture2D>("madoka-running-1"),
                Content.Load<Texture2D>("madoka-running-2"),
                Content.Load<Texture2D>("madoka-running-3"),
                Content.Load<Texture2D>("madoka-running-4"),
                Content.Load<Texture2D>("madoka-running-5"),
                Content.Load<Texture2D>("madoka-running-6"),
                Content.Load<Texture2D>("madoka-running-7")
            };
            hitImage = Content.Load<Texture2D>("madoka-hit-0");

            var levelDinosaurs = worldGenerator.GetLevelDinosaurs();
            
            var dinoImages = levelDinosaurs.Select(x => x.Dino).Distinct().ToList();
            for (int i = 0; i < dinoImages.Count(); i++)
            {
                dinosaurTextures.Add(Content.Load<Texture2D>(dinoImages[i]));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            worldGenerator.physicWorld.Update(cntT);
            cntT++;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            DrawBackground();
            DrawOutsideBounds();
            DrawBounds();

            for (int i = 0; i < worldGenerator.physicWorld.platforms.Count; i++)
            {
                Platform? platform = worldGenerator.physicWorld.platforms[i];
                DrawPlatform(platform.Position, platform.Width, platform.Height);
            }

            DrawPlayer();

            for (int i = 0; i < worldGenerator.physicWorld.dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = worldGenerator.physicWorld.dinosaurs[i];
                DrawDinosaur(dinosaur);
            }

            DrawGoal(worldGenerator.physicWorld.goal, goalTexture);

            if (worldGenerator.physicWorld.player.dinoPencil.NewPolygon != null)
            {
                DrawPolygon(worldGenerator.physicWorld.player.dinoPencil.NewPolygon);
            }

            for (int i = 0; i < worldGenerator.physicWorld.player.dinoPencil.Polygons.Count; i++)
            {
                Polygon? polygon = worldGenerator.physicWorld.player.dinoPencil.Polygons[i];
                DrawPolygon(polygon);
            }

            DrawProgressBar(windowWidth,
                worldGenerator.physicWorld.player.dinoPencil.maxPolygonPoints,
                worldGenerator.physicWorld.player.dinoPencil.polygonPoints);

            DrawHearts(worldGenerator.physicWorld.player.lifeHearts, heartTexture, brokenHeartTexture);

            DrawMousePosAndStateString();

            if (worldGenerator.physicWorld.Winned)
            {
                DrawGameEnd(font, windowWidth, windowHeight, "You win!");
            }
            if (worldGenerator.physicWorld.Loose)
            {
                DrawGameEnd(font, windowWidth, windowHeight, "You loose!");
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Drawing Methods
        private void DrawBackground()
        {
            var background = worldGenerator.physicWorld.background;
            
            var centerView0 = camera.TranslateToView(new Vector2(background.l1_X0, windowHeight / 2));
            var centerView1 = camera.TranslateToView(new Vector2(background.l1_X1, windowHeight / 2));
            var centerView2 = camera.TranslateToView(new Vector2(background.l1_X2, windowHeight / 2));

            var center2View0 = camera.TranslateToView(new Vector2(background.l2_X0, windowHeight / 2));
            var center2View1 = camera.TranslateToView(new Vector2(background.l2_X1, windowHeight / 2));

            var mountainWidth = 1000;
            var mountainHeight = 700;
            _spriteBatch.Draw(mountains, new Rectangle((int)centerView1.X, (int)centerView1.Y, mountainWidth, mountainHeight),
                new Rectangle(0, 0, mountains.Width, mountains.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            _spriteBatch.Draw(mountains, new Rectangle((int)centerView2.X, (int)centerView2.Y, mountainWidth, mountainHeight),
                new Rectangle(0, 0, mountains.Width, mountains.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            _spriteBatch.Draw(mountains, new Rectangle((int)centerView0.X, (int)centerView0.Y, mountainWidth, mountainHeight),
                new Rectangle(0, 0, mountains.Width, mountains.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            var plantWidth = 1000;
            var plantHeight = 400;
            _spriteBatch.Draw(plantsBackground, new Rectangle((int)center2View0.X, (int)center2View0.Y + 200, plantWidth, plantHeight),
                new Rectangle(0, 0, plantsBackground.Width, plantsBackground.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
            _spriteBatch.Draw(plantsBackground, new Rectangle((int)center2View1.X, (int)center2View1.Y + 200, plantWidth, plantHeight),
                new Rectangle(0, 0, plantsBackground.Width, plantsBackground.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        private void DrawBounds()
        {
            var leftBound = camera.TranslateToView(new Vector2(0, 0));
            var rightBound = camera.TranslateToView(new Vector2(worldGenerator.GetLevelWidth(), 0));
            var bottomBound = camera.TranslateToView(new Vector2(0, windowHeight + 50));
            var rightBottomBound = camera.TranslateToView(new Vector2(worldGenerator.GetLevelWidth(), windowHeight + 50));

            DrawLine(_spriteBatch, new Vector2(leftBound.X, leftBound.Y), new Vector2(rightBound.X, rightBound.Y), Color.Black, 5, 1);
            DrawLine(_spriteBatch, new Vector2(leftBound.X, leftBound.Y), new Vector2(bottomBound.X, bottomBound.Y), Color.Black, 5, 1);
            DrawLine(_spriteBatch, new Vector2(rightBound.X, rightBound.Y), new Vector2(rightBottomBound.X, rightBottomBound.Y), Color.Black, 5, 1);
            DrawLine(_spriteBatch, new Vector2(bottomBound.X, bottomBound.Y), new Vector2(rightBottomBound.X, rightBottomBound.Y), Color.Black, 5, 1);
        }

        private void DrawOutsideBounds()
        {
            var borderLimit = camera.ViewportSize.Width;

            var leftTopBound = camera.TranslateToView(new Vector2(-borderLimit, 0));
            var rightTopBound = camera.TranslateToView(new Vector2(worldGenerator.GetLevelWidth(), 0));
            var leftBottomBound = camera.TranslateToView(new Vector2(0, windowHeight + 50));
            
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)leftTopBound.X, (int)leftTopBound.Y, borderLimit, windowHeight + borderLimit), Color.White, 1);
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)rightTopBound.X, (int)rightTopBound.Y, borderLimit, windowHeight + borderLimit), Color.White, 1);
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)leftBottomBound.X, (int)leftBottomBound.Y, worldGenerator.GetLevelWidth() + 2 * borderLimit, borderLimit), Color.White, 1);
        }

        private void DrawPlayer()
        {
            var player = worldGenerator.physicWorld.player;
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

            var width = player.polygon.particles[1].Position.X - player.polygon.particles[0].Position.X;
            var height = player.polygon.particles[3].Position.Y - player.polygon.particles[0].Position.Y;
            var centerView = camera.TranslateToView(new Vector2(player.formKeeper.Center.X * 1, player.formKeeper.Center.Y * 1));

            SpriteEffects spriteEffect = player.Orientation == Orientation.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (camera.IsVisible(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y)))
            {
                _spriteBatch.Draw(image, new Rectangle((int)(centerView.X - width / 2), (int)centerView.Y + 10, (int)width, (int)height),
                                       new Rectangle(0, 0, image.Width, image.Height), Color.White, 0, Vector2.Zero, spriteEffect, 1);
            }
        }

        private void DrawPlatform(Vector2 position, float width, float height)
        {
            var centerView = camera.TranslateToView(new Vector2(position.X, position.Y + height));
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)centerView.X, (int)centerView.Y, (int)width, (int)height), Color.White, 1);
            DrawRectangleLines(_spriteBatch, new Rectangle((int)centerView.X, (int)centerView.Y, (int)width, (int)height), Color.Black, 5, 1);
        }

        private void DrawDinosaur(Dinosaur dinosaur)
        {
            var imageString = dinosaur.image;
            var image = dinosaurTextures.FirstOrDefault(x => x.Name == imageString);
            var width = dinosaur.polygon.particles[1].Position.X - dinosaur.polygon.particles[0].Position.X;
            var height = dinosaur.polygon.particles[3].Position.Y - dinosaur.polygon.particles[0].Position.Y;
            var centerView = camera.TranslateToView(new Vector2(dinosaur.formKeeper.Center.X * 1, dinosaur.formKeeper.Center.Y * 1));

            SpriteEffects spriteEffect = dinosaur.Orientation == Orientation.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            _spriteBatch.Draw(image, new Rectangle((int)centerView.X, (int)centerView.Y, (int)width, (int)height),
                                                        new Rectangle(0, 0, image.Width, image.Height), Color.White, 0, Vector2.Zero, spriteEffect, 1);
            
        }

        private void DrawGoal(Goal goal, Texture2D goalTexture)
        {
            var centerView = camera.TranslateToView(new Vector2(goal.Position.X, goal.Position.Y));
            var width = goalTexture.Width;
            var height = goalTexture.Height;
            _spriteBatch.Draw(goalTexture, new Rectangle((int)centerView.X, (int)centerView.Y, width, height),
                               new Rectangle(0, 0, goalTexture.Width, goalTexture.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
        }

        private void DrawProgressBar(int width, int max, int current)
        {
            var x = width - 200 - 50;
            var y = 50;
            var w = 200;
            var h = 30;
            var progress = (float)current / max;
            var progressWidth = (int)(w * progress);
            DrawFilledRectangle(_spriteBatch, new Rectangle(x, y, w, h), Color.White, 1);
            DrawFilledRectangle(_spriteBatch, new Rectangle(x, y, progressWidth, h), Color.Orange, 1);
            DrawRectangleLines(_spriteBatch, new Rectangle(x, y, w, h), Color.Black, 5, 1);
        }

        private void DrawHearts(bool[] lifeHearts, Texture2D heartTexture, Texture2D brokenHeartTexture)
        {
            var x = 50;
            var y = 50;
            var gap = 50;
            for (int i = 0; i < lifeHearts.Length; i++)
            {
                var texture = lifeHearts[i] ? heartTexture : brokenHeartTexture;
                _spriteBatch.Draw(texture, new Rectangle(x + i * gap, y, 40, 40), Color.White);
            }
        }

        private void DrawPolygon(Polygon polygon)
        {
            for (int i = 0; i < polygon.particles.Count; i++)
            {
                Particle? particle = polygon.particles[i];
                Particle? nextParticle = i == polygon.particles.Count - 1 ? polygon.particles[0] : polygon.particles[i + 1];
                DrawLine(_spriteBatch, camera.TranslateToView(particle.Position), camera.TranslateToView(nextParticle.Position), Color.Orange, 5, 1);
            }
        }

        private void DrawMousePosAndStateString()
        {
            var mouse = Mouse.GetState();
            var mousePosition = new Vector2(mouse.X, mouse.Y);
            var mouseView = camera.TranslateToView(mousePosition);
            _spriteBatch.DrawString(font, $"Mouse: {mouseView.X}, {mouseView.Y}", new Vector2(50, 100), Color.Black);
            _spriteBatch.DrawString(font, $"Mouse State: {mouse.LeftButton}", new Vector2(50, 150), Color.Black);
        }

        private void DrawGameEnd(SpriteFont font, int width, int height, string message)
        {
            var textSize = font.MeasureString(message);
            var x = width / 2 - textSize.X / 2;
            var y = height / 2 - textSize.Y / 2;
            _spriteBatch.DrawString(font, message, new Vector2(x, y), Color.Black);
        }
        #endregion

        #region Drawing Helpers

        private Texture2D _blackPoint;
        private Texture2D _whiteRectangle;
        private Texture2D GetLineTexture(SpriteBatch spriteBatch)
        {
            if (_blackPoint == null)
            {
                _blackPoint = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _blackPoint.SetData(new[] { Color.White });
            }

            return _blackPoint;
        }

        private Texture2D GetRectangleTexture(SpriteBatch spriteBatch)
        {
            if (_whiteRectangle == null)
            {
                _whiteRectangle = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                _whiteRectangle.SetData(new[] { Color.White });
            }

            return _whiteRectangle;
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f, int layer = 0)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(spriteBatch, point1, distance, angle, color, thickness, layer);
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f, int layer = 0)
        {
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(GetLineTexture(spriteBatch), point, null, color, angle, origin, scale, SpriteEffects.None, layer);
        }

        private void DrawRectangleLines(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float thickness = 1f, int layer = 0)
        {
            DrawLine(spriteBatch, new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Top), color, thickness, layer);
            DrawLine(spriteBatch, new Vector2(rectangle.Right, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), color, thickness, layer);
            DrawLine(spriteBatch, new Vector2(rectangle.Right, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Bottom), color, thickness, layer);
            DrawLine(spriteBatch, new Vector2(rectangle.Left, rectangle.Bottom), new Vector2(rectangle.Left, rectangle.Top), color, thickness, layer);
        }

        private void DrawFilledRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int layer = 0)
        {
            spriteBatch.Draw(GetRectangleTexture(spriteBatch), rectangle, color);
        }
        #endregion
    }
}
