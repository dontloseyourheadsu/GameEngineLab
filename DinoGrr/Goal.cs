using DinoGrr.Physics;

namespace DinoGrr
{
    public class Goal
    {
        public Polygon polygon { get; set; }
        public FormKeeper formKeeper { get; set; }
        public Vector2 Position { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public Goal(Vector2 position)
        {
            Position = position;

            Width = 30;
            Height = 30;
            var width = Width;
            var height = Height;
            var x = (int)Position.X;
            var y = (int)Position.Y;

            var particles = new List<Particle>
            {
                new Particle(new Vector2(x - width / 2, y - height / 2), 2, 'g'), // top left
                new Particle(new Vector2(x + width / 2, y - height / 2), 2, 'g'), // top right
                new Particle(new Vector2(x + width / 2, y + height / 2), 2, 'g'), // bottom right
                new Particle(new Vector2(x - width / 2, y + height / 2), 2, 'g'), // bottom left
            };

            particles[0].Locked = true;
            particles[1].Locked = true;
            particles[2].Locked = true;
            particles[3].Locked = true;

            var sticks = new List<Stick>
            {
                new Stick(particles[0], particles[1]), // top edge
                new Stick(particles[1], particles[2]), // right edge
                new Stick(particles[2], particles[3]), // bottom edge
                new Stick(particles[3], particles[0]), // left edge
            };

            polygon = new Polygon(particles, sticks);

            formKeeper = new FormKeeper(polygon);
        }
    }
}
