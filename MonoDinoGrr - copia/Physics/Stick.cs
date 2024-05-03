using Microsoft.Xna.Framework;

namespace MonoDinoGrr.Physics
{
    public class Stick
    {
        public Particle A { get; private set; }
        public Particle B { get; private set; }
        public float Length { get; private set; }
        public float Stiffness { get; private set; }

        public Stick(Particle a, Particle b, float stiffness = 0.2f)
        {
            A = a;
            B = b;
            Length = Vector2.Distance(A.Position,B.Position);
            Stiffness = stiffness;
        }

        public void Update()
        {
            var diff = A.Position - B.Position;
            var diffLength = Vector2.Distance(A.Position, B.Position);
            var diffFactor = (Length - diffLength) / diffLength * Stiffness * 0.5f;
            var offset = diff * diffFactor;

            A.Position += offset;
            B.Position -= offset;
        }
    }
}
