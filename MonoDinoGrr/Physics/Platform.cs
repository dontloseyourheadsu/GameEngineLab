using Microsoft.Xna.Framework;

namespace MonoDinoGrr.Physics
{
    public class Platform
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Platform(Vector2 position, int width, int height)
        {
            Position = position;
            Width = width;
            Height = height;
        }

        public void HandlePolygonCollision(Polygon polygon)
        {
            for (int i = 0; i < polygon.particles.Count; i++)
            {
                Particle? particle = polygon.particles[i];
                HandleParticleCollision(particle);
            }            
        }

        private void HandleParticleCollision(Particle particle)
        {
            if ((particle.Position.X >= Position.X && particle.Position.X <= Position.X + Width) &&
                (particle.Position.Y >= Position.Y && particle.Position.Y <= Position.Y + Height))
            {
                if (particle.Position.Y >= Position.Y)
                {
                    particle.Position = new Vector2(particle.Position.X, Position.Y);
                    particle.IsInGround = true;
                    return;
                }
                if (particle.Position.X >= Position.X)
                {
                    particle.Position = new Vector2(Position.X, particle.Position.Y);
                    return;
                }
                if (particle.Position.X <= Position.X + Width)
                {
                    particle.Position = new Vector2(Position.X + Width, particle.Position.Y);
                    return;
                }
                if (particle.Position.Y <= Position.Y + Height)
                {
                    particle.Position = new Vector2(particle.Position.X, Position.Y + Height);
                    return;
                }
            }
        }
    }
}
