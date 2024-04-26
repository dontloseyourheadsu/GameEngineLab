using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoDinoGrr.Physics;
using MonoDinoGrr.Rendering;
using MonoDinoGrr.WorldGen;
using MonoDinoGrr.WorldObjects;
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
        public Texture2D platformTexture;
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
        float scale = 2.25f;
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
            platformTexture = Content.Load<Texture2D>("platform");
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
            //DrawBounds();

            DrawPlayer();


            //render.DrawBounds(Width, Height);

            //if (Winned)
            //{
            //    render.DrawWin(font, Width, Height);
            //}
            //if (Loose)
            //{
            //    render.DrawLoose(font, Width, Height);
            //}

            //for (int i = 0; i < platforms.Count; i++)
            //{
            //    Platform? platform = platforms[i];
            //    render.DrawPlatform(texturePlatform, platform.Position, platform.Width, platform.Height);
            //}

            //for (int i = 0; i < player.dinoPencil.Polygons.Count; i++)
            //{
            //    Polygon? polygon = player.dinoPencil.Polygons[i];
            //    render.DrawPolygon(polygon);
            //}

            //if (player.dinoPencil.NewPolygon != null)
            //{
            //    render.DrawPolygon(player.dinoPencil.NewPolygon);
            //}

            //for (int i = 0; i < dinosaurs.Count; i++)
            //{
            //    Dinosaur? dinosaur = dinosaurs[i];
            //    render.DrawDinosaur(dinosaur);
            //}

            //render.DrawGoal(goal, goalTexture);
            //render.DrawGirl(player, cntT, player.runningImages, player.standingImage, player.hitImage);

            //// ========================== AFTER DRAWINGS

            //render.DrawOutsideBounds(blankTexture, Width, Height);
            //render.DrawProgressBar(Width,
            //    dinoPencil.maxPolygonPoints,
            //    dinoPencil.polygonPoints);
            //render.DrawHearts(player.lifeHearts, heartTexture, brokenHeartTexture);


            // ============================ABOVE SETUP
            //var Width = GraphicsDevice.Viewport.Width;
            //var Height = GraphicsDevice.Viewport.Height;
            //var Winned = physicWorld.Winned;
            //var Loose = physicWorld.Loose;
            //var platforms = physicWorld.platforms;
            //var player = physicWorld.player;
            //var goal = physicWorld.goal;
            //var dinosaurs = physicWorld.dinosaurs;
            //var dinoPencil = physicWorld.player.dinoPencil;
            //var font = render.font;
            //var blankTexture = render.blankTexture;
            //var heartTexture = render.heartTexture;
            //var brokenHeartTexture = render.brokenHeartTexture;
            //var goalTexture = render.goalTexture;
            //var texturePlatform = render.platform;

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBackground()
        {
            var background = worldGenerator.physicWorld.background;
            
            var centerView1 = camera.TranslateToView(new Vector2(background.l1_X1, 50 * scale));

            var center2View0 = camera.TranslateToView(new Vector2(background.l2_X0, (windowHeight / 3f) * scale));
            var center2View1 = camera.TranslateToView(new Vector2(background.l2_X1, (windowHeight / 3f) * scale));
            var center2View2 = camera.TranslateToView(new Vector2(background.l2_X2, (windowHeight / 3f) * scale));

            _spriteBatch.Draw(mountains, new Vector2(centerView1.X, centerView1.Y), new Rectangle(0, 0, background.layer1.Width, background.height), Color.White, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0);

            _spriteBatch.Draw(plantsBackground, new Vector2(center2View0.X, center2View0.Y), new Rectangle(0, 0, plantsBackground.Width, plantsBackground.Height), Color.White, 0, new Vector2(0, 0), scale * 2, SpriteEffects.None, 1);
            _spriteBatch.Draw(plantsBackground, new Vector2(center2View1.X, center2View1.Y), new Rectangle(0, 0, plantsBackground.Width, plantsBackground.Height), Color.White, 0, new Vector2(0, 0), scale * 2, SpriteEffects.None, 1);
            _spriteBatch.Draw(plantsBackground, new Vector2(center2View2.X, center2View2.Y), new Rectangle(0, 0, plantsBackground.Width, plantsBackground.Height), Color.White, 0, new Vector2(0, 0), scale * 2, SpriteEffects.None, 1);
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
                _spriteBatch.Draw(image, new Vector2(centerView.X, centerView.Y), new Rectangle(0, 0, image.Width, image.Height), Color.White, 0, Vector2.Zero, scale, spriteEffect, 0);
            }
        }
    }
}
