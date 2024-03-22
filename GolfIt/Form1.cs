using System.Drawing.Text;
using System.Media;
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
        private float forceLimit = 8;
        private int level = 0;
        private Label finishLabel;
        private Button nextLevelButton;
        private Button MainMenuButton;
        private bool isMenuActive = true;
        private int turn = 0;
        SoundPlayer musicPlayer;
        private int cntT = 0;

        public Form1()
        {
            InitializeComponent();

            musicPlayer = new SoundPlayer("Resources/golfit.wav");

            musicPlayer.LoadAsync();
            musicPlayer.PlayLooping();

            Init();
        }

        private void Init()
        {
            bitmap = new Bitmap(Width, Height);
            graphics = Graphics.FromImage(bitmap);
            canvas.Image = bitmap;
            scene = new Scene(level);

            (int ballX, int ballY) = scene.map.GetBallPosition();
            (int goalX, int goalY) = scene.map.GetGoalPosition();

            ball = new Ball(scene.cellSize, new Vector(ballX, ballY), new Vector(0, 0));
            goal = new Goal(scene.cellSize, new Vector(goalX, goalY), ball);

            scene.verlets.Add(ball);
            scene.verlets.Add(goal);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            graphics.Clear(Color.White);
            scene.Update(graphics, canvas, cntT);

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

            foreach (Obstacle o in scene.obstacles)
            {
                o.Update(graphics, canvas, scene.map, cntT);
            }

            foreach (Verlet v in scene.verlets)
            {
                v.Update(graphics, canvas, scene.map);
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

            if (cntT < int.MaxValue)
            {
                cntT++;
            }
            else
            {
                cntT = 0;
            }

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
                var index = i;
                Button levelButton = new Button
                {
                    Text = $"Hole {index + 1}",
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
                    level = index;
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

            if (finishLabel is null)
            {
                finishLabel = new Label();
                canvas.Controls.Add(finishLabel);

                finishLabel.BackColor = Color.Green;
                finishLabel.ForeColor = Color.YellowGreen;
                finishLabel.BorderStyle = BorderStyle.FixedSingle;

                string message;

                if (turn == 1)
                {
                    message = "Hole-in-one!";
                }
                else if (turn == 2)
                {
                    message = "Eagle!";
                }
                else if (turn == 4)
                {
                    message = "Par!";
                }
                else if (turn == 5)
                {
                    message = "Bogey!";
                }
                else if (turn == 6)
                {
                    message = "Double Bogey!";
                }
                else if (turn == 7)
                {
                    message = "Triple Bogey!";
                }
                else
                {
                    message = "Level Finished!";
                }

                finishLabel.Text = message;


                finishLabel.Font = new Font("Arial", 24, FontStyle.Bold);
                finishLabel.TextAlign = ContentAlignment.MiddleCenter;
                finishLabel.Width = messageWidth;
                finishLabel.Height = messageHeight;
                finishLabel.Location = new Point(startX, startY);
                finishLabel.BringToFront();
                finishLabel.Visible = true;
            }

            if (nextLevelButton is null && level + 1 < scene.map.levels.Count && scene.map.levels[level + 1] is not null)
            {
                nextLevelButton = new Button();
                canvas.Controls.Add(nextLevelButton);
                nextLevelButton.Click += NextLevelButton_Click;

                nextLevelButton.Text = "Next Level";
                nextLevelButton.Font = new Font("Arial", 12, FontStyle.Bold);
                nextLevelButton.ForeColor = Color.White;
                nextLevelButton.Width = 120;
                nextLevelButton.Height = 40;
                nextLevelButton.Location = new Point((startX + messageWidth / 2 - 60) + 120, (startY + messageHeight - 40) - 20);
                nextLevelButton.BringToFront();
                nextLevelButton.Visible = true;
            }

            if (MainMenuButton is null)
            {
                MainMenuButton = new Button();
                canvas.Controls.Add(MainMenuButton);
                MainMenuButton.Click += MainMenuButton_Click;

                MainMenuButton.Text = "Main Menu";
                MainMenuButton.Font = new Font("Arial", 12, FontStyle.Bold);
                MainMenuButton.ForeColor = Color.White;
                MainMenuButton.Width = 120;
                MainMenuButton.Height = 40;
                MainMenuButton.Location = new Point((startX + messageWidth / 2 - 60) - 120, (startY + messageHeight - 40) - 20);
                MainMenuButton.BringToFront();
                MainMenuButton.Visible = true;
            }
        }

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            isMenuActive = true;

            finishLabel.Visible = false;
            if (nextLevelButton is not null)
            {
                nextLevelButton.Visible = false;
            }
            MainMenuButton.Visible = false;

            canvas.Controls.Clear();
            if (nextLevelButton is not null)
            {
                nextLevelButton.Dispose();
            }
            MainMenuButton.Dispose();
            finishLabel.Dispose();

            nextLevelButton = null;
            MainMenuButton = null;
            finishLabel = null;
        }

        private void NextLevelButton_Click(object sender, EventArgs e)
        {
            finishLabel.Visible = false;
            if (nextLevelButton is not null)
            {
                nextLevelButton.Visible = false;
            }
            MainMenuButton.Visible = false;

            canvas.Controls.Clear();
            if (nextLevelButton is not null)
            {
                nextLevelButton.Dispose();
            }
            MainMenuButton.Dispose();
            finishLabel.Dispose();

            nextLevelButton = null;
            MainMenuButton = null;
            finishLabel = null;

            level++;
            InitLevel();
        }

        private void InitLevel()
        {
            graphics.Clear(Color.White);
            turnDisplay.Show();
            turn = 0;
            levelDisplay.Text = $"Hole: {level + 1}";

            scene = new Scene(level);

            (int ballX, int ballY) = scene.map.GetBallPosition();
            (int goalX, int goalY) = scene.map.GetGoalPosition();

            ball = new Ball(scene.cellSize, new Vector(ballX, ballY), new Vector(0, 0));
            goal = new Goal(scene.cellSize, new Vector(goalX, goalY), ball);

            var obstacles = new List<Obstacle>();

            for (int i = 0; i < scene.map.GetObstaclePositions().Count; i++)
            {
                (int obsX, int obsY) = scene.map.GetObstaclePositions()[i];
                switch (scene.map.obstacles[level][i])
                {
                    case Map.triangle:
                        obstacles.Add(new Triangle(scene.cellSize, new Vector(obsX, obsY)));
                        break;
                }
            }

            obstacles = [.. obstacles, .. scene.map.movingFloors[level]];

            scene.verlets.Add(ball);
            scene.verlets.Add(goal);
            scene.obstacles = obstacles;
            ball.obstacles = obstacles;

            scene.map.SetMap(level);
            turnDisplay.Text = $"Par: {turn}";
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
                /*Vector currentPoint = new Vector(e.Location.X, e.Location.Y);
                Vector startToEnd = currentPoint - new Vector(startPoint.X, startPoint.Y);

                if (startToEnd.Length() > forceLimit)
                {
                    Vector limitedPoint = new Vector(startPoint.X, startPoint.Y) + startToEnd.Normalized() * forceLimit * scene.cellSize;
                    endPoint = new Point((int)limitedPoint.X, (int)limitedPoint.Y);
                }
                else
                {
                }*/
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
                turn++;
                turnDisplay.Text = $"Par: {turn}";
            }
        }

    }
}