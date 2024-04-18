using DinoGrr.Rendering;

namespace DinoGrr.Physics
{
    public class PhysicWorld
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Dinosaur> dinosaurs;
        public Player player;
        public List<Polygon> worldPolygons;
        public Background background;
        Camera camera;

        public PhysicWorld(int width, int height)
        {
            Width = width;
            Height = height;
            CreatePlayer();
            DefineWorldObjects(width, height);
        }

        private void CreatePlayer()
        {
            player = new Player();
        }

        private void DefineWorldObjects(int width, int height)
        {
            dinosaurs = new List<Dinosaur>();
            dinosaurs.Add(new Dinosaur(100, 100, 75, 50, Resource.dinosaur_green, player));
            dinosaurs.Add(new Dinosaur(100, 200, 50, 50, Resource.dinosaur_blue, player));

            worldPolygons = new List<Polygon>();
            for (int i = 0; i < dinosaurs.Count; i++)
            {
                Dinosaur? dinosaur = dinosaurs[i];
                worldPolygons.Add(dinosaur.polygon);
            }

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

            player.Update(Width, Height);

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
                            particle.CheckPolygonCollision(otherPolygon);
                        }
                    }
                }
            }

            // ========================== DRAWINGS

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

            render.DrawGirl(player);

            // ========================== AFTER DRAWINGS
            render.DrawOutsideBounds(Width, Height);
        }
    }
}
