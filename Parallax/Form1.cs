using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parallax
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            layer1 = MyResources.B1;
            layer2 = MyResources.B2;
            layer3 = MyResources.B3;

            l1_X1 = l2_X1 = l3_X1 = 0;
            l1_X2 = l2_X2 = l3_X2 = width;

            player.Image = MyResources.HomuraHold;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (right) BackgroundMove();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            player.Image = MyResources.HomuraHold;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Right & hold)
            {
                right = true;
                hold = false;
                player.Image = MyResources.HomuraRunning;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Right & !hold)
            {
                right = false;
                hold = true;
                player.Image = MyResources.HomuraHold;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;

            g.DrawImage(layer1, l1_X1, 0, layer1.Width + 225, this.Height - 50);
            g.DrawImage(layer1, l1_X2, 0, layer1.Width + 225, this.Height - 50);

            g.DrawImage(layer2, l2_X1, 25, layer1.Width - 150, this.Height - 200);
            g.DrawImage(layer2, l2_X2, 25, layer1.Width - 150, this.Height - 200);

            g.DrawImage(layer3, l3_X1, 180, layer1.Width + 225, this.Height - 225);
            g.DrawImage(layer3, l3_X2, 180, layer1.Width + 225, this.Height - 225);
        }
    }
}
