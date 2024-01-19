using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Collision
{
    internal class Ball
    {
        public float diameter, radio;
        public Point pos;
        public int impulseX, impulseY;
        public int speedX, speedY;
        public PictureBox containerSpace;
        private List<PictureBox> _obstacles;

        public Ball(Random rand, PictureBox size, List<PictureBox> obstacles)
        {
            containerSpace = size;
            _obstacles = obstacles;
            diameter = rand.Next(15, 70);
            pos = new Point(rand.Next(0, size.Width - (int) diameter), rand.Next(0, size.Height - (int) diameter));
            radio = diameter / 2;
            impulseX = rand.Next(-5, 5);
            impulseY = rand.Next(-5, 5);
            speedX = Math.Abs(impulseX);
            speedY = Math.Abs(impulseY);
        }

        /// <summary>
        /// Renders the ball and the obstacles
        /// </summary>
        /// <param name="g">Graphics object to render to</param>
        public void Render(Graphics g)
        {
            g.FillEllipse(Brushes.Yellow, pos.X - radio, pos.Y - radio, diameter, diameter);
            g.DrawEllipse(Pens.AliceBlue, pos.X - radio, pos.Y - radio, diameter, diameter);
            g.FillEllipse(Brushes.Gray, pos.X - 2, pos.Y - 2, 4, 4);

            foreach (PictureBox obstacle in _obstacles)
            {
                g.FillRectangle(Brushes.White, obstacle.Location.X, obstacle.Location.Y, obstacle.Size.Width - speedX, obstacle.Size.Height - speedY);
            }
        }

        private void CalculateContainerCollition()
        {
            if (pos.X + radio >= containerSpace.Width)
            {
                impulseX = -speedX;
            }
            else if (pos.X - radio <= 0)
            {
                impulseX = speedX;
            }
            if (pos.Y + radio >= containerSpace.Height)
            {
                impulseY = -speedY;
            }
            else if (pos.Y - radio <= 0)
            {
                impulseY = speedY;
            }
        }

        /// <summary>
        /// Clamps a value between a minimum and a maximum
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>The clamped value</returns>
        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        /// <summary>
        /// Calculates collisions with obstacles
        /// </summary>
        private void CalculateObstacleCollisions()
        {
            foreach (PictureBox obstacle in _obstacles)
            {
                // Defining the hitbox for the obstacle
                Rectangle obstacleHitbox = new Rectangle(obstacle.Location, obstacle.Size);

                // Calculating the closest point on the obstacle's hitbox to the center of the ball
                int closestX = Clamp(pos.X, obstacleHitbox.Left, obstacleHitbox.Right);
                int closestY = Clamp(pos.Y, obstacleHitbox.Top, obstacleHitbox.Bottom);

                // Calculating the distance from this point to the center of the ball
                int distanceX = pos.X - closestX;
                int distanceY = pos.Y - closestY;

                // Check for collision (distance less than the ball's radius)
                if ((distanceX * distanceX) + (distanceY * distanceY) < (radio * radio))
                {
                    // Collision detected

                    // Decide on how to change the ball's movement based on the side of collision
                    if (pos.X < obstacleHitbox.Left || pos.X > obstacleHitbox.Right)
                    {
                        impulseX = -impulseX; // Horizontal collision
                    }
                    if (pos.Y < obstacleHitbox.Top || pos.Y > obstacleHitbox.Bottom)
                    {
                        impulseY = -impulseY; // Vertical collision
                    }

                    // Adjusting the position of the ball to avoid sticking into the obstacle
                    pos.X += impulseX;
                    pos.Y += impulseY;
                }
            }
        }

        /// <summary>
        /// Checks if the ball is colliding with an obstacle
        /// </summary>
        /// <param name="obstacle">Obstacle to check collision with</param>
        /// <returns>True if the ball is colliding with the obstacle, false otherwise</returns>
        public bool IsCollidingWithBox(PictureBox obstacle)
        {
            Rectangle obstacleHitbox = new Rectangle(obstacle.Location, obstacle.Size);

            int closestX = Clamp(pos.X, obstacleHitbox.Left, obstacleHitbox.Right);
            int closestY = Clamp(pos.Y, obstacleHitbox.Top, obstacleHitbox.Bottom);

            int distanceX = pos.X - closestX;
            int distanceY = pos.Y - closestY;

            if ((distanceX * distanceX) + (distanceY * distanceY) < (radio * radio))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates collisions with the container and obstacles
        /// </summary>
        private void CalculateCollisions()
        {
            CalculateContainerCollition();
            CalculateObstacleCollisions();
        }

        /// <summary>
        /// Updates the ball's position and calculates collisions
        /// </summary>
        public void Update()
        {
            CalculateCollisions();

            pos.X += impulseX;
            pos.Y += impulseY;
        }
    }
}
