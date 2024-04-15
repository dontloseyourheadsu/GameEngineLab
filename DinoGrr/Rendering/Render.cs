using DinoGrr.Physics;

namespace DinoGrr.Rendering
{
    public class Render
    {
        public Graphics graphics;
        Camera camera;

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
            for (int i = 0; i < polygon.sticks.Count; i++)
            {
                Stick? stick = polygon.sticks[i];
                DrawStick(stick);
            }
        }

        public void DrawDinosaur(Dinosaur dinosaur)
        {
            var width = dinosaur.polygon.particles[1].Position.X - dinosaur.polygon.particles[0].Position.X;
            var height = dinosaur.polygon.particles[3].Position.Y - dinosaur.polygon.particles[0].Position.Y;
     
            if (dinosaur.Orientation == Orientation.Right && dinosaur.ImageOrientation != Orientation.Right)
            {
                dinosaur.image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                dinosaur.ImageOrientation = Orientation.Right;
            }

            graphics.DrawImage(dinosaur.image,
                dinosaur.formKeeper.Center.X - dinosaur.Width / 2,
                dinosaur.formKeeper.Center.Y - dinosaur.Height / 2,
                width, height);
        }

        public void DrawRaycast(Vector2 start, Vector2 end)
        {
            var pen = new Pen(Color.Red, 1);
            graphics.DrawLine(pen, start.X, start.Y, end.X, end.Y);
            graphics.FillEllipse(Brushes.Red, start.X - 5, start.Y - 5, 10, 10);
        }
    }
}
