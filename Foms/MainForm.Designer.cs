namespace NAMI.Foms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            roundedButton1 = new NAMI.Helpers.RoundedButton();
            roundedButton2 = new NAMI.Helpers.RoundedButton();
            roundedButton3 = new NAMI.Helpers.RoundedButton();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("T-FLEX Type B", 36F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.ForeColor = SystemColors.Control;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(933, 130);
            label1.TabIndex = 0;
            label1.Text = "Система восприятия среды \r\nавтономного транспортного средства";
            // 
            // roundedButton1
            // 
            roundedButton1.BackColor = SystemColors.ButtonHighlight;
            roundedButton1.CornerRadius = 20;
            roundedButton1.Font = new Font("T-FLEX Type B", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton1.ForeColor = SystemColors.Highlight;
            roundedButton1.Location = new Point(12, 180);
            roundedButton1.Name = "roundedButton1";
            roundedButton1.Size = new Size(380, 110);
            roundedButton1.TabIndex = 5;
            roundedButton1.Text = "Запустить симуляцию";
            roundedButton1.UseVisualStyleBackColor = false;
            roundedButton1.Click += roundedButton1_Click;
            // 
            // roundedButton2
            // 
            roundedButton2.BackColor = SystemColors.ButtonHighlight;
            roundedButton2.CornerRadius = 20;
            roundedButton2.Font = new Font("T-FLEX Type B", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton2.ForeColor = SystemColors.Highlight;
            roundedButton2.Location = new Point(12, 310);
            roundedButton2.Name = "roundedButton2";
            roundedButton2.Size = new Size(380, 110);
            roundedButton2.TabIndex = 6;
            roundedButton2.Text = "Настройки";
            roundedButton2.UseVisualStyleBackColor = false;
            roundedButton2.Click += roundedButton2_Click;
            // 
            // roundedButton3
            // 
            roundedButton3.BackColor = SystemColors.ButtonHighlight;
            roundedButton3.CornerRadius = 20;
            roundedButton3.Font = new Font("T-FLEX Type B", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton3.ForeColor = SystemColors.Highlight;
            roundedButton3.Location = new Point(12, 440);
            roundedButton3.Name = "roundedButton3";
            roundedButton3.Size = new Size(380, 110);
            roundedButton3.TabIndex = 7;
            roundedButton3.Text = "Выход";
            roundedButton3.UseVisualStyleBackColor = false;
            roundedButton3.Click += roundedButton3_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(960, 562);
            Controls.Add(roundedButton3);
            Controls.Add(roundedButton2);
            Controls.Add(roundedButton1);
            Controls.Add(label1);
            Name = "MainForm";
            Text = "SimulationForm";
            Load += SimulationForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Helpers.RoundedButton roundedButton1;
        private Helpers.RoundedButton roundedButton2;
        private Helpers.RoundedButton roundedButton3;
    }
}