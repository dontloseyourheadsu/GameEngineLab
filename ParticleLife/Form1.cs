using System.Drawing;

namespace ParticleLife
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        Graphics g;
        Dictionary<Color, List<Particle>> particles;
        Random r = new Random();

        public Form1()
        {
            InitializeComponent();
            Init();
        }
        private List<Particle> CreateParticles(int amount, Color color)
        {
            var particleList = new List<Particle>();
            for (int i = 0; i < amount; i++)
            {
                particleList.Add(new Particle(r, canvas.Width, canvas.Height, color, new Size(5, 5)));
            }
            return particleList;
        }

        private void Init()
        {
            bmp = new Bitmap(canvas.Width, canvas.Height);
            g = Graphics.FromImage(bmp);
            canvas.Image = bmp;
            particles = new Dictionary<Color, List<Particle>>()
            {
                { Color.Yellow, CreateParticles(800, Color.Yellow) },
                { Color.Red, CreateParticles(800, Color.Red) },
                { Color.LightGreen, CreateParticles(800, Color.LightGreen) }
            };
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            g.Clear(Color.Black);
            //green attracts green, repels red
            //red attracts red, repels yellow
            //yellow attracts yellow, attracts green
            //red attracts green
            ParticleRule(Color.LightGreen, Color.LightGreen, Gravity.Attract(0.1f));
            ParticleRule(Color.LightGreen, Color.Red, Gravity.Repel(0.1f));
            ParticleRule(Color.Red, Color.Red, Gravity.Attract(0.1f));
            ParticleRule(Color.Red, Color.Yellow, Gravity.Repel(0.1f));
            ParticleRule(Color.Yellow, Color.Yellow, Gravity.Attract(0.1f));
            ParticleRule(Color.Yellow, Color.LightGreen, Gravity.Attract(0.1f));
            ParticleRule(Color.Red, Color.LightGreen, Gravity.Attract(0.1f));

            foreach (var pL in particles)
            {
                foreach (var p in pL.Value)
                {
                    p.Draw(g);
                }
            }
            canvas.Refresh();
        }

        private void ParticleRule(Color receiver, Color sender, float gravity, float forceDistance = 150, float friction = 0.9f)
        {
            var particles1 = particles[receiver];
            var particles2 = particles[sender];

            for (int i = 0; i < particles1.Count; i++)
            {
                var forceX = 0f;
                var forceY = 0f;
                var a = particles1[i];
                for (int j = 0; j < particles2.Count; j++)
                {
                    var b = particles2[j];
                    var dx = a.X - b.X;
                    var dy = a.Y - b.Y;
                    var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (distance > 0 && distance < forceDistance)
                    {
                        var force = gravity * 1 / distance;
                        forceX += force * dx / distance;
                        forceY += force * dy / distance;
                    }
                }
                a.SpeedX = (a.SpeedX + forceX) * friction;
                a.SpeedY = (a.SpeedY + forceY) * friction;

                if (a.X < 0 || a.X > canvas.Width - a.Size.Width)
                {
                    a.SpeedX *= -1;
                }
                if (a.Y < 0 || a.Y > canvas.Height - a.Size.Height)
                {
                    a.SpeedY *= -1;
                }

                a.X += a.SpeedX;
                a.Y += a.SpeedY;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Init();
        }
    }
}
