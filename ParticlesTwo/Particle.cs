using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{
    public class Particle
    {
        //original position velocity
        public PointF OP, OV;
        //position, velocity, force
        public float pX, pY, vX, vY, tFX, tFY;

        public float Alfa { get; set; }

        public float Mass { get; set; }
        public Image Image { get; set; }
        public float Size { get; set; }
        private float Lifetime;
        private float elapsedTime;
        public  string imageType = "fire";

        public Particle(PointF position, PointF velocity)
        {
            OP = position;
            OV = velocity;
            Alfa = .17f;
            Init();
        }

        private void Init()
        {
            pX = OP.X + (float)Util.Instance.Rand.NextDouble();
            pY = OP.Y + (float)Util.Instance.Rand.NextDouble();

            vX = OV.X;
            vY = OV.Y;
            elapsedTime = 0;

            if (imageType == "fire")
                Image = Util.Instance.FIRE_IMGS[Util.Instance.Rand.Next(Util.Instance.FIRE_IMGS.Length)];
            else if (imageType == "water")
                Image = Util.Instance.WAT_IMGS[Util.Instance.Rand.Next(Util.Instance.WAT_IMGS.Length)];

            Lifetime = (float)Util.Instance.Rand.NextDouble() - .59f;
            Size = Util.Instance.Rand.Next(5, 15);
            Mass = Size;

            float VAL = (float)Util.Instance.Rand.NextDouble();

            VAL = Math.Min(VAL, Alfa);
            Image = Util.ApplyAlpha(Image, VAL);
        }

        public void Update(List<Influencer> influencers, float deltaTime, Size space, PointF pos)
        {
            tFX = 0;
            tFY = 0;
            OP = pos;

            elapsedTime += deltaTime;

            for (int i = 0; i < influencers.Count; i++)
            {
                PointF force = influencers[i].GetForce(this);
                tFX += force.X;
                tFY += force.Y;
            }

            vX += tFX;
            vY += tFY;

            pX += vX;
            pY += vY;

            // Actualizar de acuerdo con el tiempo de vida y el espacio del canvas
            if (pX < 0 || pX > space.Width || pY < 0 || pY > space.Height || elapsedTime > Lifetime)
            {
                Init();
            }
        }

        public void Render(Graphics g)
        {
            g.DrawImage(Image, pX, pY, Size, Size);
        }
    }
}