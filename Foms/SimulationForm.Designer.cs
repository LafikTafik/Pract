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
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
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
            dataGridView1.Location = new Point(818, 12);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(400, 700);
            dataGridView1.TabIndex = 1;
            // 
            // labelDecision
            // 
            labelDecision.AutoSize = true;
            labelDecision.BackColor = SystemColors.ButtonHighlight;
            labelDecision.Font = new Font("T-FLEX Type B", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 204);
            labelDecision.ForeColor = SystemColors.Highlight;
            labelDecision.Location = new Point(12, 749);
            labelDecision.Name = "labelDecision";
            labelDecision.Size = new Size(522, 36);
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
            roundedButton1.Location = new Point(418, 841);
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
            roundedButton2.Location = new Point(12, 841);
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
            // SimulationForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1822, 953);
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
    }
}
