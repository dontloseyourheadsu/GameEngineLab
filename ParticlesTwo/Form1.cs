namespace ParticlesTwo
{
    public partial class Form1 : Form
    {
        static Emitter emitter;
        Scene scene;
        Canvas canvas;
        static GravityInfluencer gravityInfluencer;
        static WindInfluencer windInfluencer;
        float deltaTime;
        int minVelX;
        int maxVelX;
        int minVelY;
        int maxVelY;


        public Form1()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            if (canvasLayout.Width == 0)
                return;

            deltaTime = 0;
            scene = new Scene();
            canvas = new Canvas(canvasLayout.Size);

            canvasLayout.Image = canvas.bitmap;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            canvas.Render(scene, deltaTime);

            canvasLayout.Invalidate();
            deltaTime = 0.001f;
        }

        private void EmitterClick(object sender, EventArgs e)
        {
            if (scene.Emitter.Count >= 1) return;
            var pointX = Util.Instance.Rand.Next(100, 200);
            var pointY = Util.Instance.Rand.Next(100, 200);
            var gravity = -0.5f;
            var wind = (float)(Util.Instance.Rand.NextDouble() * (0.5 - (-0.5)) + (-0.5));

            positionX.Text = pointX.ToString();
            positionY.Text = pointY.ToString();
            gravityTextField.Text = gravity.ToString();
            windTextField.Text = wind.ToString();
            alphaTextField.Text = "0";

            emitter = new Emitter(new PointF(pointX, pointY), canvasLayout.Size);
            gravityInfluencer = new GravityInfluencer(gravity, canvasLayout.Size.Height);
            emitter.AddInfluencer(gravityInfluencer);
            windInfluencer = new WindInfluencer(wind, canvasLayout.Size.Height);
            emitter.AddInfluencer(windInfluencer);
            emitter.GenerateParticles();

            velocityXMin.Text = emitter.MIN_X.ToString();
            velocityXMax.Text = emitter.MAX_X.ToString();
            velocityYMin.Text = emitter.MIN_Y.ToString();
            velocityYMax.Text = emitter.MAX_Y.ToString();

            scene.Emitter.Add(emitter);
        }

        private void Update(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(gravityTextField.Text) || string.IsNullOrEmpty(alphaTextField.Text) || string.IsNullOrEmpty(windTextField.Text)
                || string.IsNullOrEmpty(positionX.Text) || string.IsNullOrEmpty(positionY.Text)
                || string.IsNullOrEmpty(velocityXMin.Text) || string.IsNullOrEmpty(velocityXMax.Text)
                || string.IsNullOrEmpty(velocityYMin.Text) || string.IsNullOrEmpty(velocityYMax.Text))
            {
                MessageBox.Show("Please fill all the fields");
                return;
            }
            minVelX = int.Parse(velocityXMin.Text);
            maxVelX = int.Parse(velocityXMax.Text);
            minVelY = int.Parse(velocityYMin.Text);
            maxVelY = int.Parse(velocityYMax.Text);

            for (int i = 0; i < scene.Emitter.Count; i++)
            {
                for (int j = 0; j < scene.Emitter[i].Particles.Count; j++)
                {
                    emitter.ChangeVelocity(minVelX, maxVelX, minVelY, maxVelY);
                }
                emitter.Position = new PointF(float.Parse(positionX.Text), float.Parse(positionY.Text));
                gravityInfluencer.GravitationalConstant = float.Parse(gravityTextField.Text);
                windInfluencer.WindForce = float.Parse(windTextField.Text);
            }
        }

        private void SetFire(object sender, EventArgs e)
        {
            for (int i = 0; i < scene.Emitter.Count; i++)
            {
                for (int j = 0; j < scene.Emitter[i].Particles.Count; j++)
                {
                    scene.Emitter[i].Particles[j].imageType = "fire";
                }
            }
        }

        private void SetWater(object sender, EventArgs e)
        {
            for (int i = 0; i < scene.Emitter.Count; i++)
            {
                for (int j = 0; j < scene.Emitter[i].Particles.Count; j++)
                {
                    scene.Emitter[i].Particles[j].imageType = "water";
                }
            }
        }
    }
}
