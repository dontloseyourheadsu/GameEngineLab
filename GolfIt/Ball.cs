using System.Drawing;

namespace GolfIt
{
    public class Ball : Verlet
    {
        private int cellSize;
        public Vector position;
        public Vector oldPosition;
        private Vector velocity;
        private Vector acceleration;
        private Vector pushVelocity;
        float friction = 0.99f;
        public float radius, diameter;
        Color color;
        public float mass = 1.0f;
        Brush brush;
        public bool isPinned;

        public Ball(int cellSize, Vector position, Vector velocity)
        {
            this.cellSize = cellSize;
            this.position = position;
            this.oldPosition = position;
            this.velocity = velocity;
            this.acceleration = new Vector(0, 0);
            this.pushVelocity = new Vector(0, 0);
            this.color = Color.White;
            this.brush = new SolidBrush(color);
        }

        public void Update(Graphics g, PictureBox canvas, Map map)
        {
            if (!isPinned)
            {
                Vector newPosition;

                acceleration = pushVelocity / mass;
                pushVelocity = new Vector(0, 0);

                newPosition = position + (position - oldPosition) + acceleration;
                oldPosition = position;
                position = newPosition;

                Vector velocity = position - oldPosition;
                velocity *= friction;
                position = oldPosition + velocity;

                if (position.X < 0 || position.X > canvas.Width)
                {
                    velocity.X = -velocity.X;
                }
                if (position.Y < 0 || position.Y > canvas.Height)
                {
                    velocity.Y = -velocity.Y;
                }

                Collision collision = CheckWallCollision(map);

                if (collision == Collision.TopLeft)
                {
                    velocity.X = Math.Abs(velocity.X);
                    velocity.Y = Math.Abs(velocity.Y);
                }
                else if (collision == Collision.TopRight)
                {
                    velocity.X = -Math.Abs(velocity.X);
                    velocity.Y = Math.Abs(velocity.Y);
                }
                else if (collision == Collision.BottomLeft)
                {
                    velocity.X = Math.Abs(velocity.X);
                    velocity.Y = -Math.Abs(velocity.Y);
                }
                else if (collision == Collision.BottomRight)
                {
                    velocity.X = -Math.Abs(velocity.X);
                    velocity.Y = -Math.Abs(velocity.Y);
                }

                position = oldPosition + velocity;
            }

            Render(g);
        }


        public void PushBall(Vector pushVelocity)
        {
            this.pushVelocity = pushVelocity;
        }

        public void Render(Graphics g)
        {
            int borderRadius = 2;
            g.FillEllipse(Brushes.Black, position.X - cellSize / 2 - borderRadius, position.Y - cellSize / 2 - borderRadius, cellSize + borderRadius * 2, cellSize + borderRadius * 2);
            g.FillEllipse(brush, position.X - cellSize / 2, position.Y - cellSize / 2, cellSize, cellSize);
            g.FillPie(Brushes.Gray, position.X - cellSize / 2, position.Y - cellSize / 2, cellSize, cellSize, 0, 90);
            g.FillEllipse(brush, position.X - cellSize / 2 + borderRadius / 2, position.Y - cellSize / 2 + borderRadius / 2, cellSize - borderRadius, cellSize - borderRadius);
        }

        public bool IsMoving()
        {
            Vector currentVelocity = position - oldPosition;
            float speedThreshold = 0.01f;
            return currentVelocity.Length() > speedThreshold;
        }

        public Collision CheckWallCollision(Map map)
        {
            float radius = cellSize / 2.0f + 2; 

            bool topLeft = map.IsWall((int)(position.X - radius), (int)(position.Y - radius));
            bool topRight = map.IsWall((int)(position.X + radius), (int)(position.Y - radius));
            bool bottomLeft = map.IsWall((int)(position.X - radius), (int)(position.Y + radius));
            bool bottomRight = map.IsWall((int)(position.X + radius), (int)(position.Y + radius));

            if (topLeft) return Collision.TopLeft;
            if (topRight) return Collision.TopRight;
            if (bottomLeft) return Collision.BottomLeft;
            if (bottomRight) return Collision.BottomRight;
            return Collision.None;
        }

    }
}
