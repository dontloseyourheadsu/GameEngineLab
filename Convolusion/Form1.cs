namespace Particles
{
    public partial class Form1 : Form
    {
        Graphics g;
        Bitmap bmp;
        int particleCount = 20;
        List<Particle> particles;
        Random rand = new Random();
        float deltaTime;
        int screenFactor = 100;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            if (canvas.Width == 0) return;

            particles = new List<Particle>();
            bmp = new Bitmap(canvas.Width, canvas.Height);
            g = Graphics.FromImage(bmp);
            deltaTime = 0;
            canvas.Image = bmp;
            for (int i = 0; i < particleCount; i++)
            {
                particles.Add(new Particle(rand, canvas.Size, i, screenFactor));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (canvas.Width != this.Width || canvas.Height != this.Height)
            {
                canvas.Width = this.Width - screenFactor;
                canvas.Height = this.Height - screenFactor;
                bmp = new Bitmap(canvas.Width, canvas.Height);
                g = Graphics.FromImage(bmp);
                canvas.Image = bmp;
            }

            g.Clear(Color.Black);
            Particle p;
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].screenFactor = screenFactor;
                particles[i].space = canvas.Size;
                particles[i].Update(particles, deltaTime); 
                p = particles[i]; 
                g.FillEllipse(new SolidBrush(particles[i].c), p.X - p.diameter / 2, p.Y - p.diameter / 2, p.diameter, p.diameter);
                particles[i].changed = false;
            }
            canvas.Invalidate(); 
            deltaTime += .1f;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text)) particleCount = 10;

            if (int.TryParse(textBox1.Text, out var textNumber))
            {
                particleCount = textNumber;
            }
            else
            {
                MessageBox.Show("Invalid input");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Init();
        }
    }
}
