using System.Drawing;

namespace Parallax
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        int motion1 = 1;
        int motion2 = 3;
        int motion3 = 10;

        int width = 688;
        int l1_X1, l1_X2, l2_X1, l2_X2, l3_X1, l3_X2;

        Bitmap layer0, layer1, layer2, layer3;
        bool right, hold = true;

        static Graphics g;

        private void BackgroundMove()
        {
            if (l1_X1 < -width) { l1_X1 = width - motion1; }
            l1_X1 -= motion1; l1_X2 -= motion1;
            if (l1_X2 < -width) { l1_X2 = width - motion1; }

            if (l2_X1 < -width) { l2_X1 = width - motion2; }
            l2_X1 -= motion2; l2_X2 -= motion2;
            if (l2_X2 < -width) { l2_X2 = width - motion2; }

            if (l3_X1 < -width) { l3_X1 = width - motion3; }
            l3_X1 -= motion3; l3_X2 -= motion3;
            if (l3_X2 < -width) { l3_X2 = width - motion3; }

            Invalidate();
        }

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.player = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.player)).BeginInit();
            this.SuspendLayout();
            // 
            // player
            // 
            this.player.BackColor = System.Drawing.Color.Transparent;
            this.player.Location = new System.Drawing.Point(285, 220);
            this.player.Name = "player";
            this.player.Size = new System.Drawing.Size(50, 80);
            this.player.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.player.TabIndex = 0;
            this.player.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 350);
            this.Controls.Add(this.player);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.player)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox player;
        private System.Windows.Forms.Timer timer1;
    }
}

