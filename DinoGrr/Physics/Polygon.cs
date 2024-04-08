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
            foreach (var particle in particles)
            {
                particle.Update(subStep, gravity: 0.1f);
                particle.KeepInsideCanvas(width, height);
            }
        }

        private void UpdateSticks()
        {
            foreach (var stick in sticks)
            {
                stick.Update();
            }
        }
    }
}
