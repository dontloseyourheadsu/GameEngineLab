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

        public void Update(Graphics g, PictureBox canvas)
        {
            if (!isPinned)
            {
                Vector newPosition;

                // Apply the push velocity once and reset it
                acceleration = pushVelocity / mass;
                pushVelocity = new Vector(0, 0); // Reset pushVelocity after applying it

                // Verlet Integration
                newPosition = position + (position - oldPosition) + acceleration;
                oldPosition = position;
                position = newPosition;

                // Apply friction to simulate energy loss (damping)
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
            float speedThreshold = 0.01f; // Define a suitable threshold for your game
            return currentVelocity.Length() > speedThreshold;
        }

    }
}
