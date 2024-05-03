using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MonoDinoGrr.Physics
{
    public class Platform
    {
        public Vector2 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle Rectangle { get; set; }

        public Platform(Vector2 position, int width, int height)
        {
            Position = position;
            Width = width;
            Height = height;
            Rectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public void HandlePolygonCollision(Polygon polygon)
        {
            for (int i = 0; i < polygon.particles.Count; i++)
            {
                Particle? particle = polygon.particles[i];
                if (Rectangle.Contains(particle.Position))
                {
                    HandleParticleCollision(particle);
                }
                HandleParticleCollision(particle);
            }            
        }

        private void HandleParticleCollision(Particle particle)
        {
            var hitBox = 10;
            Rectangle top = new Rectangle((int)Position.X, (int)Position.Y, Width, hitBox);
            Rectangle bottom = new Rectangle((int)Position.X, (int)Position.Y + Height - hitBox, Width, hitBox);
            Rectangle left = new Rectangle((int)Position.X, (int)Position.Y, hitBox, Height);
            Rectangle right = new Rectangle((int)Position.X + Width - hitBox, (int)Position.Y, hitBox, Height);

            if (top.Contains(particle.Position))
            {
                particle.Position = new Vector2(particle.Position.X, Position.Y);
                particle.IsInGround = true;
                return;
            }
            if (bottom.Contains(particle.Position))
            {
                particle.Position = new Vector2(particle.Position.X, Position.Y + Height);
                return;
            }
            if (left.Contains(particle.Position))
            {
                particle.Position = new Vector2(Position.X, particle.Position.Y);
                return;
            }
            if (right.Contains(particle.Position))
            {
                particle.Position = new Vector2(Position.X + Width, particle.Position.Y);
                return;
            }
        }
    }
}
