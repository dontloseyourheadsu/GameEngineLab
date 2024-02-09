using System.Runtime.CompilerServices;

namespace Pacman
{
    internal class Pacman
    {
        public int speed = 30;
        public int x = 1;
        public int y = 1;
        public float prevX = 1;
        public float prevY = 1;
        public Direction direction = Direction.None;
        public Direction nextDirection = Direction.None;
        public int points = 0;
        public static readonly int pillPoint = 1;
        public static readonly int powerPillPoint = 10;
        public static readonly int ghostPoint = 100;
        private AnimationType animation = AnimationType.None;
        public int lives = 3;
        public int respawnSpeed = 500;
        public bool isDead = false;
        private int cntT = 0;

        public void SetLeftAnimation(PictureBox pictureBox)
        {
            if (animation == AnimationType.RightRun || animation == AnimationType.None)
            {
                pictureBox.Enabled = true;
                pictureBox.Image = Resource1.madoka_running;

                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Size = new Size(50, 50);

                pictureBox.Paint -= PictureBox_Paint;
                pictureBox.Paint += PictureBox_Paint;

                animation = AnimationType.LeftRun;

                pictureBox.Invalidate();
            }
        }

        public void SetRightAnimation(PictureBox pictureBox)
        {
            if (animation == AnimationType.LeftRun || animation == AnimationType.None)
            {
                pictureBox.Enabled = true;
                pictureBox.Image = Resource1.madoka_running;

                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Size = new Size(50, 50);

                pictureBox.Paint -= PictureBox_Paint;
                pictureBox.Paint += PictureBox_Paint;

                animation = AnimationType.RightRun;

                pictureBox.Invalidate();
            }
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (pb.Image != null && direction != Direction.None)
            {
                Rectangle drawRect = new Rectangle(0, 0, pb.Width, pb.Height);

                if (direction == Direction.Left)
                {
                    e.Graphics.TranslateTransform(pb.Width, 0);
                    e.Graphics.ScaleTransform(-1, 1);
                }

                e.Graphics.Clear(Color.Transparent);
                e.Graphics.DrawImage(pb.Image, drawRect);

                e.Graphics.ResetTransform();
            }
        }

        private void DrawDeath(int cntT, Graphics g)
        {
            int basePillSize = 10;
            int pillSizeVariation = cntT % 4;
            int pillSize = basePillSize + pillSizeVariation;
            int offsetX = (50 - pillSize) / 2;
            int offsetY = (50 - pillSize) / 2;

            Color pillColor = (cntT % 2 == 0) ? Color.FromArgb(255, 182, 193) : Color.HotPink;

            g.FillEllipse(new SolidBrush(pillColor), x * 50 + offsetX, y * 50 + offsetY, pillSize, pillSize);
        }

        public void Animate(int cellSize, int cntT, PictureBox pictureBox, Graphics g)
        {
            var animationStep = cntT % speed;
            float stepSize = 1f / speed;
            float adjustmentFactor = animationStep * stepSize;

            if (isDead)
            {
                prevX = x;
                prevY = y;
            }

            float interpolatedX = prevX != x ? prevX : x;
            float interpolatedY = prevY != y ? prevY : y;

            if (prevX != x || prevY != y)
            {
                switch (direction)
                {
                    case Direction.Left:
                        interpolatedX -= adjustmentFactor;
                        break;
                    case Direction.Right:
                        interpolatedX += adjustmentFactor;
                        break;
                    case Direction.Up:
                        interpolatedY -= adjustmentFactor;
                        break;
                    case Direction.Down:
                        interpolatedY += adjustmentFactor;
                        break;
                }
            }

            interpolatedX *= cellSize;
            interpolatedY *= cellSize;

            if (isDead)
            {
                DrawDeath(cntT, g);
                pictureBox.Hide();
                return;
            }
            if (direction == Direction.Left)
            {
                SetLeftAnimation(pictureBox);
            }
            else if (direction == Direction.Right)
            {
                SetRightAnimation(pictureBox);
            }


            pictureBox.Location = new Point((int)interpolatedX, (int)interpolatedY);
        }

        public void ResetForNextLife(PictureBox pictureBox, char[,] level)
        {
            level[x, y] = ' ';
            x = 1;
            y = 1;
            prevX = 1;
            prevY = 1;
            direction = Direction.None;
            nextDirection = Direction.None;
            animation = AnimationType.None;

            level[x, y] = 'p';

            pictureBox.Show();

            isDead = false;
        }
    }
}