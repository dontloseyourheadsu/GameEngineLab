namespace GolfIt
{
    public class Triangle : Obstacle
    {
        private float cellSize;
        private Vector position;
        private Point[] points;
        Color color;
        Brush brush;
        int width, height;
        private int rotationSpeed = 2000;

        private Point[] originalPoints;

        public Triangle(int cellSize, Vector position)
        {
            this.cellSize = cellSize;
            this.position = position * cellSize;
            this.color = Color.LightBlue;
            this.brush = new SolidBrush(color);
            this.width = 5 * cellSize;
            this.height = 5 * cellSize;

            originalPoints = new Point[]
            {
                new Point(0, -height / 2),
                new Point(-width / 2, height / 2),
                new Point(width / 2, height / 2)
            };

            points = new Point[3];
        }

        public void Update(Graphics g, PictureBox canvas, Map map, int cntT)
        {
            float angleDegrees = (cntT % rotationSpeed) * (360.0f / rotationSpeed);
            float angleRadians = (float)(Math.PI / 180) * angleDegrees;

            for (int i = 0; i < originalPoints.Length; i++)
            {
                float x = originalPoints[i].X;
                float y = originalPoints[i].Y;

                points[i] = new Point(
                    (int)(position.X + x * Math.Cos(angleRadians) - y * Math.Sin(angleRadians)),
                    (int)(position.Y + x * Math.Sin(angleRadians) + y * Math.Cos(angleRadians))
                );
            }

            Render(g, canvas);
        }

        public Vector DetectCollision(Ball ball)
        {
            Vector collisionNormal = new Vector(0, 0);
            Vector[] vertices = new Vector[3];

            for (int i = 0; i < 3; i++)
            {
                vertices[i] = new Vector(points[i].X, points[i].Y);
            }

            for (int i = 0; i < 3; i++)
            {
                Vector edge = vertices[(i + 1) % 3] - vertices[i];
                Vector edgeNormal = new Vector(-edge.Y, edge.X);
                edgeNormal.Normalized();

                Vector ballToVertex = ball.position - vertices[i];
                float distance = ballToVertex.Dot(edgeNormal);

                if (distance > 0)
                {
                    Vector vertexToBall = ball.position - vertices[i];
                    float dotProduct = vertexToBall.Dot(edge);
                    float edgeLength = edge.Length();
                    float edgeLengthSquared = edgeLength * edgeLength;
                    float t = Math.Max(0, Math.Min(1, dotProduct / edgeLengthSquared));

                    Vector closestPoint = vertices[i] + edge * t;
                    Vector distanceVector = ball.position - closestPoint;
                    float distanceSquared = distanceVector.Length() * distanceVector.Length();

                    if (distanceSquared < ball.radius * ball.radius)
                    {
                        collisionNormal = distanceVector;
                        collisionNormal.Normalized();
                        return collisionNormal;
                    }
                }
            }

            return collisionNormal;
        }

        public void Render(Graphics g, PictureBox canvas)
        {
            g.FillPolygon(brush, points);
        }

        public void SetRotationSpeed(int newSpeed)
        {
            rotationSpeed = newSpeed;
        }
    }

}
