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
        public Orientation StandingImageOrientation { get; set; }
        public Orientation[] RunningImagesOrientations = [Orientation.Left, 
                   Orientation.Left, 
                   Orientation.Left, 
                   Orientation.Left, 
                   Orientation.Left, 
                   Orientation.Left, 
                   Orientation.Left, 
                   Orientation.Left];
        public Orientation HitImageOrientation { get; set; }
        public Particle LeftLeg { get; set; }
        public Particle RightLeg { get; set; }
        public Bitmap standingImage = Resource.madoka_standing_0;
        public Bitmap[] runningImages = [Resource.madoka_running_0, 
            Resource.madoka_running_1, 
            Resource.madoka_running_2, 
            Resource.madoka_running_3, 
            Resource.madoka_running_4, 
            Resource.madoka_running_5, 
            Resource.madoka_running_6, 
            Resource.madoka_running_7];
        public Bitmap hitImage = Resource.madoka_hit_0;
        public bool[] lifeHearts { get; set; }
        public int lifePointer { get; set; }
        public int inmunityCntT { get; set; }
        int inmunityTime = 120;
        public bool isDamaged = false;
        public Vector2 PreviousPosition { get; set; }

        public DinoPencil dinoPencil { get; set; }

        public bool isRunning { get; set; }
        public int runningCntT { get; set; }

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

            var height = 60;
            var width = 40;
            var x = (int)Position.X;
            var y = (int)Position.Y;

            var particles = new List<Particle>
            {
                new Particle(new Vector2(x - width / 2, y - height / 2), 2, 'g'), // top left
                new Particle(new Vector2(x + width / 2, y - height / 2), 2, 'g'), // top right
                new Particle(new Vector2(x + width / 2, y + height / 2), 2, 'g'), // bottom right
                new Particle(new Vector2(x - width / 2, y + height / 2), 2, 'g'), // bottom left
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
            StandingImageOrientation = Orientation.Left;
            HitImageOrientation = Orientation.Left;
            lifeHearts = [true, true, true, true, true];
            lifePointer = 4;
        }

        public void Update(int width, int height, List<Polygon> worldPolygons)
        {
            PreviousPosition = new Vector2(Position.X, Position.Y);
            Position = formKeeper.Center;
            CameraPosition = new Vector2(Position.X-200, Position.Y-500);
            polygon.Update(width, height);
            formKeeper.RestoreOriginalForm();

            dinoPencil.Update(width, height, worldPolygons);

            if (isDamaged && inmunityCntT < inmunityTime)
            {
                inmunityCntT++;
            }
            else
            {
                isDamaged = false;
            }
            CheckIfItsMoving();
        }

        private void CheckIfItsMoving()
        {
            if (Position.X > PreviousPosition.X - 0.3f && Position.X < PreviousPosition.X + 0.3f)
            {
                isRunning = false;
                PreviousPosition.X = Position.X;
            }
            else
            {
                isRunning = true;
            }
        }

        public void MoveRight()
        {
            int speed = 1;
            LeftLeg.Position += new Vector2(speed, 0);
            RightLeg.Position += new Vector2(speed, 0);
            Orientation = Orientation.Right;
        }

        public void MoveLeft()
        {
            int speed = 1;
            LeftLeg.Position += new Vector2(-speed, 0);
            RightLeg.Position += new Vector2(-speed, 0);
            Orientation = Orientation.Left;
        }

        public void Jump()
        {
            if (LeftLeg.IsInGround && RightLeg.IsInGround)
            {
                LeftLeg.Position += new Vector2(0, -10);
                RightLeg.Position += new Vector2(0, -10);
            }
        }

        public void RemoveHearts()
        {
            if (lifePointer >= 0 && !isDamaged)
            {
                isDamaged = true;
                lifeHearts[lifePointer--] = false;
                inmunityCntT = 0;
            }
        }
    }
}
