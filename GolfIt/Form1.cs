using System.Drawing.Text;
using System.Net;

namespace GolfIt
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        private Graphics graphics;
        private Scene scene;
        private Ball ball;
        private Goal goal;
        private bool isDragging = false;
        private Point startPoint;
        private Point endPoint;
        private float forceLimit = 6;
        private int level = 0;
        private Label finishLabel;
        private Button nextLevelButton;
        private Button MainMenuButton;
        private bool isMenuActive = true;
        private int turn = 0;


        public Form1()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            bitmap = new Bitmap(Width, Height);
            graphics = Graphics.FromImage(bitmap);
            canvas.Image = bitmap;
            scene = new Scene();
            ball = new Ball(scene.cellSize, new Vector(3 * scene.cellSize, 3 * scene.cellSize), new Vector(0, 0));
            goal = new Goal(scene.cellSize, new Vector(15 * scene.cellSize, 15 * scene.cellSize), ball);
            scene.verlets.Add(ball);
            scene.verlets.Add(goal);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);
            scene.Update(graphics, canvas);

            if (isMenuActive)
            {
                DrawMenu();
                return;
            }
            
            if (goal.isBallInGoal)
            {
                FinishLevel();
                return;
            }

            turnDisplay.Text = $"Turn: {turn}";

            foreach (Verlet v in scene.verlets)
            {
                v.Update(graphics, canvas);
            }

            if (!ball.IsMoving())
            {
                if (isDragging)
                {
                    graphics.DrawLine(new Pen(Color.Gray, 5), startPoint, endPoint);
                    graphics.DrawArc(new Pen(Color.Gray, 5), endPoint.X - 10, endPoint.Y - 10, 20, 20, 0, 360);
                }
                canShootDisplay.BackColor = Color.YellowGreen;
            }
            else
            {
                canShootDisplay.BackColor = Color.Gray;
            }

            canvas.Image = bitmap;

            Invalidate();
        }

        private void DrawMenu()
        {
            levelDisplay.Text = "Select a level";
            turnDisplay.Hide();
            int buttonWidth = 120;
            int buttonHeight = 40;
            int buttonsPerRow = 5;
            int spacing = 10;
            int startX = (canvas.Width - (buttonsPerRow * buttonWidth + (buttonsPerRow - 1) * spacing)) / 2;
            int startY = 100;

            for (int i = 0; i < 10; i++)
            {
                Button levelButton = new Button
                {
                    Text = $"Level {i + 1}",
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    Width = buttonWidth,
                    Height = buttonHeight,
                    Visible = isMenuActive,
                    Location = new Point(startX + (i % buttonsPerRow) * (buttonWidth + spacing), startY + (i / buttonsPerRow) * (buttonHeight + spacing))
                };

                levelButton.Click += (sender, e) =>
                {
                    level = int.Parse(((Button)sender).Text.Split(' ')[1]);
                    isMenuActive = false;
                    ClearMenuButtons();
                    InitLevel();
                };

                canvas.Controls.Add(levelButton);
            }
        }

        private void ClearMenuButtons()
        {
            var buttons = canvas.Controls.OfType<Button>().ToList();
            foreach (var button in buttons)
            {
                canvas.Controls.Remove(button);
            }

            isMenuActive = false;
        }


        private void FinishLevel()
        {
            turnDisplay.Hide();
            int messageWidth = (int)(canvas.Width * 0.6);
            int messageHeight = (int)(canvas.Height * 0.6);
            int startX = (canvas.Width - messageWidth) / 2;
            int startY = (canvas.Height - messageHeight) / 2;

            if (finishLabel == null)
            {
                finishLabel = new Label();
                canvas.Controls.Add(finishLabel);
            }

            finishLabel.BackColor = Color.Green;
            finishLabel.ForeColor = Color.YellowGreen;
            finishLabel.BorderStyle = BorderStyle.FixedSingle;
            finishLabel.Text = "Level Finished";
            finishLabel.Font = new Font("Arial", 24, FontStyle.Bold);
            finishLabel.TextAlign = ContentAlignment.MiddleCenter;
            finishLabel.Width = messageWidth;
            finishLabel.Height = messageHeight;
            finishLabel.Location = new Point(startX, startY);
            finishLabel.BringToFront();
            finishLabel.Visible = true;

            if (nextLevelButton == null)
            {
                nextLevelButton = new Button();
                canvas.Controls.Add(nextLevelButton);
                nextLevelButton.Click += NextLevelButton_Click;
            }

            nextLevelButton.Text = "Next Level";
            nextLevelButton.Font = new Font("Arial", 12, FontStyle.Bold);
            nextLevelButton.ForeColor = Color.White;
            nextLevelButton.Width = 120;
            nextLevelButton.Height = 40;
            nextLevelButton.Location = new Point((startX + messageWidth / 2 - 60) + 120, (startY + messageHeight - 40) - 20);
            nextLevelButton.BringToFront();
            nextLevelButton.Visible = true;

            if (MainMenuButton == null)
            {
                MainMenuButton = new Button();
                canvas.Controls.Add(MainMenuButton);
                MainMenuButton.Click += MainMenuButton_Click;
            }

            MainMenuButton.Text = "Main Menu";
            MainMenuButton.Font = new Font("Arial", 12, FontStyle.Bold);
            MainMenuButton.ForeColor = Color.White;
            MainMenuButton.Width = 120;
            MainMenuButton.Height = 40;
            MainMenuButton.Location = new Point((startX + messageWidth / 2 - 60) - 120, (startY + messageHeight - 40) - 20);
            MainMenuButton.BringToFront();
            MainMenuButton.Visible = true;
        }

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            isMenuActive = true;

            finishLabel.Visible = false;
            nextLevelButton.Visible = false;
            MainMenuButton.Visible = false;
        }

        private void NextLevelButton_Click(object sender, EventArgs e)
        {
            level++;
            InitLevel(); 

            finishLabel.Visible = false;
            nextLevelButton.Visible = false;
            MainMenuButton.Visible = false;
        }

        private void InitLevel()
        {
            graphics.Clear(Color.White);
            turnDisplay.Show();
            turn = 0;
            levelDisplay.Text = $"Level: {level}";

            scene = new Scene();
            ball = new Ball(scene.cellSize, new Vector(3 * scene.cellSize, 3 * scene.cellSize), new Vector(0, 0));
            goal = new Goal(scene.cellSize, new Vector(15 * scene.cellSize, 15 * scene.cellSize), ball);

            scene.verlets.Add(ball);
            scene.verlets.Add(goal);
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (!ball.IsMoving())
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.Location.X < ball.position.X - scene.cellSize / 2 - 2 || e.Location.X > ball.position.X + scene.cellSize / 2 + 2 || e.Location.Y < ball.position.Y - scene.cellSize / 2 - 2 || e.Location.Y > ball.position.Y + scene.cellSize / 2 + 2)
                    {
                        return;
                    }
                
                    isDragging = true;
                    startPoint = e.Location;
                    endPoint = startPoint;
                }
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && !ball.IsMoving())
            {
                endPoint = e.Location;
                canvas.Invalidate();
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && !ball.IsMoving())
            {
                isDragging = false;
                Vector forceDirection = new Vector(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                forceDirection /= 20;
                float forceMagnitude = Math.Min(forceDirection.Length(), forceLimit);
                forceDirection = forceDirection.Normalized();
                ball.PushBall(-forceDirection * forceMagnitude);
            }
        }

    }
}