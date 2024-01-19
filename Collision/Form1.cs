using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Collision
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            pictureBoxes = new List<PictureBox>();
            Random rand = new Random();
            int amount = rand.Next(3, 5);
            for (int i = 0; i < amount; i++)
            {
                PictureBox pb = new PictureBox();
                pb.BackColor = Color.Red;
                pb.Size = new Size(rand.Next(20, 80), rand.Next(20, 80));
                pb.Location = new Point(rand.Next(0, pictureBox2.Width - pb.Width), rand.Next(0, pictureBox2.Height - pb.Height));
                pictureBoxes.Add(pb);
            }

            ball = new Ball(rand, pictureBox2, pictureBoxes);
            bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            g = Graphics.FromImage(bmp);
            pictureBox2.Image = bmp;

            foreach (PictureBox pb in pictureBoxes)
            {
                if (ball.IsCollidingWithBox(pb))
                {
                    Init();
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Init();
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            g.Clear(Color.Transparent);
            ball.Update();
            ball.Render(g);

            label1.Text = $"DIR: {ball.pos.X}, {ball.pos.Y}";
            label2.Text = $"SPEED: {ball.impulseX}, {ball.impulseY}";
            label3.Text = $"SIZE: {ball.diameter}";
            label4.Text = $"RANDOM OBSTACLES GENERATED: {pictureBoxes.Count}";

            pictureBox2.Invalidate();
        }
    }
}
