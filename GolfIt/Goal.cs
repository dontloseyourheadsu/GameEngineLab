namespace GolfIt
{

    public class Goal : Verlet
    {
        private float cellSize;
        private Vector position;
        Color color;
        public bool isBallInGoal = false;
        Brush brush;
        private Ball ball;

        public Goal(int cellSize, Vector position, Ball ball)
        {
            this.cellSize = cellSize * 1.25f;
            this.position = position;
            this.color = Color.Black;
            this.brush = new SolidBrush(color);
            this.ball = ball;
        }

        public void Update(Graphics g, PictureBox canvas)
        {
            if (Math.Pow(ball.position.X - position.X, 2) + Math.Pow(ball.position.Y - position.Y, 2) < Math.Pow(cellSize / 2, 2))
            {
                isBallInGoal = true;
            }
            else
            {
                isBallInGoal = false;
            }

            Render(g);
        }

        public void Render(Graphics g)
        {
            g.FillEllipse(brush, position.X - cellSize / 2, position.Y - cellSize / 2, cellSize, cellSize);
        }
    }
}
