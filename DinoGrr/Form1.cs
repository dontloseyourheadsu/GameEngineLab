using DinoGrr.Physics;
using DinoGrr.Rendering;
using DinoGrr.WorldGen;

namespace DinoGrr
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        Graphics graphics;
        Render render;
        bool mouseDown = false;

        Camera camera;

        Vector2 mouseG = new Vector2(0, 0);
        WorldGenerator worldGenerator;

        PhysicWorld physicWorld;

        int cntT = 0;

        public Form1()
        {
            InitializeComponent();
            StartWorld();
        }

        private void ScaleCanvas()
        {
            canvas.Width = Width;
            canvas.Height = Height;
            bitmap = new Bitmap(canvas.Width, canvas.Height);
            graphics = Graphics.FromImage(bitmap);
            canvas.Image = bitmap;
        }

        private void StartWorld()
        {
            ScaleCanvas();
            worldGenerator = new WorldGenerator();
            
            physicWorld = worldGenerator.CurrentWorld;
            camera = new Camera(physicWorld.player, new Size(canvas.Width, canvas.Height));
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
            if (physicWorld.gameEnd)
            {
                worldGenerator.NextLevel();
                physicWorld = worldGenerator.CurrentWorld;
                camera = new Camera(physicWorld.player, new Size(canvas.Width, canvas.Height));
                render = new Render(graphics, camera);
            }
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
            if (mouseDown)
            {
                var mass = 2;
                var realPlacement = camera.TranslateToOrigin(new Point(mouse.X, mouse.Y));
                physicWorld.player.dinoPencil.AddParticle(realPlacement.X, realPlacement.Y, mass);
            }

            mouseG = new Vector2(mouse.X, mouse.Y);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                physicWorld.player.dinoPencil.RemovePolygon();
            }

            if (physicWorld.player.isDamaged)
            {
                return;
            }

            if (e.KeyCode == Keys.A)
            {
                physicWorld.player.MoveLeft();
                physicWorld.background.BackgroundMoveRight();
            }
            else if (e.KeyCode == Keys.D)
            {
                physicWorld.player.MoveRight();
                physicWorld.background.BackgroundMoveLeft();
            }
            else if (e.KeyCode == Keys.W)
            {
                physicWorld.player.Jump();
            }
        }
    }
}
