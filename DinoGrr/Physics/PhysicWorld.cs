using DinoGrr.Rendering;
using DinoGrr.WorldGen;

namespace DinoGrr.Physics
{
    public class PhysicWorld
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Dinosaur> dinosaurs;
        public List<Platform> platforms;
        public Player player;
        public List<Polygon> worldPolygons;
        public Background background;
        Camera camera;
        Goal goal;
        public bool Winned { get; set; }
        public bool Loose { get; set; }
        public int gameFinishedTimeout = 300;
        public int gameFinnishedCntT = 0;
        public bool gameEnd = false;

        public PhysicWorld(int width, int height, LevelGoal levelGoal, LevelPlayer levelPlayer, List<LevelDinosaur> levelDinosaurs, List<LevelPlatform> levelPlatforms)
        {
            Width = width;
            Height = height;
            CreatePlatforms(levelPlatforms);
            CreateGoal(levelGoal);
            CreatePlayer(levelPlayer);
            DefineWorldObjects(width, height, levelDinosaurs);
        }

        private void CreatePlatforms(List<LevelPlatform> levelPlatforms)
        {
            platforms = new List<Platform>();
            for (int i = 0; i < levelPlatforms.Count; i++)
            {
                LevelPlatform? levelPlatform = levelPlatforms[i];
                platforms.Add(new Platform(new Vector2(levelPlatform.X, levelPlatform.Y), levelPlatform.Width, levelPlatform.Height));
            }
        }

        private void CreateGoal(LevelGoal levelGoal)
        {
            goal = new Goal(new Vector2(levelGoal.X, levelGoal.Y));
        }

        private void CreatePlayer(LevelPlayer levelPlayer)
        {
            player = new Player(new Vector2(levelPlayer.X, levelPlayer.Y));
        }

        private void DefineWorldObjects(int width, int height, List<LevelDinosaur> levelDinosaurs)
        {
            dinosaurs = new List<Dinosaur>();
            for (int i = 0; i < levelDinosaurs.Count; i++)
            {
                LevelDinosaur? levelDinosaur = levelDinosaurs[i];
                var image = (Bitmap)Resource.ResourceManager.GetObject(levelDinosaur.Dino);
                dinosaurs.Add(new Dinosaur(levelDinosaur.X, levelDinosaur.Y, levelDinosaur.Width, levelDinosaur.Height, image, player));
            }

            worldPolygons = new List<Polygon>();
            for (int i = 0; i < dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = dinosaurs[i];
                worldPolygons.Add(dinosaur.polygon);
            }
            worldPolygons.Add(player.polygon);

            background = new Background(width, height+100);
        }

        public void Update(int cntT, Vector2 mouseG, Render render)
        {
            // ========================== WORLD
            render.DrawBackground(background);
            render.DrawBounds(Width, Height);

            // ========================== UPDATES
            for (int i = 0; i < dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = dinosaurs[i];
                dinosaur.Update(Width, Height, cntT);
            }

            player.Update(Width, Height, worldPolygons);

            // ========================== COLLISIONS

            // iterate all polygons, for each polygon particle check collision with other polygons
            for (int i = 0; i < worldPolygons.Count; i++)
            {
                Polygon? polygon = worldPolygons[i];
                for (int j = 0; j < polygon.particles.Count; j++)
                {
                    Particle? particle = polygon.particles[j];
                    for (int k = 0; k < worldPolygons.Count; k++)
                    {
                        Polygon? otherPolygon = worldPolygons[k];
                        if (polygon != otherPolygon)
                        {
                            var isColliding = particle.CheckPolygonCollision(otherPolygon);
                            if (isColliding)
                            {
                                if((particle.BelongsTo == 'g' && otherPolygon.particles[0].BelongsTo == 'd') || (particle.BelongsTo == 'd' && otherPolygon.particles[0].BelongsTo == 'g'))
                                {
                                    player.RemoveHearts();
                                    var looseCounter = 0;
                                    for (int i1 = 0; i1 < player.lifeHearts.Length; i1++)
                                    {
                                        bool heart = player.lifeHearts[i1];
                                        if (!heart)
                                        {
                                            looseCounter++;
                                        }
                                    }
                                    if (looseCounter == player.lifeHearts.Length)
                                    {
                                        Loose = true;
                                        gameFinnishedCntT++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < goal.polygon.particles.Count; i++)
            {
                Particle? particle = goal.polygon.particles[i];
                if (particle.CheckPolygonCollision(player.polygon))
                {
                    Winned = true;
                    gameFinnishedCntT++;
                }
            }

            for (int i = 0; i < platforms.Count; i++)
            {
                Platform? platform = platforms[i];
                platform.HandlePolygonCollision(player.polygon);
                for (int i1 = 0; i1 < dinosaurs.Count; i1++)
                {
                    Dinosaur? dinosaur = dinosaurs[i1];
                    platform.HandlePolygonCollision(dinosaur.polygon);
                }
            }

            // ========================== DRAWINGS

            if (Winned)
            {
                render.DrawWin(Width, Height);
                gameFinnishedCntT++;
            }
            if (Loose)
            {
                render.DrawLoose(Width, Height);
                gameFinnishedCntT++;
            }

            for (int i = 0; i < platforms.Count; i++)
            {
                Platform? platform = platforms[i];
                render.DrawPlatform(platform.Position, platform.Width, platform.Height);
            }

            for (int i = 0; i < player.dinoPencil.Polygons.Count; i++)
            {
                Polygon? polygon = player.dinoPencil.Polygons[i];
                render.DrawPolygon(polygon);
            }

            if (player.dinoPencil.NewPolygon != null)
            {
                render.DrawPolygon(player.dinoPencil.NewPolygon);
            }

            for (int i = 0; i < dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = dinosaurs[i];
                render.DrawDinosaur(dinosaur);
            }

            render.DrawGoal(goal);
            render.DrawGirl(player, cntT);

            // ========================== AFTER DRAWINGS
            render.DrawOutsideBounds(Width, Height);
            render.DrawProgressBar(Width,
                player.dinoPencil.maxPolygonPoints, 
                player.dinoPencil.polygonPoints);
            render.DrawHearts(player.lifeHearts);

            // ========================== GAME FINISHED
            if (gameFinnishedCntT > gameFinishedTimeout)
            {
                gameEnd = true;
            }
        }
    }
}
