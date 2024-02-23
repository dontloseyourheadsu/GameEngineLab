namespace ParticlesTwo
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
            canvasLayout = new PictureBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            velocityXMin = new TextBox();
            velocityXMax = new TextBox();
            positionX = new TextBox();
            positionY = new TextBox();
            velocityYMin = new TextBox();
            velocityYMax = new TextBox();
            gravityTextField = new TextBox();
            alphaTextField = new TextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            label6 = new Label();
            windTextField = new TextBox();
            ((System.ComponentModel.ISupportInitialize)canvasLayout).BeginInit();
            SuspendLayout();
            // 
            // canvasLayout
            // 
            canvasLayout.BackColor = Color.Transparent;
            canvasLayout.Location = new Point(268, 12);
            canvasLayout.Name = "canvasLayout";
            canvasLayout.Size = new Size(652, 402);
            canvasLayout.TabIndex = 0;
            canvasLayout.TabStop = false;
            // 
            // button1
            // 
            button1.Location = new Point(12, 380);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 1;
            button1.Text = "Emitter";
            button1.UseVisualStyleBackColor = true;
            button1.Click += EmitterClick;
            // 
            // button2
            // 
            button2.Location = new Point(12, 453);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 2;
            button2.Text = "Fire";
            button2.UseVisualStyleBackColor = true;
            button2.Click += SetFire;
            // 
            // button3
            // 
            button3.Location = new Point(140, 453);
            button3.Name = "button3";
            button3.Size = new Size(112, 34);
            button3.TabIndex = 3;
            button3.Text = "Wat";
            button3.UseVisualStyleBackColor = true;
            button3.Click += SetWater;
            // 
            // button4
            // 
            button4.Location = new Point(140, 380);
            button4.Name = "button4";
            button4.Size = new Size(112, 34);
            button4.TabIndex = 4;
            button4.Text = "Update Emitter";
            button4.UseVisualStyleBackColor = true;
            button4.Click += Update;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.White;
            label1.Location = new Point(258, 471);
            label1.Name = "label1";
            label1.Size = new Size(51, 25);
            label1.TabIndex = 5;
            label1.Text = "Vel X";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.White;
            label2.Location = new Point(252, 427);
            label2.Name = "label2";
            label2.Size = new Size(50, 25);
            label2.TabIndex = 6;
            label2.Text = "Vel Y";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.White;
            label3.Location = new Point(483, 427);
            label3.Name = "label3";
            label3.Size = new Size(40, 25);
            label3.TabIndex = 7;
            label3.Text = "Pos";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.White;
            label4.Location = new Point(687, 431);
            label4.Name = "label4";
            label4.Size = new Size(67, 25);
            label4.TabIndex = 8;
            label4.Text = "Gravity";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.White;
            label5.Location = new Point(696, 472);
            label5.Name = "label5";
            label5.Size = new Size(58, 25);
            label5.TabIndex = 9;
            label5.Text = "Alpha";
            // 
            // velocityXMin
            // 
            velocityXMin.Location = new Point(315, 472);
            velocityXMin.Name = "velocityXMin";
            velocityXMin.Size = new Size(43, 31);
            velocityXMin.TabIndex = 10;
            // 
            // velocityXMax
            // 
            velocityXMax.Location = new Point(375, 472);
            velocityXMax.Name = "velocityXMax";
            velocityXMax.Size = new Size(43, 31);
            velocityXMax.TabIndex = 11;
            // 
            // positionX
            // 
            positionX.Location = new Point(546, 431);
            positionX.Name = "positionX";
            positionX.Size = new Size(43, 31);
            positionX.TabIndex = 12;
            // 
            // positionY
            // 
            positionY.Location = new Point(612, 431);
            positionY.Name = "positionY";
            positionY.Size = new Size(43, 31);
            positionY.TabIndex = 13;
            // 
            // velocityYMin
            // 
            velocityYMin.Location = new Point(315, 428);
            velocityYMin.Name = "velocityYMin";
            velocityYMin.Size = new Size(43, 31);
            velocityYMin.TabIndex = 14;
            // 
            // velocityYMax
            // 
            velocityYMax.Location = new Point(381, 428);
            velocityYMax.Name = "velocityYMax";
            velocityYMax.Size = new Size(43, 31);
            velocityYMax.TabIndex = 15;
            // 
            // gravityTextField
            // 
            gravityTextField.Location = new Point(775, 431);
            gravityTextField.Name = "gravityTextField";
            gravityTextField.Size = new Size(43, 31);
            gravityTextField.TabIndex = 16;
            // 
            // alphaTextField
            // 
            alphaTextField.Location = new Point(775, 472);
            alphaTextField.Name = "alphaTextField";
            alphaTextField.Size = new Size(43, 31);
            alphaTextField.TabIndex = 17;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = Color.White;
            label6.Location = new Point(483, 475);
            label6.Name = "label6";
            label6.Size = new Size(54, 25);
            label6.TabIndex = 18;
            label6.Text = "Wind";
            // 
            // windTextField
            // 
            windTextField.Location = new Point(546, 475);
            windTextField.Name = "windTextField";
            windTextField.Size = new Size(43, 31);
            windTextField.TabIndex = 19;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(914, 515);
            Controls.Add(windTextField);
            Controls.Add(label6);
            Controls.Add(alphaTextField);
            Controls.Add(gravityTextField);
            Controls.Add(velocityYMax);
            Controls.Add(velocityYMin);
            Controls.Add(positionY);
            Controls.Add(positionX);
            Controls.Add(velocityXMax);
            Controls.Add(velocityXMin);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(canvasLayout);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)canvasLayout).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvasLayout;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox velocityXMin;
        private TextBox velocityXMax;
        private TextBox positionX;
        private TextBox positionY;
        private TextBox velocityYMin;
        private TextBox velocityYMax;
        private TextBox gravityTextField;
        private TextBox alphaTextField;
        private System.Windows.Forms.Timer timer1;
        private Label label6;
        private TextBox windTextField;
    }
}
