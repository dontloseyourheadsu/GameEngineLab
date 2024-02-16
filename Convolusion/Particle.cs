using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Particles
{
    internal class Particle
    {
        public Size space;
        public float Radius, diameter, vx, vy;
        public float X, Y;
        public int index;
        public Color c;
        public bool changed;
        public int screenFactor;

        public Particle(Random rand, Size size, int index, int screenFactor)
        {
            X = rand.Next(0, size.Width);
            Y = rand.Next(0, size.Height);
            vx = rand.Next(-5, 10);
            vy = rand.Next(-5, 10);
            diameter = rand.Next(40, 55);
            space = size;
            this.index = index;
            changed = false;
            this.screenFactor = screenFactor;
            Radius = diameter / 2; c = Color.FromArgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
        }

        private void ResolveWalls()
        {
            if ((X - Radius) < 0)
            {
                changed = true; vx *= -1; X = Radius;
            }
            if ((X + Radius) > space.Width)
            {
                changed = true; vx *= -1; X = space.Width - Radius;
            }
            if ((Y - Radius) < 0)
            {
                vy *= -1; changed = true; Y = Radius;
            }
            if ((Y + Radius) > space.Height - screenFactor)
            {
                vy *= -1; changed = true; Y = space.Height - screenFactor - Radius;
            }
        }

        private float Distance(Particle other)
        {
            return (float)Math.Sqrt(Math.Pow(this.X - other.X, 2) + Math.Pow(this.Y - other.Y, 2));
        }

        public bool Collision(Particle other)
        {
            return Distance(other) < (this.Radius + other.Radius);
        }

        public void Resolve(Particle other)
        {
            float overlap = (this.Radius + other.Radius) - Distance(other);

            float dx = this.X - other.X;
            float dy = this.Y - other.Y;

            float distance = Distance(other);
            float nx = dx / distance;
            float ny = dy / distance;

            this.X += nx * overlap * 0.8f;
            this.Y += ny * overlap * 0.8f;
            other.X -= nx * overlap * 0.8f;
            other.Y -= ny * overlap * 0.5f;

            float slowDownFactor = 0.9f;
            this.vx *= -slowDownFactor;
            this.vy *= -slowDownFactor;
            other.vx *= -slowDownFactor;
            other.vy *= -slowDownFactor;
        }


        public void Update(List<Particle> particles, float deltaTime)
        {
            X += vx * deltaTime;
            Y += vy * deltaTime;

            ResolveWalls();

            foreach (Particle other in particles)
            {
                if (other != this && Collision(other))
                {
                    Resolve(other);
                }
            }

            vx *= 0.99f;
            vy *= 0.99f;
        }

    }
}
