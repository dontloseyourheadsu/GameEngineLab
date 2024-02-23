using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{
    public class Emitter
    {
        public PointF Position { get; set; }
        public List<Particle> Particles { get; set; }
        public List<Influencer> Influencers { get; set; }
        private Size space;

        public int MIN_X { get; set; } = -1;
        public int MAX_X { get; set; } = 2;

        public int MIN_Y { get; set; } = -1;
        public int MAX_Y { get; set; } = -1;


        public Emitter(PointF position, Size space)
        {
            this.Position = position;
            this.space = space;
            this.Influencers = new List<Influencer>();
            this.Particles = new List<Particle>();
        }

        public void GenerateParticles(int count = 300)
        {
            for (int i = 0; i < count; i++)
            {
                // Generate random velocities decimals.
                float scaleX = 0.01f;
                float scaleY = 0.5f;

                //float velocityX = ((float)Util.Instance.Rand.NextDouble() * (MAX_X - MIN_X) + MIN_X) * scaleX;
                //float velocityY = ((float)Util.Instance.Rand.NextDouble() * (MAX_Y - MIN_Y) + MIN_Y) * scaleY;

                float velocityX = Util.Instance.Rand.Next(MIN_X, MAX_X);
                float velocityY = Util.Instance.Rand.Next(MIN_Y, MAX_Y);

                // Emit a new particle with the random velocities.
                EmitParticle(new PointF(velocityX, velocityY));
            }
        }

        public void Render(Graphics g, float deltaTime)
        {
            int div = 8;
            int index = 0;

            for (int p = 0; p < Particles.Count / div; p++)
            {
                index = p * div;

                Update(g, index + 0, deltaTime);
                Update(g, index + 1, deltaTime);
                Update(g, index + 2, deltaTime);
                Update(g, index + 3, deltaTime);
                Update(g, index + 4, deltaTime);
                Update(g, index + 5, deltaTime);
                Update(g, index + 6, deltaTime);
                Update(g, index + 7, deltaTime);
            }
        }

        public void Update(Graphics g, int p, float deltaTime)
        {
            Particles[p].Update(Influencers, deltaTime, space, Position);
            Particles[p].Render(g);
        }

        // EmitParticle is supposed to initialize a new particle with a given velocity and add it to the particles list.
        public void EmitParticle(PointF velocity)
        {
            Particle newParticle = new Particle(this.Position, velocity);
            Particles.Add(newParticle);
        }

        public void ChangeVelocity(int minX, int maxX, int minY, int maxY)
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                var particle = Particles[i];

                float scaleX = 0.01f;
                float scaleY = 0.5f;

                //particle.vX = ((float)Util.Instance.Rand.NextDouble() * (maxX - minX) + minX) * scaleX;
                //particle.vY = ((float)Util.Instance.Rand.NextDouble() * (maxY - minY) + minY) * scaleY;

                particle.vX = Util.Instance.Rand.Next(minX, maxX);
                particle.vY = Util.Instance.Rand.Next(minY, maxY);

                particle.OV.X = particle.vX;
                particle.OV.Y = particle.vY;
            }
        }

        public void AddInfluencer(Influencer influencer)
        {
            Influencers.Add(influencer);
        }

        public void ChangeAlpha(float alpha)
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                var particle = Particles[i];
                particle.Alfa = Math.Min(particle.Alfa, alpha);

                particle.Image = Util.ApplyAlpha(particle.Image, particle.Alfa);
            }
        }
    }
}
