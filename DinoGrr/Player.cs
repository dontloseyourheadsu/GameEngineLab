using DinoGrr.Physics;

namespace DinoGrr
{
    public class Player
    {
        public Polygon polygon { get; set; }
        public FormKeeper formKeeper { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 CameraPosition { get; set; }
        public Orientation Orientation { get; set; }
        public Orientation ImageOrientation { get; set; }
        public Particle LeftLeg { get; set; }
        public Particle RightLeg { get; set; }
        public Bitmap image;
        int speed = 1;

        public DinoPencil dinoPencil { get; set; }

        public Player(Vector2 position = null)
        {
            dinoPencil = new DinoPencil();
            if (position == null)
            {
                Position = new Vector2(100, 100);
            }
            else
            {
                Position = position;
            }
            CameraPosition = new Vector2(Position.X - 200, Position.Y - 500);

            var height = 40;
            var width = 20;
            var x = (int)Position.X;
            var y = (int)Position.Y;

            var particles = new List<Particle>
            {
                new Particle(new Vector2(x - width / 2, y - height / 2), 2), // top left
                new Particle(new Vector2(x + width / 2, y - height / 2), 2), // top right
                new Particle(new Vector2(x + width / 2, y + height / 2), 2), // bottom right
                new Particle(new Vector2(x - width / 2, y + height / 2), 2), // bottom left
            };

            var sticks = new List<Stick>
            {
                new Stick(particles[0], particles[1]), // top edge
                new Stick(particles[1], particles[2]), // right edge
                new Stick(particles[2], particles[3]), // bottom edge
                new Stick(particles[3], particles[0]), // left edge
            };

            polygon = new Polygon(particles, sticks);
            LeftLeg = particles[3];
            RightLeg = particles[2];

            formKeeper = new FormKeeper(polygon);
            ImageOrientation = Orientation.Left;
            image = Resource.dinosaur_girl;
        }

        public void Update(int width, int height)
        {
            Position = formKeeper.Center;
            CameraPosition = new Vector2(Position.X-200, Position.Y-500);
            polygon.Update(width, height);
            formKeeper.RestoreOriginalForm();

            dinoPencil.Update(width, height);
        }

        public void MoveRight()
        {
            LeftLeg.Position += new Vector2(speed, 0);
            RightLeg.Position += new Vector2(speed, 0);
            Orientation = Orientation.Right;
        }

        public void MoveLeft()
        {
            LeftLeg.Position += new Vector2(-speed, 0);
            RightLeg.Position += new Vector2(-speed, 0);
            Orientation = Orientation.Left;
        }

        public void Jump()
        {
            if (LeftLeg.IsInGround && RightLeg.IsInGround)
            {
                LeftLeg.Position += new Vector2(0, -speed * 2);
                RightLeg.Position += new Vector2(0, -speed * 2);
            }
        }
    }
}
