namespace ParticleLife
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
            button1 = new Button();
            dataGridView1 = new DataGridView();
            Receiver = new DataGridViewTextBoxColumn();
            Sender = new DataGridViewTextBoxColumn();
            Gravity = new DataGridViewTextBoxColumn();
            ForceDistance = new DataGridViewTextBoxColumn();
            Friction = new DataGridViewTextBoxColumn();
            dataGridView2 = new DataGridView();
            tableLayoutPanel1 = new TableLayoutPanel();
            Particle = new DataGridViewTextBoxColumn();
            Amount = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.BackColor = Color.Transparent;
            canvas.Location = new Point(857, 47);
            canvas.Name = "canvas";
            canvas.Size = new Size(508, 513);
            canvas.TabIndex = 0;
            canvas.TabStop = false;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 1;
            button1.Text = "Reset";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Receiver, Sender, Gravity, ForceDistance, Friction });
            dataGridView1.Location = new Point(12, 289);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.Size = new Size(814, 225);
            dataGridView1.TabIndex = 3;
            // 
            // Receiver
            // 
            Receiver.HeaderText = "Receiver";
            Receiver.MinimumWidth = 8;
            Receiver.Name = "Receiver";
            Receiver.Width = 150;
            // 
            // Sender
            // 
            Sender.HeaderText = "Sender";
            Sender.MinimumWidth = 8;
            Sender.Name = "Sender";
            Sender.Width = 150;
            // 
            // Gravity
            // 
            Gravity.HeaderText = "Gravity";
            Gravity.MinimumWidth = 8;
            Gravity.Name = "Gravity";
            Gravity.Width = 150;
            // 
            // ForceDistance
            // 
            ForceDistance.HeaderText = "ForceDistance";
            ForceDistance.MinimumWidth = 8;
            ForceDistance.Name = "ForceDistance";
            ForceDistance.Width = 150;
            // 
            // Friction
            // 
            Friction.HeaderText = "Friction";
            Friction.MinimumWidth = 8;
            Friction.Name = "Friction";
            Friction.Width = 150;
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { Particle, Amount });
            dataGridView2.Location = new Point(17, 81);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersWidth = 62;
            dataGridView2.Size = new Size(365, 162);
            dataGridView2.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Location = new Point(477, 61);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(8, 8);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // Particle
            // 
            Particle.HeaderText = "ParticleColor";
            Particle.MinimumWidth = 8;
            Particle.Name = "Particle";
            Particle.Width = 150;
            // 
            // Amount
            // 
            Amount.HeaderText = "Amount";
            Amount.MinimumWidth = 8;
            Amount.Name = "Amount";
            Amount.Width = 150;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1426, 644);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(dataGridView2);
            Controls.Add(dataGridView1);
            Controls.Add(button1);
            Controls.Add(canvas);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox canvas;
        private System.Windows.Forms.Timer timer1;
        private Button button1;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Receiver;
        private DataGridViewTextBoxColumn Sender;
        private DataGridViewTextBoxColumn Gravity;
        private DataGridViewTextBoxColumn ForceDistance;
        private DataGridViewTextBoxColumn Friction;
        private DataGridView dataGridView2;
        private DataGridViewTextBoxColumn Particle;
        private DataGridViewTextBoxColumn Amount;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
