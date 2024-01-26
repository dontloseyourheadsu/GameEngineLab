using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mapas
{
    public partial class Form1 : Form
    {
        Map map;
        Bitmap bmp;
        Graphics g;
        int cellSize = 15;
        int mapSize = 15;
        int cntT = 0;
        int pacmanSpeed = 10;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            map = new Map(mapSize);
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            g.Clear(Color.Transparent);
            if (cntT % pacmanSpeed == 0)
            {
               map.UpdatePacman();
            }
            DrawMap();
            pictureBox1.Invalidate();
            cntT++;
        }

        private void DrawMap()
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    g.FillRectangle(Brushes.Black, x * cellSize, y * cellSize, cellSize, cellSize);
                    switch (map.level[x, y])
                    {
                        case '#':
                            Pill.DrawPill(g, x, y, cellSize, cntT);
                            break;
                        case 'w':
                            Brick.DrawBrick(g, x, y, cellSize);
                            break;
                        case 'p':
                            g.FillEllipse(Brushes.Yellow, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;
                        case '"':
                            Pill.DrawPowerPill(g, x, y, cellSize, cntT);
                            break;
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                    if (map.level[map.pacmanX - 1, map.pacmanY] != 'w')
                    {
                        map.pacmanDirection = Direction.Left;
                    }
                    break;
                case Keys.Right:
                    if (map.level[map.pacmanX + 1, map.pacmanY] != 'w')
                    {
                        map.pacmanDirection = Direction.Right;
                    }
                    break;
                case Keys.Up:
                    if (map.level[map.pacmanX, map.pacmanY - 1] != 'w')
                    {
                        map.pacmanDirection = Direction.Up;
                    }
                    break;
                case Keys.Down:
                    if (map.level[map.pacmanX, map.pacmanY + 1] != 'w')
                    {
                        map.pacmanDirection = Direction.Down;
                    }
                    break;
            }

            label1.Text = keyData.ToString();

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
