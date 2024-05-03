using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoDinoGrr.Rendering;
using MonoDinoGrr.WorldGen;
using MonoDinoGrr.WorldObjects;
using System.Collections.Generic;

namespace MonoDinoGrr.Physics
{
    public class PhysicWorld
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Polygon> worldPolygons;

        public List<Dinosaur> dinosaurs;
        public List<Platform> platforms;
        public Player player;
        public Goal goal;
        public Background background;
        public Camera camera;
        
        public bool Winned { get; set; }
        public bool Loose { get; set; }
        public int gameFinishedTimeout = 300;
        public int gameFinnishedCntT = 0;
        public bool gameEnd = false;

        public PhysicWorld(int width, int height, List<Dinosaur> dinosaurs, List<Platform> platforms, Player player, Goal goal, Background background, Camera camera)
        {
            Width = width;
            Height = height;

            worldPolygons = new List<Polygon>();
            for (int i = 0; i < dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = dinosaurs[i];
                worldPolygons.Add(dinosaur.polygon);
            }
            worldPolygons.Add(player.polygon);
            this.platforms = platforms;
            this.goal = goal;
            this.dinosaurs = dinosaurs;
            this.player = player;
            this.background = background;
            this.camera = camera;
        }

        public void Update(int cntT)
        {
            // ========================== UPDATES
            for (int i = 0; i < dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = dinosaurs[i];
                dinosaur.Update(Width, Height, cntT);
            }

            player.Update(Width, Height, worldPolygons, background, camera);

            // ========================== COLLISIONS

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
                for (int j = 0; j < worldPolygons.Count; j++)
                {
                    Polygon? polygon = worldPolygons[j];
                    platform.HandlePolygonCollision(polygon);
                }
                for (int j = 0; j < player.dinoPencil.Polygons.Count; j++)
                {
                    Polygon? polygon = player.dinoPencil.Polygons[j];
                    platform.HandlePolygonCollision(polygon);
                }
            }

            // ========================== GAME FINISHED
            if (Winned || Loose)
            {
                gameFinnishedCntT++;
            }

            if (gameFinnishedCntT > gameFinishedTimeout)
            {
                gameEnd = true;
            }
        }
    }
}
