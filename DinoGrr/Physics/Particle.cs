namespace DinoGrr.Physics
{
    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Mass { get; set; }
        public bool IsInGround { get; set; }

        public Particle(Vector2 position, float mass)
        {
            Position = position;
            PreviousPosition = position;
            Mass = mass;
        }

        public void Update(int subStep, float gravity)
        {
            IsInGround = false;
            var velocity = new Vector2(2 * Position.X - PreviousPosition.X, 2 * Position.Y - PreviousPosition.Y);

            var force = new Vector2(0.0f, gravity / subStep);
            var acceleration = new Vector2(force.X / Mass, force.Y / Mass);
            var prevPosition = new Vector2(Position.X, Position.Y);
            var deltaTimeSquared = (1.0f / subStep) * (1.0f / subStep);

            Position = new Vector2(velocity.X + acceleration.X * deltaTimeSquared,
                               velocity.Y + acceleration.Y * deltaTimeSquared
                               );
            PreviousPosition = prevPosition;
        }

        public void KeepInsideCanvas(int width, int height)
        {
            if (Position.Y >= height)
            {
                Position.Y = height;
                IsInGround = true;
            }
            if (Position.X >= width)
            {
                Position.X = width;
            }
            if (Position.Y < 0)
            {
                Position.Y = 0;
            }
            if (Position.X < 0)
            { 
                Position.X = 0; 
            }
        }
    }
}
