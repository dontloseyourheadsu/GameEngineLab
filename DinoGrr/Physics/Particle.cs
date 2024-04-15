namespace DinoGrr.Physics
{
    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Mass { get; set; }
        public bool IsInGround { get; set; }
        public bool Locked { get; set; }
        public bool IsCollisionActive { get; set; }

        public Particle(Vector2 position, float mass)
        {
            Position = position;
            PreviousPosition = position;
            Mass = mass;
            IsCollisionActive = true;
        }

        public void Update(int subStep, float gravity)
        {
            IsInGround = false;

            if (Locked)
            {
                return;
            }

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

        public void CheckPolygonCollision(Polygon polygon)
        {
            if (!IsCollisionActive)
            {
                return;
            }

            var leftIntersectionCount = RayCasting.GetRayCastingCount(
                                    Position, polygon.sticks, rayLimit: 0);

            if (leftIntersectionCount % 2 != 0)
            {
                HandlePolygonCollision(polygon);
            }
        }

        private void HandlePolygonCollision(Polygon polygon)
        {
            var closestEdge = RayCasting.GetClosestEdge(Position, polygon.sticks);
            Stick edge = closestEdge.Item1;
            Vector2 closestPoint = closestEdge.Item2;

            Position = closestPoint + (Position - closestPoint) * 1f;
            PreviousPosition = closestPoint + (Position - closestPoint) * 0.8f;
        }

        public void ParticleCollision(Particle otherParticle)
        {
            if (!IsCollisionActive)
            {
                return;
            }

            var distance = Position.GetDistance(otherParticle.Position);
            var sumRadius = Mass + otherParticle.Mass;

            if (distance < sumRadius)
            {
                var collisionVector = new Vector2(Position.X - otherParticle.Position.X,
                                                                      Position.Y - otherParticle.Position.Y);

                var collisionVectorNormalized = new Vector2(collisionVector.X / distance, collisionVector.Y / distance);

                var newPosition = new Vector2(Position.X + collisionVectorNormalized.X,
                                                                 Position.Y + collisionVectorNormalized.Y);

                Position = newPosition;
            }
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
