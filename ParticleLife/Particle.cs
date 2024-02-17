using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLife
{
    internal class Particle
    {
        Size size;
        float x, y;
        float speedX, speedY;
        Color c;
        Random r;

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float SpeedX { get => speedX; set => speedX = value; }
        public float SpeedY { get => speedY; set => speedY = value; }
        public Size Size { get => size; set => size = value; }

        public Particle(Random random, int width, int height, Color color, Size size)
        {
            r = random;
            c = color;
            this.size = size;
            RandomPosition(width, height);
            RandomSpeed();
        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(c), x, y, size.Width, size.Height);
        }

        public void RandomPosition(int width, int height)
        {
            x = r.Next(width);
            y = r.Next(height);
        }

        public void RandomSpeed()
        {
            speedX = (float)(r.NextDouble() - 0.5);
            speedY = (float)(r.NextDouble() - 0.5);
        }
    }
}
