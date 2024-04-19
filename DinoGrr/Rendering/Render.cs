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
            for (int i = 0; i < polygon.sticks.Count; i++)
            {
                Stick stick = polygon.sticks[i];
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

            if (dinosaur.Orientation == Orientation.Left && dinosaur.ImageOrientation != Orientation.Left)
            {
                dinosaur.image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                dinosaur.ImageOrientation = Orientation.Left;
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
            var pen = new Pen(Color.Black, 5);
            var viewTopLeft = Camera.TranslateToView(new Point(0, 0));
            var viewTopRight = Camera.TranslateToView(new Point(Width, 0));
            var viewBottomLeft = Camera.TranslateToView(new Point(0, Height));
            var viewBottomRight = Camera.TranslateToView(new Point(Width, Height));

            Graphics.DrawLine(pen, viewTopLeft.X, viewTopLeft.Y, viewTopRight.X, viewTopRight.Y);
            Graphics.DrawLine(pen, viewTopRight.X, viewTopRight.Y, viewBottomRight.X, viewBottomRight.Y);
            Graphics.DrawLine(pen, viewBottomRight.X, viewBottomRight.Y, viewBottomLeft.X, viewBottomLeft.Y);
            Graphics.DrawLine(pen, viewBottomLeft.X, viewBottomLeft.Y, viewTopLeft.X, viewTopLeft.Y);
        }

        public void DrawOutsideBounds(int Width, int Height)
        {
            var brush = new SolidBrush(Color.White);
            var viewTopLeft = Camera.TranslateToView(new Point(0, 0));
            var viewTopRight = Camera.TranslateToView(new Point(Width, 0));
            var viewBottomLeft = Camera.TranslateToView(new Point(0, Height));
            var extension = 200;

            Graphics.FillRectangle(brush, viewTopLeft.X - extension, viewTopLeft.Y, extension - 2.5f, Height + extension);
            Graphics.FillRectangle(brush, viewTopRight.X, viewTopRight.Y, extension * 3, Height + extension);
            Graphics.FillRectangle(brush, viewBottomLeft.X - extension, viewBottomLeft.Y + 1f, Width + extension*2, extension);

        }

        public void DrawGirl(Player player, int cntT)
        {
            var width = player.polygon.particles[1].Position.X - player.polygon.particles[0].Position.X;
            var height = player.polygon.particles[3].Position.Y - player.polygon.particles[0].Position.Y;
            var centerView = Camera.TranslateToView(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y));

            Bitmap image;
            if (player.isDamaged)
            {
                if (cntT % 30  < 15)
                {
                    image = player.hitImage;
                }
                else
                {
                    image = new Bitmap(1, 1);
                }

                if (player.Orientation == Orientation.Right && player.HitImageOrientation != Orientation.Right)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    player.HitImageOrientation = Orientation.Right;
                }

                if (player.Orientation == Orientation.Left && player.HitImageOrientation != Orientation.Left)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    player.HitImageOrientation = Orientation.Left;
                }
            }
            else if (player.isRunning)
            {
                image = player.runningImages[player.runningCntT % 8];

                if (player.Orientation == Orientation.Right && player.RunningImagesOrientations[player.runningCntT % 8] != Orientation.Right)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    player.RunningImagesOrientations[player.runningCntT % 8] = Orientation.Right;
                }
                if (player.Orientation == Orientation.Left && player.RunningImagesOrientations[player.runningCntT % 8] != Orientation.Left)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    player.RunningImagesOrientations[player.runningCntT % 8] = Orientation.Left;
                }

                if (cntT % 3 == 0)
                {
                    player.runningCntT++;
                }
            }
            else
            {
                image = player.standingImage;
                player.runningCntT = 0;

                if (player.Orientation == Orientation.Right && player.StandingImageOrientation != Orientation.Right)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    player.StandingImageOrientation = Orientation.Right;
                }

                if (player.Orientation == Orientation.Left && player.StandingImageOrientation != Orientation.Left)
                {
                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    player.StandingImageOrientation = Orientation.Left;
                }
            }

            if (Camera.IsVisible(new Point((int)player.formKeeper.Center.X, (int)player.formKeeper.Center.Y)))
            {
                Graphics.DrawImage(image, centerView.X - width / 2, centerView.Y - height / 2, width, height);
            }
        }

        public void DrawBackground(Background background)
        {
            var centerView1 = Camera.TranslateToView(new Point((int)background.l1_X1, 50));

            var center2View0 = Camera.TranslateToView(new Point((int)background.l2_X0, background.height / 2 + 15));
            var center2View1 = Camera.TranslateToView(new Point((int)background.l2_X1, background.height / 2 + 15));
            var center2View2 = Camera.TranslateToView(new Point((int)background.l2_X2, background.height / 2 + 15));

            Graphics.DrawImage(background.layer1, centerView1.X - 200, centerView1.Y, background.layer1.Width, background.height);

            Graphics.DrawImage(background.layer2, center2View0.X, center2View0.Y, background.layer2.Width + 225, background.height / 3);
            Graphics.DrawImage(background.layer2, center2View1.X, center2View1.Y, background.layer2.Width + 225, background.height / 3);
            Graphics.DrawImage(background.layer2, center2View2.X, center2View2.Y, background.layer2.Width + 225, background.height / 3);
        }

        public void DrawProgressBar(int width, int max, float value)
        {
            var centerView = Camera.TranslateToView(new Point(width - 200 - 20, 20));

            Graphics.FillRectangle(Brushes.White, centerView.X, centerView.Y, 200, 20);
            Graphics.FillRectangle(Brushes.Green, centerView.X, centerView.Y, 200 * value / max, 20);
            Graphics.DrawRectangle(new Pen(Color.Black, 3), centerView.X, centerView.Y, 200, 20);
        }

        public void DrawHearts(bool[] lifeHearts)
        {
            var x = 20;
            var y = 20;

            var centerView = Camera.TranslateToView(new Point(x, y));

            x = centerView.X;
            y = centerView.Y;

            var size = 30;
            for (int i = 0; i < lifeHearts.Length; i++)
            {
                if (lifeHearts[i])
                {
                    Graphics.DrawImage(Resource.dino_heart, x, y, size, size);
                }
                else
                {
                    Graphics.DrawImage(Resource.dino_heart_broken, x, y, size, size);
                }
                x += size + 10;
            }
        }

        public void DrawGoal(Goal goal)
        {
            var viewPosition = Camera.TranslateToView(new Point((int)goal.Position.X, (int)goal.Position.Y));
            var pen = new Pen(Color.Black, 3);
            var brush = new SolidBrush(Color.White);

            Graphics.FillRectangle(Brushes.Orange, viewPosition.X - goal.Width / 2, viewPosition.Y - goal.Height / 2, goal.Width, goal.Height);
            Graphics.DrawRectangle(pen, viewPosition.X - goal.Width / 2, viewPosition.Y - goal.Height / 2, goal.Width, goal.Height);
            // draw a diamond inside the rectangle of white
            Graphics.FillPolygon(brush, new Point[] { new Point(viewPosition.X, viewPosition.Y - goal.Height / 2), new Point(viewPosition.X + goal.Width / 2, viewPosition.Y), new Point(viewPosition.X, viewPosition.Y + goal.Height / 2), new Point(viewPosition.X - goal.Width / 2, viewPosition.Y) });
        }

        public void DrawWin(int Width, int Height)
        {
            var font = new Font("Arial", 50);
            var viewCenter = Camera.TranslateToView(new Point(Width / 2, Height / 2));
            Graphics.DrawString("You Win!", font, Brushes.Black, viewCenter.X - 100, viewCenter.Y - 25);
        }

        public void DrawLoose(int Width, int Height)
        {
            var font = new Font("Arial", 50);
            var viewCenter = Camera.TranslateToView(new Point(Width / 2 - 100, Height / 2));
            Graphics.DrawString("You Loose!", font, Brushes.Black, viewCenter.X - 100, viewCenter.Y - 25);
        }

        public void DrawPlatform(Vector2 position, int width, int height)
        {
            var viewPosition = Camera.TranslateToView(new Point((int)position.X, (int)position.Y));
            var pen = new Pen(Color.Black, 3);
            var brush = new SolidBrush(Color.White);

            Graphics.FillRectangle(brush, viewPosition.X, viewPosition.Y, width, height);
            Graphics.DrawRectangle(pen, viewPosition.X, viewPosition.Y, width, height);
        }
    }
}
