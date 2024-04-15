namespace DinoGrr.Physics
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public Vector2 Normalized()
        {
            float magnitude = Magnitude();
            return new Vector2(X / magnitude, Y / magnitude);
        }

        public float Dot(Vector2 other)
        {
            return X * other.X + Y * other.Y;
        }

        public float GetDistance(Vector2 other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2 operator *(Vector2 a, float b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }

        public static Vector2 operator /(Vector2 a, float b)
        {
            return new Vector2(a.X / b, a.Y / b);
        }
    }

    public static class Vector2Extensions
    {
        public static Vector2 Add(this Vector2 a, Vector2 b)
        {
            return a + b;
        }

        public static Vector2 Subtract(this Vector2 a, Vector2 b)
        {
            return a - b;
        }

        public static Vector2 Multiply(this Vector2 a, Vector2 b)
        {
            return a * b;
        }

        public static Vector2 Divide(this Vector2 a, Vector2 b)
        {
            return a / b;
        }

        public static Vector2 Multiply(this Vector2 a, float b)
        {
            return a * b;
        }

        public static Vector2 Divide(this Vector2 a, float b)
        {
            return a / b;
        }
    }

}
