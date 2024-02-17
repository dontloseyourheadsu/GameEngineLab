using System.Drawing;

namespace ParticleLife
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        Graphics g;
        Dictionary<Color, List<Particle>> particles;
        Random r = new Random();
        List<ParticleConfiguration> configurations;

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

        private void InitializeCanvas()
        {
            canvas.Width = Width - 100 - canvas.Location.X;
            canvas.Height = Height - 100 - canvas.Location.Y;
            bmp = new Bitmap(canvas.Width, canvas.Height);
            g = Graphics.FromImage(bmp);
            canvas.Image = bmp;
        }

        private void Init()
        {
            InitializeCanvas();

            particles = new Dictionary<Color, List<Particle>>()
            {
                { Color.Yellow, CreateParticles(200, Color.Yellow) },
                { Color.Red, CreateParticles(200, Color.Red) },
                { Color.LightGreen, CreateParticles(200, Color.LightGreen) }
            };

            configurations =
            [
                new ParticleConfiguration(Color.LightGreen, Color.LightGreen, Force.Attract(0.1f), forceDistance: 100, friction: 0.99f),
                new ParticleConfiguration(Color.LightGreen, Color.Red, Force.Repel(0.1f), forceDistance: 100, friction: 0.99f),
                new ParticleConfiguration(Color.LightGreen, Color.Yellow, Force.Repel(0.1f), forceDistance: 100, friction: 0.99f),
                new ParticleConfiguration(Color.Red, Color.Red, Force.Attract(0.1f), forceDistance: 100, friction: 0.99f),
                new ParticleConfiguration(Color.Red, Color.Yellow, Force.Repel(0.1f), forceDistance: 100, friction: 0.99f),
                new ParticleConfiguration(Color.Yellow, Color.Yellow, Force.Attract(0.1f), forceDistance: 100, friction: 0.99f)
            ];

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            InitializeCanvas();

            g.Clear(Color.Black);
            foreach (var config in configurations)
            {
                ParticleRule(config.receiver, config.sender, config.gravity, config.forceDistance, config.friction);
            }

            foreach (var pL in particles)
            {
                foreach (var p in pL.Value)
                {
                    p.Draw(g);
                }
            }
            canvas.Refresh();
        }

        private void ParticleRule(Color receiver, Color sender, float gravity, float forceDistance, float friction)
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
