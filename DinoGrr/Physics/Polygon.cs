namespace DinoGrr.Physics
{
    public class Polygon
    {
        public List<Particle> particles { get; set; }
        public List<Stick> sticks { get; set; }

        public Polygon(List<Particle> particles, List<Stick> sticks)
        {
            this.particles = particles;
            this.sticks = sticks;
        }

        public void Update(int width, int height)
        {
            var subSteps = 1;
            for(int i = 1; i <= subSteps; i++)
            {
                UpdateParticles(i, width, height);
                UpdateSticks();
            }
        }

        private void UpdateParticles(int subStep, int width, int height)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                Particle? particle = particles[i];
                particle.Update(subStep, gravity: 0.5f);
                particle.KeepInsideCanvas(width, height);
            }
        }

        private void UpdateSticks()
        {
            for (int i = 0; i < sticks.Count; i++)
            {
                Stick? stick = sticks[i];
                stick.Update();
            }
        }
    }
}
