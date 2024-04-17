using DinoGrr.Physics;
using DinoGrr.Rendering;
using Microsoft.VisualBasic.Devices;

namespace DinoGrr
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        Graphics graphics;
        Render render;
        bool mouseDown = false;

        Vector2 mouseG = new Vector2(0, 0);

        PhysicWorld physicWorld;

        int cntT = 0;

        public Form1()
        {
            InitializeComponent();
            StartWorld();
        }

        private void ScaleCanvas()
        {
            bitmap = new Bitmap(canvas.Width, canvas.Height);
            graphics = Graphics.FromImage(bitmap);
            canvas.Image = bitmap;
        }

        private void StartWorld()
        {
            ScaleCanvas();
            physicWorld = new PhysicWorld(canvas.Width, canvas.Height);
            Camera camera = new Camera(physicWorld.player, new Size(canvas.Width , canvas.Height));
            render = new Render(graphics, camera);
        }

        private void UpdateGame()
        {
            physicWorld.Update(cntT, mouseG, render);
        }

        private void TimerGameLoop(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);
            UpdateGame();
            canvas.Invalidate();
            cntT++;
        }

        private void canvas_MouseDown(object sender, EventArgs e)
        {
            physicWorld.player.dinoPencil.NewPolygon = new Polygon(new List<Particle>(), new List<Stick>());
            mouseDown = true;
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            physicWorld.player.dinoPencil.AddPolygon();
            mouseDown = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs mouse)
        {
            if (mouseDown && cntT % 5 == 0)
            {
                var mass = 2;
                physicWorld.player.dinoPencil.AddParticle(mouse.X, mouse.Y, mass);
            }

            mouseG = new Vector2(mouse.X, mouse.Y);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                physicWorld.player.MoveLeft();
            }
            else if (e.KeyCode == Keys.D)
            {
                physicWorld.player.MoveRight();
            }
            else if (e.KeyCode == Keys.W)
            {
                physicWorld.player.Jump();
            }
        }
    }
}
