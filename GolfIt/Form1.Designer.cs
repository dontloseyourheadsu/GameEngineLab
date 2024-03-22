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
            canvas.Location = new Point(8, 34);
            canvas.Margin = new Padding(2, 2, 2, 2);
            canvas.Name = "canvas";
            canvas.Size = new Size(560, 240);
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
            canShootDisplay.Location = new Point(8, 7);
            canShootDisplay.Margin = new Padding(2, 2, 2, 2);
            canShootDisplay.Name = "canShootDisplay";
            canShootDisplay.Size = new Size(20, 17);
            canShootDisplay.TabIndex = 1;
            canShootDisplay.TabStop = false;
            // 
            // turnDisplay
            // 
            turnDisplay.AutoSize = true;
            turnDisplay.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            turnDisplay.ForeColor = Color.White;
            turnDisplay.Location = new Point(496, 9);
            turnDisplay.Margin = new Padding(2, 0, 2, 0);
            turnDisplay.Name = "turnDisplay";
            turnDisplay.Size = new Size(50, 18);
            turnDisplay.TabIndex = 2;
            turnDisplay.Text = "label1";
            // 
            // levelDisplay
            // 
            levelDisplay.AutoSize = true;
            levelDisplay.Font = new Font("Impact", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            levelDisplay.ForeColor = Color.White;
            levelDisplay.Location = new Point(239, 9);
            levelDisplay.Margin = new Padding(2, 0, 2, 0);
            levelDisplay.Name = "levelDisplay";
            levelDisplay.Size = new Size(57, 23);
            levelDisplay.TabIndex = 3;
            levelDisplay.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(4.9F, 9F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(580, 344);
            Controls.Add(levelDisplay);
            Controls.Add(turnDisplay);
            Controls.Add(canShootDisplay);
            Controls.Add(canvas);
            Margin = new Padding(2, 2, 2, 2);
            MaximumSize = new Size(596, 383);
            MinimumSize = new Size(596, 383);
            Name = "Form1";
            Text = "Golf it";
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
