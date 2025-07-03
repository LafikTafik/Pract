namespace NAMI.Foms
{
    partial class SignRecognitionForm
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
            labelSign = new Label();
            picboxsign = new PictureBox();
            roundedButton3 = new NAMI.Helpers.RoundedButton();
            roundedButton1 = new NAMI.Helpers.RoundedButton();
            ((System.ComponentModel.ISupportInitialize)picboxsign).BeginInit();
            SuspendLayout();
            // 
            // labelSign
            // 
            labelSign.AutoSize = true;
            labelSign.BackColor = SystemColors.ButtonHighlight;
            labelSign.Font = new Font("T-FLEX Type B", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 204);
            labelSign.ForeColor = SystemColors.Highlight;
            labelSign.Location = new Point(12, 447);
            labelSign.Name = "labelSign";
            labelSign.Size = new Size(82, 36);
            labelSign.TabIndex = 10;
            labelSign.Text = "Знак:";
            // 
            // picboxsign
            // 
            picboxsign.Location = new Point(12, 12);
            picboxsign.Name = "picboxsign";
            picboxsign.Size = new Size(806, 432);
            picboxsign.SizeMode = PictureBoxSizeMode.Zoom;
            picboxsign.TabIndex = 9;
            picboxsign.TabStop = false;
            // 
            // roundedButton3
            // 
            roundedButton3.BackColor = SystemColors.ControlLightLight;
            roundedButton3.CornerRadius = 20;
            roundedButton3.FlatStyle = FlatStyle.Flat;
            roundedButton3.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton3.ForeColor = SystemColors.Highlight;
            roundedButton3.Location = new Point(12, 539);
            roundedButton3.Name = "roundedButton3";
            roundedButton3.Size = new Size(400, 100);
            roundedButton3.TabIndex = 11;
            roundedButton3.Text = "Загрузить знак";
            roundedButton3.UseVisualStyleBackColor = false;
            roundedButton3.Click += roundedButton3_Click;
            // 
            // roundedButton1
            // 
            roundedButton1.BackColor = SystemColors.ControlLightLight;
            roundedButton1.CornerRadius = 20;
            roundedButton1.FlatStyle = FlatStyle.Flat;
            roundedButton1.Font = new Font("T-FLEX Type B", 25.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            roundedButton1.ForeColor = SystemColors.Highlight;
            roundedButton1.Location = new Point(418, 539);
            roundedButton1.Name = "roundedButton1";
            roundedButton1.Size = new Size(400, 100);
            roundedButton1.TabIndex = 12;
            roundedButton1.Text = "Выход";
            roundedButton1.UseVisualStyleBackColor = false;
            roundedButton1.Click += roundedButton1_Click;
            // 
            // SignRecognitionForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(838, 651);
            Controls.Add(roundedButton1);
            Controls.Add(roundedButton3);
            Controls.Add(labelSign);
            Controls.Add(picboxsign);
            Name = "SignRecognitionForm";
            Text = "SignRecognitionForm";
            ((System.ComponentModel.ISupportInitialize)picboxsign).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelSign;
        private PictureBox picboxsign;
        private Helpers.RoundedButton roundedButton3;
        private Helpers.RoundedButton roundedButton1;
    }
}