using System.Media;

namespace Pacman
{
    public partial class Form1 : Form
    {
        Map map;
        Pacman pacman;
        Ghost[] ghosts = new Ghost[4];
        Bitmap bmp;
        Graphics g;
        int cntTGhostScared = 1;
        int deathCntT = 1;
        int cntT = 1;
        SoundPlayer musicPlayer;
        bool playDeadSound = false;
        int pacmanPoints = 0;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            musicPlayer = new SoundPlayer("Resources/Venari-Strigas.wav");

            musicPlayer.LoadAsync();
            musicPlayer.PlayLooping();

            label1.Font = LoadFont(12);
            label2.Font = LoadFont(12);
            label3.Font = LoadFont(12);

            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);

            pictureBox1.Image = bmp;

            pacman = new Pacman();
            pacman.SetRightAnimation(pictureBox2);
            ghosts[0] = new Ghost(1, 13, Ghost.GhostType.Red);
            ghosts[1] = new Ghost(2, 13, Ghost.GhostType.Pink);
            ghosts[2] = new Ghost(3, 13, Ghost.GhostType.Blue);
            ghosts[3] = new Ghost(4, 13, Ghost.GhostType.Orange);

            pictureBox1.Controls.Add(pictureBox2);
            pictureBox1.Controls.Add(pictureBox3);
            pictureBox1.Controls.Add(pictureBox4);
            pictureBox1.Controls.Add(pictureBox5);
            pictureBox1.Controls.Add(pictureBox6);

            map = new Map(cntT, g, pacman, ghosts);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            g.Clear(Color.Transparent);
            if (pacman.isDead)
            {
                if (!playDeadSound)
                {
                    musicPlayer.Stop();
                    musicPlayer = new SoundPlayer("Resources/Once-We-were.wav");
                    musicPlayer.LoadAsync();
                    musicPlayer.PlayLooping();
                    playDeadSound = true;
                    deathCntT = 1;
                }

                if (deathCntT % pacman.respawnSpeed == 0)
                {
                    pacman.ResetForNextLife(pictureBox2, map.level);
                    playDeadSound = false;
                    musicPlayer.Stop();
                    musicPlayer = new SoundPlayer("Resources/Venari-Strigas.wav");
                    musicPlayer.LoadAsync();
                    musicPlayer.PlayLooping();
                }
                deathCntT++;
            }
            else
            {
                if (cntT % pacman.speed == 0)
                {
                    map.UpdatePacman();
                }

                foreach (var ghost in ghosts)
                {
                    if (cntT % ghost.speed == 0)
                    {
                        map.UpdateGhost(ghost.ghostType);
                    }
                }
            }

            if (ghosts[0].isScared || ghosts[1].isScared || ghosts[2].isScared || ghosts[3].isScared)
            {
                if (map.consumedPowerPill)
                {
                    cntTGhostScared = 1;
                    map.consumedPowerPill = false;
                }

                if (cntTGhostScared % ghosts[0].scaredTime == 0)
                {
                    foreach (var ghost in ghosts)
                    {
                        ghost.isScared = false;
                    }
                }
                cntTGhostScared++;
            }


            foreach (var ghost in ghosts)
            {
                if (ghost.respawn)
                {
                    if (ghost.respawnCntT % ghost.respawnSpeed == 0)
                    {
                        ghost.ResetForNextLife(map.level);
                        ghost.respawnCntT = 1;
                    }
                    ghost.respawnCntT++;
                }
            }

            map.DrawMap(cntT, pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6);
            pictureBox1.Invalidate();

            pictureBox1.Refresh();
            pictureBox2.Refresh();
            pictureBox3.Refresh();
            pictureBox4.Refresh();
            pictureBox5.Refresh();
            pictureBox6.Refresh();
            label1.Text = "Score: " + pacman.points;
            label2.Text = "Lives: " + pacman.lives;

            if (cntT < int.MaxValue - 10)
            {
                cntT++;
            }
            else
            {
                cntT = 0;
            }

            if (pacman.lives == 0)
            {
                musicPlayer.Stop();
                musicPlayer = new SoundPlayer("Resources/Once-We-were.wav");
                musicPlayer.LoadAsync();
                musicPlayer.PlayLooping();

                timer1.Stop();
                label3.Text = "Game Over!";
            }
            else if (pacmanPoints != pacman.points)
            {
                if (map.PacmanWins())
                {
                    timer1.Stop();
                    label3.Text = "You Win!";
                }
                pacmanPoints = pacman.points;
            }
        }

        private void Form1_KeyPress(object sender, KeyEventArgs keyData)
        {
            if (pacman.x == 0 || pacman.x == map.mapSize - 1 || pacman.y == 0 || pacman.y == map.mapSize - 1)
            {
                return;
            }

            switch (keyData.KeyData)
            {
                case Keys.Left:
                    if (cntT % pacman.speed == 0)
                    {
                        pacman.direction = Direction.Left;
                    }
                    pacman.nextDirection = Direction.Left;
                    break;
                case Keys.Right:
                    if (cntT % pacman.speed == 0)
                    {
                        pacman.direction = Direction.Right;
                    }
                    pacman.nextDirection = Direction.Right;
                    break;
                case Keys.Up:
                    if (cntT % pacman.speed == 0)
                    {
                        pacman.direction = Direction.Up;
                    }
                    pacman.nextDirection = Direction.Up;
                    break;
                case Keys.Down:
                    if (cntT % pacman.speed == 0)
                    {
                        pacman.direction = Direction.Down;
                    }
                    pacman.nextDirection = Direction.Down;
                    break;
            }
        }
    }
}