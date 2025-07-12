namespace NAMI
{
    partial class SimulationForm
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
            pictureBox1 = new PictureBox();
            dataGridView1 = new DataGridView();
            labelDecision = new Label();
            roundedButton1 = new NAMI.Helpers.RoundedButton();
            roundedButton2 = new NAMI.Helpers.RoundedButton();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            timer1 = new System.Windows.Forms.Timer(components);
            roundedButton3 = new NAMI.Helpers.RoundedButton();
            roundedButton4 = new NAMI.Helpers.RoundedButton();
            pictureBoxSign = new PictureBox();
            labelSignDecision = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSign).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ButtonHighlight;
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(800, 700);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += pictureBox1_Paint;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(824, 12);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(390, 425);
            dataGridView1.TabIndex = 1;
            // 
            // labelDecision
            // 
            labelDecision.AutoSize = true;
            labelDecision.BackColor = SystemColors.ButtonHighlight;
            labelDecision.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            labelDecision.ForeColor = SystemColors.Highlight;
            labelDecision.Location = new Point(818, 497);
            labelDecision.Name = "labelDecision";
            labelDecision.Size = new Size(664, 47);
            labelDecision.TabIndex = 2;
            labelDecision.Text = "\"Рекомендация: Движение разрешено\"";
            // 
            // roundedButton1
            // 
            roundedButton1.BackColor = SystemColors.ControlLightLight;
            roundedButton1.CornerRadius = 20;
            roundedButton1.FlatStyle = FlatStyle.Flat;
            roundedButton1.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton1.ForeColor = SystemColors.Highlight;
            roundedButton1.Location = new Point(1230, 791);
            roundedButton1.Name = "roundedButton1";
            roundedButton1.Size = new Size(400, 100);
            roundedButton1.TabIndex = 3;
            roundedButton1.Text = "Выход";
            roundedButton1.UseVisualStyleBackColor = false;
            roundedButton1.Click += roundedButton1_Click;
            // 
            // roundedButton2
            // 
            roundedButton2.BackColor = SystemColors.ControlLightLight;
            roundedButton2.CornerRadius = 20;
            roundedButton2.FlatStyle = FlatStyle.Flat;
            roundedButton2.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton2.ForeColor = SystemColors.Highlight;
            roundedButton2.Location = new Point(12, 791);
            roundedButton2.Name = "roundedButton2";
            roundedButton2.Size = new Size(400, 100);
            roundedButton2.TabIndex = 4;
            roundedButton2.Text = "Запуск";
            roundedButton2.UseVisualStyleBackColor = false;
            roundedButton2.Click += roundedButton2_Click;
            // 
            // timer1
            // 
            timer1.Interval = 50;
            // 
            // roundedButton3
            // 
            roundedButton3.BackColor = SystemColors.ControlLightLight;
            roundedButton3.CornerRadius = 20;
            roundedButton3.FlatStyle = FlatStyle.Flat;
            roundedButton3.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton3.ForeColor = SystemColors.Highlight;
            roundedButton3.Location = new Point(418, 791);
            roundedButton3.Name = "roundedButton3";
            roundedButton3.Size = new Size(400, 100);
            roundedButton3.TabIndex = 5;
            roundedButton3.Text = "Пауза";
            roundedButton3.UseVisualStyleBackColor = false;
            roundedButton3.Click += roundedButton3_Click;
            // 
            // roundedButton4
            // 
            roundedButton4.BackColor = SystemColors.ControlLightLight;
            roundedButton4.CornerRadius = 20;
            roundedButton4.FlatStyle = FlatStyle.Flat;
            roundedButton4.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton4.ForeColor = SystemColors.Highlight;
            roundedButton4.Location = new Point(824, 791);
            roundedButton4.Name = "roundedButton4";
            roundedButton4.Size = new Size(400, 100);
            roundedButton4.TabIndex = 6;
            roundedButton4.Text = "Распознование знаков";
            roundedButton4.UseVisualStyleBackColor = false;
            roundedButton4.Click += roundedButton4_Click;
            // 
            // pictureBoxSign
            // 
            pictureBoxSign.Location = new Point(1220, 12);
            pictureBoxSign.Name = "pictureBoxSign";
            pictureBoxSign.Size = new Size(400, 425);
            pictureBoxSign.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSign.TabIndex = 7;
            pictureBoxSign.TabStop = false;
            // 
            // labelSignDecision
            // 
            labelSignDecision.AutoSize = true;
            labelSignDecision.BackColor = SystemColors.ButtonHighlight;
            labelSignDecision.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            labelSignDecision.ForeColor = SystemColors.Highlight;
            labelSignDecision.Location = new Point(818, 571);
            labelSignDecision.Name = "labelSignDecision";
            labelSignDecision.Size = new Size(105, 47);
            labelSignDecision.TabIndex = 8;
            labelSignDecision.Text = "Знак:";
            // 
            // SimulationForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1632, 903);
            Controls.Add(labelSignDecision);
            Controls.Add(pictureBoxSign);
            Controls.Add(roundedButton4);
            Controls.Add(roundedButton3);
            Controls.Add(roundedButton2);
            Controls.Add(roundedButton1);
            Controls.Add(labelDecision);
            Controls.Add(dataGridView1);
            Controls.Add(pictureBox1);
            Name = "SimulationForm";
            Text = "Симуляция";
            Load += SimulationForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSign).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private DataGridView dataGridView1;
        private Label labelDecision;
        private Helpers.RoundedButton roundedButton1;
        private Helpers.RoundedButton roundedButton2;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Timer timer1;
        private Helpers.RoundedButton roundedButton3;
        private Helpers.RoundedButton roundedButton4;
        private PictureBox pictureBoxSign;
        private Label labelSignDecision;
    }
}
