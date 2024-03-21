using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace GolfIt
{
    public class MovingFloor : Obstacle
    {
        public Vector position;
        public Direction direction;
        private int cellSize;
        private float floorForce;
        private Point[] points;

        public MovingFloor(Vector position, Direction direction, int cellSize)
        {
            this.position = position;
            this.direction = direction;
            this.cellSize = cellSize;
            this.floorForce = 0.15f;

            points = new Point[6];

            CalculateArrowPoints();
        }

        public Vector DetectCollision(Ball ball)
        { 
            var velocity = new Vector(0, 0);

            if (!Collided(ball))
            {
                return velocity;
            }

            switch (direction)
            {
                case Direction.Up:
                    velocity.Y = -floorForce;
                    break;
                case Direction.Down:
                    velocity.Y = floorForce;
                    break;
                case Direction.Left:
                    velocity.X = -floorForce;
                    break;
                case Direction.Right:
                    velocity.X = floorForce;
                    break;
            }

            return velocity;
        }

        public bool Collided(Ball ball)
        {
            bool top = IsMovingFloor((int)(ball.position.X), (int)(ball.position.Y - ball.radius));
            bool bottom = IsMovingFloor((int)(ball.position.X), (int)(ball.position.Y + ball.radius));
            bool left = IsMovingFloor((int)(ball.position.X - ball.radius), (int)(ball.position.Y));
            bool right = IsMovingFloor((int)(ball.position.X + ball.radius), (int)(ball.position.Y));
            bool topLeft = IsMovingFloor((int)(ball.position.X - ball.radius), (int)(ball.position.Y - ball.radius));
            bool topRight = IsMovingFloor((int)(ball.position.X + ball.radius), (int)(ball.position.Y - ball.radius));
            bool bottomLeft = IsMovingFloor((int)(ball.position.X - ball.radius), (int)(ball.position.Y + ball.radius));
            bool bottomRight = IsMovingFloor((int)(ball.position.X + ball.radius), (int)(ball.position.Y + ball.radius));

            return top || bottom || left || right || topLeft || topRight || bottomLeft || bottomRight;
        }

        public bool IsMovingFloor(int x, int y)
        {
            int gridX = x / cellSize;
            int gridY = y / cellSize;


            return gridX == position.X && gridY == position.Y;
        }

        public void Render(Graphics g, PictureBox canvas)
        {
            g.FillRectangle(Brushes.Yellow, position.X * cellSize, position.Y * cellSize, cellSize, cellSize);
            g.FillPolygon(Brushes.Orange, points);
        }
        public void Update(Graphics g, PictureBox canvas, Map map, int cntT)
        {
            Render(g, canvas);
        }

        private Point[] CalculateArrowPoints()
        {
            int halfCell = cellSize / 2;
            int x = (int)position.X;
            int y = (int)position.Y;

            switch (direction)
            {
                case Direction.Up:
                    points[0] = new Point(x * cellSize, (y + 1) * cellSize);
                    points[1] = new Point(x * cellSize + halfCell, y * cellSize + halfCell);
                    points[2] = new Point((x + 1) * cellSize, (y + 1) * cellSize);
                    points[3] = new Point((x + 1) * cellSize, y * cellSize + halfCell);
                    points[4] = new Point(x * cellSize + halfCell, y * cellSize);
                    points[5] = new Point(x * cellSize, y * cellSize + halfCell);
                    break;
                case Direction.Down:
                    points[0] = new Point(x * cellSize, y * cellSize);
                    points[1] = new Point(x * cellSize + halfCell, y * cellSize + halfCell);
                    points[2] = new Point((x + 1) * cellSize, y * cellSize);
                    points[3] = new Point((x + 1) * cellSize, y * cellSize + halfCell);
                    points[4] = new Point(x * cellSize + halfCell, (y + 1) * cellSize);
                    points[5] = new Point(x * cellSize, y * cellSize + halfCell);
                    break;
                case Direction.Left:
                    points[0] = new Point((x + 1) * cellSize, y * cellSize);
                    points[1] = new Point(x * cellSize + halfCell, y * cellSize + halfCell);
                    points[2] = new Point((x + 1) * cellSize, (y + 1) * cellSize);
                    points[3] = new Point(x * cellSize + halfCell, (y + 1) * cellSize);
                    points[4] = new Point(x * cellSize, y * cellSize + halfCell);
                    points[5] = new Point(x * cellSize + halfCell, y * cellSize);
                    break;
                case Direction.Right:
                    points[0] = new Point(x * cellSize, y * cellSize);
                    points[1] = new Point(x * cellSize + halfCell, y * cellSize + halfCell);
                    points[2] = new Point(x * cellSize, (y + 1) * cellSize);
                    points[3] = new Point(x * cellSize + halfCell, (y + 1) * cellSize);
                    points[4] = new Point((x + 1) * cellSize, y * cellSize + halfCell);
                    points[5] = new Point(x * cellSize + halfCell, y * cellSize);
                    break;
            }

            return points;
        }

    }

    public enum Direction
    {
        Up, Down, Left, Right,
    }
}
