using DinoGrr.Physics;

namespace DinoGrr.Rendering
{

    public class Render
    {
        public Graphics Graphics;
        public Camera Camera;

        public Render(Graphics graphics, Camera camera)
        {
            Graphics = graphics;
            Camera = camera;
        }

        public void DrawParticle(Particle particle)
        {
            var worldPosition = new Point((int)(particle.Position.X - particle.Mass), (int)(particle.Position.Y - particle.Mass));
            var viewPosition = Camera.TranslateToView(worldPosition);

            if (Camera.IsVisible(worldPosition))
            {
                Graphics.FillEllipse(Brushes.Orange, viewPosition.X, viewPosition.Y, particle.Mass * 2, particle.Mass * 2);
            }
        }

        public void DrawStick(Stick stick)
        {
            var viewPositionA = Camera.TranslateToView(new Point((int)stick.A.Position.X, (int)stick.A.Position.Y));
            var viewPositionB = Camera.TranslateToView(new Point((int)stick.B.Position.X, (int)stick.B.Position.Y));

            var pen = new Pen(Color.Orange, stick.A.Mass * 2);
            if (Camera.IsVisible(new Point((int)stick.A.Position.X, (int)stick.A.Position.Y)) || Camera.IsVisible(new Point((int)stick.B.Position.X, (int)stick.B.Position.Y)))
            {
                Graphics.DrawLine(pen, viewPositionA.X, viewPositionA.Y, viewPositionB.X, viewPositionB.Y);
            }
        }

        public void DrawPolygon(Polygon polygon)
        {
            foreach (Stick stick in polygon.sticks)
            {
                DrawStick(stick);
            }
        }

        public void DrawDinosaur(Dinosaur dinosaur)
        {
            var width = dinosaur.polygon.particles[1].Position.X - dinosaur.polygon.particles[0].Position.X;
            var height = dinosaur.polygon.particles[3].Position.Y - dinosaur.polygon.particles[0].Position.Y;
            var centerView = Camera.TranslateToView(new Point((int)dinosaur.formKeeper.Center.X, (int)dinosaur.formKeeper.Center.Y));

            if (dinosaur.Orientation == Orientation.Right && dinosaur.ImageOrientation != Orientation.Right)
            {
                dinosaur.image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                dinosaur.ImageOrientation = Orientation.Right;
            }

            if (Camera.IsVisible(new Point((int)dinosaur.formKeeper.Center.X, (int)dinosaur.formKeeper.Center.Y)))
            {
                Graphics.DrawImage(dinosaur.image, centerView.X - width / 2, centerView.Y - height / 2, width, height);
            }
        }

        public void DrawRaycast(Vector2 start, Vector2 end)
        {
            var viewStart = Camera.TranslateToView(new Point((int)start.X, (int)start.Y));
            var viewEnd = Camera.TranslateToView(new Point((int)end.X, (int)end.Y));

            var pen = new Pen(Color.Red, 1);
            if (Camera.IsVisible(new Point((int)start.X, (int)start.Y)) || Camera.IsVisible(new Point((int)end.X, (int)end.Y)))
            {
                Graphics.DrawLine(pen, viewStart.X, viewStart.Y, viewEnd.X, viewEnd.Y);
                Graphics.FillEllipse(Brushes.Red, viewStart.X - 5, viewStart.Y - 5, 10, 10);
            }
        }

        public void DrawBounds(int Width, int Height)
        {
            // draw the bounds with black lines
            var pen = new Pen(Color.Black, 1);
            var viewTopLeft = Camera.TranslateToView(new Point(0, 0));
            var viewTopRight = Camera.TranslateToView(new Point(Width, 0));
            var viewBottomLeft = Camera.TranslateToView(new Point(0, Height));
            var viewBottomRight = Camera.TranslateToView(new Point(Width, Height));

            Graphics.DrawLine(pen, viewTopLeft.X, viewTopLeft.Y, viewTopRight.X, viewTopRight.Y);
            Graphics.DrawLine(pen, viewTopRight.X, viewTopRight.Y, viewBottomRight.X, viewBottomRight.Y);
            Graphics.DrawLine(pen, viewBottomRight.X, viewBottomRight.Y, viewBottomLeft.X, viewBottomLeft.Y);
            Graphics.DrawLine(pen, viewBottomLeft.X, viewBottomLeft.Y, viewTopLeft.X, viewTopLeft.Y);
        }
    }
}
