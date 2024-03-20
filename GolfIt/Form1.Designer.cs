namespace GolfIt
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            canvas = new PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            canShootDisplay = new PictureBox();
            turnDisplay = new Label();
            levelDisplay = new Label();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)canShootDisplay).BeginInit();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.Location = new Point(12, 57);
            canvas.Name = "canvas";
            canvas.Size = new Size(800, 400);
            canvas.TabIndex = 0;
            canvas.TabStop = false;
            canvas.MouseDown += canvas_MouseDown;
            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseUp += canvas_MouseUp;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
            // 
            // canShootDisplay
            // 
            canShootDisplay.BackColor = Color.YellowGreen;
            canShootDisplay.Location = new Point(12, 12);
            canShootDisplay.Name = "canShootDisplay";
            canShootDisplay.Size = new Size(29, 28);
            canShootDisplay.TabIndex = 1;
            canShootDisplay.TabStop = false;
            // 
            // turnDisplay
            // 
            turnDisplay.AutoSize = true;
            turnDisplay.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            turnDisplay.ForeColor = Color.White;
            turnDisplay.Location = new Point(709, 15);
            turnDisplay.Name = "turnDisplay";
            turnDisplay.Size = new Size(77, 27);
            turnDisplay.TabIndex = 2;
            turnDisplay.Text = "label1";
            // 
            // levelDisplay
            // 
            levelDisplay.AutoSize = true;
            levelDisplay.Font = new Font("Impact", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            levelDisplay.ForeColor = Color.White;
            levelDisplay.Location = new Point(341, 15);
            levelDisplay.Name = "levelDisplay";
            levelDisplay.Size = new Size(83, 35);
            levelDisplay.TabIndex = 3;
            levelDisplay.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(822, 556);
            Controls.Add(levelDisplay);
            Controls.Add(turnDisplay);
            Controls.Add(canShootDisplay);
            Controls.Add(canvas);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            ((System.ComponentModel.ISupportInitialize)canShootDisplay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvas;
        private System.Windows.Forms.Timer timer1;
        private PictureBox canShootDisplay;
        private Label turnDisplay;
        private Label levelDisplay;
    }
}
