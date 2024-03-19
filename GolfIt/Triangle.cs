namespace GolfIt
{
    internal class Triangle : Obstacle
    {
        private float cellSize;
        private Vector position;
        private Point[] points;
        Color color;
        Brush brush;
        int width, height;

        public Triangle (int cellSize, Vector position)
        {
            this.cellSize = cellSize;
            this.position = position;
            this.color = Color.LightBlue;
            this.brush = new SolidBrush(color);
            this.width = 2 * cellSize;
            this.height = 2 * cellSize;

            points = new Point[3];
            points[0] = new Point((int)position.X, (int)position.Y);
            points[1] = new Point((int)position.X + width, (int)position.Y);
            points[2] = new Point((int)position.X + width / 2, (int)position.Y - height);
        }

        public Collision DetectCollision(Ball ball)
        {
            throw new NotImplementedException();
        }

        public void Render(Graphics g, PictureBox canvas)
        {
            g.FillPolygon(brush, points);
        }

        public void Update(Graphics g, PictureBox canvas, Map map, int cntT)
        {
            float rotatePercent = (float)cntT / 3;
            points[0] = new Point((int)position.X, (int)position.Y);
            points[1] = new Point((int)position.X + width, (int)position.Y);
            points[2] = new Point((int)position.X + width / 2, (int)position.Y - height);

            Render(g, canvas);
        }
    }
}
