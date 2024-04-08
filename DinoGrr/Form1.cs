using DinoGrr.Physics;
using DinoGrr.Rendering;

namespace DinoGrr
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        Graphics graphics;
        Render render;
        bool mouseDown = false;

        Player player;
        List<Dinosaur> dinosaurs;
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
            render = new Render(graphics);
            CreatePlayer();
            DefineWordObjects();

        }

        private void CreatePlayer()
        {
            player = new Player();
        }

        private void DefineWordObjects()
        {
            dinosaurs = new List<Dinosaur>();
            dinosaurs.Add(new Dinosaur(100, 100, 50, 50));
        }

        private void UpdateGame()
        {
            foreach (var dinosaur in dinosaurs)
            {
                dinosaur.Update(canvas.Width, canvas.Height, cntT);
            }

            player.Update(canvas.Width, canvas.Height);

            foreach (var polygon in player.Polygons)
            {
                render.DrawPolygon(polygon);
            }
            
            if (player.NewPolygon != null)
            {
                render.DrawPolygon(player.NewPolygon);
            }

            foreach (var dinosaur in dinosaurs)
            {
                render.DrawDinosaur(dinosaur);
            }
        }

        private void TimerGameLoop(object sender, EventArgs e)
        {
            graphics.Clear(Color.Black);
            UpdateGame();
            canvas.Invalidate();
            cntT++;
        }

        private void canvas_MouseDown(object sender, EventArgs e)
        {
            player.NewPolygon = new Polygon(new List<Particle>(), new List<Stick>());
            mouseDown = true;
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            player.AddPolygon();
            mouseDown = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs mouse)
        {
            if (mouseDown && cntT % 5 == 0)
            {
                var mass = 2;
                player.AddParticle(mouse.X, mouse.Y, mass);
            }
        }
    }
}
