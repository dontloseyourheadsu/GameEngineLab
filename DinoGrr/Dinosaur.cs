using DinoGrr.Physics;

namespace DinoGrr
{
    public class Dinosaur
    {
        public Polygon polygon { get; set; }
        public Particle CenterPosition { get; set; }

        public Dinosaur(int x, int y, int width, int height)
        {
            var particles = new List<Particle>
            {
                new Particle(new Vector2(x - width / 2, y - height / 2), 2), // top left
                new Particle(new Vector2(x + width / 2, y - height / 2), 2), // top right
                new Particle(new Vector2(x + width / 2, y + height / 2), 2), // bottom right
                new Particle(new Vector2(x - width / 2, y + height / 2), 2), // bottom left
                new Particle(new Vector2(x, y), 2) // center
            };

                var sticks = new List<Stick>
            {
                new Stick(particles[0], particles[1]), // top edge
                new Stick(particles[1], particles[2]), // right edge
                new Stick(particles[2], particles[3]), // bottom edge
                new Stick(particles[3], particles[0]), // left edge
                new Stick(particles[0], particles[4]), // diagonal top left to center
                new Stick(particles[1], particles[4]), // diagonal top right to center
                new Stick(particles[2], particles[4]), // diagonal bottom right to center
                new Stick(particles[3], particles[4])  // diagonal bottom left to center
            };

            polygon = new Polygon(particles, sticks);
            CenterPosition = particles[4]; // set the position to the center particle
        }

        public void Update(int width, int height, int cntT)
        {
            polygon.Update(width, height);
            if (cntT % 60 == 0)
            {
                JumpRight();
                StandUp();
            }
        }

        public void JumpRight()
        {
            if (polygon.particles[2].IsInGround && polygon.particles[3].IsInGround)
            { 
                CenterPosition.Position += new Vector2(3, -10); 
            }
        }

        public void JumpLeft()
        {
            if (polygon.particles[0].IsInGround && polygon.particles[1].IsInGround)
            {
                CenterPosition.Position += new Vector2(-3, -10);
            }
        }

        public void StandUp()
        {
            if (polygon.particles[0].IsInGround)
            {
                polygon.particles[0].Position += new Vector2(0, -5);
            }
            if (polygon.particles[1].IsInGround)
            {
                polygon.particles[1].Position += new Vector2(0, -5);
            }
        }
    }
}
