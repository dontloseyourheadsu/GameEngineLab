using DinoGrr.Physics;

namespace DinoGrr.Rendering
{
    public class Render
    {
        private Graphics graphics;
        public Render(Graphics g)
        {
            graphics = g;
        }

        public void DrawParticle(Particle particle)
        {
            graphics.FillEllipse(Brushes.Orange, particle.Position.X - particle.Mass, particle.Position.Y - particle.Mass, particle.Mass * 2, particle.Mass * 2);
        }

        public void DrawStick(Stick stick)
        {
            var pen = new Pen(Color.Orange, stick.A.Mass * 2);
            graphics.DrawLine(pen, stick.A.Position.X, stick.A.Position.Y, stick.B.Position.X, stick.B.Position.Y);
        }

        public void DrawPolygon(Polygon polygon)
        {
            foreach (var stick in polygon.sticks)
            {
                DrawStick(stick);
            }
        }

        public void DrawDinosaur(Dinosaur dinosaur)
        {
            foreach (var stick in dinosaur.polygon.sticks)
            {
                DrawStick(stick);
            }
        }
    }
}
