namespace GolfIt
{
    public class Vector
    {
        public float X, Y;

        public Vector(float x, float y)
        {
            this.Y = (float)y;
            this.X = (float)x;

        }


        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y);
        }


        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator *(float a, Vector v)
        {
            return new Vector(a * v.X, a * v.Y);
        }

        public static Vector operator *(Vector v, float a)
        {
            return new Vector(a * v.X, a * v.Y);
        }

        public static Vector operator /(Vector v, float a)
        {
            return new Vector(v.X / a, v.Y / a);
        }

        public float MagSQR()
        {
            float f = (X * X) + (Y * Y);
            return f;
        }

        public float Length()
        {
            float f = (float)Math.Sqrt((X * X) + (Y * Y));
            return f;
        }


        public float Distance(Vector a)
        {
            float f = (float)Math.Sqrt(Math.Pow(X - a.X, 2) + Math.Pow(Y - a.Y, 2));
            return f;
        }

        public Vector Normalized()
        {
            float magnitude = Length();
            if (magnitude == 0) throw new InvalidOperationException("Cannot normalize a zero vector.");
            return this / magnitude;
        }
    }
}
