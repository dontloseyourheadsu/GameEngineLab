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
        public Texture2D transparentTexture;

        List<Texture2D> dinosaurTextures = new List<Texture2D>();

        int cntT = 0;
        int windowWidth;
        int worldHeight;

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

        private void InstanciateWorld()
        {
            var levelDinosaurs = worldGenerator.GetLevelDinosaurs();
            var levelPlatforms = worldGenerator.GetLevelPlatforms();
            var levelGoal = worldGenerator.GetLevelGoal();
            var levelPlayer = worldGenerator.GetLevelPlayer();
            var worldWidth = worldGenerator.GetLevelWidth();

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

            var background = new Background(windowWidth, worldHeight, Content);

            camera = new Camera(player, new Rectangle(0, 0, windowWidth, worldHeight), 1);

            worldGenerator.physicWorld = new PhysicWorld(worldWidth, worldHeight, dinosaurs, platforms, player, goal, background, camera);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            windowWidth = GraphicsDevice.Viewport.Width;
            worldHeight = 1080;

            worldGenerator = new WorldGenerator();
            
            InstanciateWorld();

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

            standingImage = Content.Load<Texture2D>("dino-girl-standing-0");
            runningImages = new Texture2D[8]
            {
                Content.Load<Texture2D>("dino-girl-running-0"),
                Content.Load<Texture2D>("dino-girl-running-1"),
                Content.Load<Texture2D>("dino-girl-running-2"),
                Content.Load<Texture2D>("dino-girl-running-3"),
                Content.Load<Texture2D>("dino-girl-running-4"),
                Content.Load<Texture2D>("dino-girl-running-5"),
                Content.Load<Texture2D>("dino-girl-running-6"),
                Content.Load<Texture2D>("dino-girl-running-7")
            };
            hitImage = Content.Load<Texture2D>("dino-girl-hit-0");

            transparentTexture = new Texture2D(GraphicsDevice, 1, 1);
            transparentTexture.SetData(new Color[] { Color.Transparent });
            var dinoImages = new List<Texture2D>()
            {
                Content.Load<Texture2D>("dinosaur-blue"),
                Content.Load<Texture2D>("dinosaur-green"),
                Content.Load<Texture2D>("dinosaur-light-green"),
                Content.Load<Texture2D>("dinosaur-purple"),
                Content.Load<Texture2D>("dinosaur-red")
            };

            dinosaurTextures.AddRange(dinoImages);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            if (worldGenerator.physicWorld.gameEnd)
            {
                var currentLevel = worldGenerator.Level;
                while (currentLevel == worldGenerator.Level)
                { 
                    worldGenerator.Level = new Random().Next(0, worldGenerator.GetLevelCount());
                    InstanciateWorld();
                }
            }
            else
            {
                worldGenerator.physicWorld.Update(cntT);
                cntT++;
            }

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
                DrawCreatingPolygon(worldGenerator.physicWorld.player.dinoPencil.NewPolygon);
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


            if (worldGenerator.physicWorld.Winned)
            {
                DrawGameEnd(font, windowWidth, worldHeight, "You win!");
            }
            if (worldGenerator.physicWorld.Loose)
            {
                DrawGameEnd(font, windowWidth, worldHeight, "You loose!");
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Drawing Methods
        private void DrawBackground()
        {
            var background = worldGenerator.physicWorld.background;
            
            var centerView0 = camera.TranslateToView(new Vector2(background.l1_X0, worldHeight / 2));
            var centerView1 = camera.TranslateToView(new Vector2(background.l1_X1, worldHeight / 2));
            var centerView2 = camera.TranslateToView(new Vector2(background.l1_X2, worldHeight / 2));

            var center2View0 = camera.TranslateToView(new Vector2(background.l2_X0, worldHeight / 2));
            var center2View1 = camera.TranslateToView(new Vector2(background.l2_X1, worldHeight / 2));

            var mountainWidth = background.width;
            var mountainHeight = 700;
            _spriteBatch.Draw(mountains, new Rectangle((int)centerView1.X, (int)centerView1.Y, mountainWidth, mountainHeight),
                new Rectangle(0, 0, mountains.Width, mountains.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            _spriteBatch.Draw(mountains, new Rectangle((int)centerView2.X, (int)centerView2.Y, mountainWidth, mountainHeight),
                new Rectangle(0, 0, mountains.Width, mountains.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            _spriteBatch.Draw(mountains, new Rectangle((int)centerView0.X, (int)centerView0.Y, mountainWidth, mountainHeight),
                new Rectangle(0, 0, mountains.Width, mountains.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            var plantWidth = background.width;
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
            var bottomBound = camera.TranslateToView(new Vector2(0, worldHeight + 50));
            var rightBottomBound = camera.TranslateToView(new Vector2(worldGenerator.GetLevelWidth(), worldHeight + 50));

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
            var leftBottomBound = camera.TranslateToView(new Vector2(0, worldHeight + 50));
            
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)leftTopBound.X, (int)leftTopBound.Y, borderLimit, worldHeight + borderLimit), Color.White, 1);
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)rightTopBound.X, (int)rightTopBound.Y, borderLimit, worldHeight + borderLimit), Color.White, 1);
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)leftBottomBound.X, (int)leftBottomBound.Y, worldGenerator.GetLevelWidth() + 2 * borderLimit, borderLimit), Color.White, 1);
        }

        private void DrawPlayer()
        {
            var player = worldGenerator.physicWorld.player;
            Texture2D image = standingImage;
            if (player.isDamaged)
            {
                if (cntT % 30 < 15)
                {
                    image = hitImage;
                }
                else
                {
                    image = transparentTexture;
                }
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
            var centerView = camera.TranslateToView(new Vector2(position.X, position.Y + 60));
            DrawFilledRectangle(_spriteBatch, new Rectangle((int)centerView.X, (int)centerView.Y, (int)width, (int)height), Color.White, 1);
            DrawRectangleLines(_spriteBatch, new Rectangle((int)centerView.X, (int)centerView.Y, (int)width, (int)height), Color.Black, 5, 1);
        }

        private void DrawDinosaur(Dinosaur dinosaur)
        {
            var imageString = dinosaur.image;
            var image = dinosaurTextures.FirstOrDefault(x => x.Name == imageString);
            var width = dinosaur.polygon.particles[1].Position.X - dinosaur.polygon.particles[0].Position.X;
            var height = dinosaur.polygon.particles[3].Position.Y - dinosaur.polygon.particles[0].Position.Y;
            var centerView = camera.TranslateToView(new Vector2(dinosaur.polygon.particles[0].Position.X, dinosaur.polygon.particles[0].Position.Y));

            SpriteEffects spriteEffect = dinosaur.Orientation == Orientation.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            _spriteBatch.Draw(image, new Rectangle((int)centerView.X, (int)(centerView.Y + height - 10), (int)width, (int)height),
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
            for(int i = 0; i < polygon.sticks.Count; i++)
            {
                Stick? stick = polygon.sticks[i];
                var particleMass = Math.Max(stick.A.Mass, stick.B.Mass);
                var vectorA = new Vector2(stick.A.Position.X, stick.A.Position.Y + 50);
                var vectorB = new Vector2(stick.B.Position.X, stick.B.Position.Y + 50);
                DrawLine(_spriteBatch, camera.TranslateToView(vectorA), camera.TranslateToView(vectorB), Color.Orange, particleMass, 1);
            }
        }

        private void DrawCreatingPolygon(Polygon polygon)
        {
            for (int i = 0; i < polygon.sticks.Count; i++)
            {
                Stick? stick = polygon.sticks[i];
                var particleMass = Math.Max(stick.A.Mass, stick.B.Mass);
                var vectorA = new Vector2(stick.A.Position.X, stick.A.Position.Y);
                var vectorB = new Vector2(stick.B.Position.X, stick.B.Position.Y);
                DrawLine(_spriteBatch, camera.TranslateToView(vectorA), camera.TranslateToView(vectorB), Color.Orange, particleMass, 1);
            }
        }

        private void DrawGameEnd(SpriteFont font, int width, int height, string message)
        {
            var textSize = font.MeasureString(message);
            var scale = 1;
            var x = width / 2 - textSize.X / 2 * scale;
            var y = height / 2 - textSize.Y / 2 * scale;

            DrawFilledRectangle(_spriteBatch, new Rectangle((int)x, (int)y, (int)textSize.X, (int)textSize.Y), Color.White, 1);
            DrawRectangleLines(_spriteBatch, new Rectangle((int)x, (int)y, (int)textSize.X, (int)textSize.Y), Color.Black, 5, 1);

            _spriteBatch.DrawString(font, message, new Vector2(x, y), Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
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
