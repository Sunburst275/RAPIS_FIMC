namespace RAPIS_FIMC
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
            this.LoadButton = new System.Windows.Forms.Button();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.FileTextBox = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.FileContentBox = new System.Windows.Forms.RichTextBox();
            this.DeleteFromLabel = new System.Windows.Forms.Label();
            this.FromNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.ToLabel = new System.Windows.Forms.Label();
            this.ToNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.CloseButton = new System.Windows.Forms.Button();
            this.CurrentlyOpenedLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.FromNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ToNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(632, 12);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(75, 23);
            this.LoadButton.TabIndex = 0;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // BrowseButton
            // 
            this.BrowseButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BrowseButton.Location = new System.Drawing.Point(551, 12);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 1;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // FileTextBox
            // 
            this.FileTextBox.Location = new System.Drawing.Point(12, 12);
            this.FileTextBox.Name = "FileTextBox";
            this.FileTextBox.Size = new System.Drawing.Size(533, 20);
            this.FileTextBox.TabIndex = 2;
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(579, 287);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(127, 37);
            this.StartButton.TabIndex = 3;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // FileContentBox
            // 
            this.FileContentBox.DetectUrls = false;
            this.FileContentBox.EnableAutoDragDrop = true;
            this.FileContentBox.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileContentBox.HideSelection = false;
            this.FileContentBox.Location = new System.Drawing.Point(12, 39);
            this.FileContentBox.Name = "FileContentBox";
            this.FileContentBox.ReadOnly = true;
            this.FileContentBox.Size = new System.Drawing.Size(694, 242);
            this.FileContentBox.TabIndex = 4;
            this.FileContentBox.TabStop = false;
            this.FileContentBox.Text = "";
            this.FileContentBox.WordWrap = false;
            // 
            // DeleteFromLabel
            // 
            this.DeleteFromLabel.AutoSize = true;
            this.DeleteFromLabel.Location = new System.Drawing.Point(12, 288);
            this.DeleteFromLabel.Name = "DeleteFromLabel";
            this.DeleteFromLabel.Size = new System.Drawing.Size(61, 13);
            this.DeleteFromLabel.TabIndex = 5;
            this.DeleteFromLabel.Text = "Delete from";
            // 
            // FromNumericUpDown
            // 
            this.FromNumericUpDown.Location = new System.Drawing.Point(79, 286);
            this.FromNumericUpDown.Name = "FromNumericUpDown";
            this.FromNumericUpDown.Size = new System.Drawing.Size(70, 20);
            this.FromNumericUpDown.TabIndex = 6;
            this.FromNumericUpDown.ValueChanged += new System.EventHandler(this.FromNumericUpDown_ValueChanged);
            // 
            // ToLabel
            // 
            this.ToLabel.AutoSize = true;
            this.ToLabel.Location = new System.Drawing.Point(155, 288);
            this.ToLabel.Name = "ToLabel";
            this.ToLabel.Size = new System.Drawing.Size(16, 13);
            this.ToLabel.TabIndex = 7;
            this.ToLabel.Text = "to";
            // 
            // ToNumericUpDown
            // 
            this.ToNumericUpDown.Location = new System.Drawing.Point(177, 286);
            this.ToNumericUpDown.Name = "ToNumericUpDown";
            this.ToNumericUpDown.Size = new System.Drawing.Size(70, 20);
            this.ToNumericUpDown.TabIndex = 8;
            this.ToNumericUpDown.ValueChanged += new System.EventHandler(this.ToNumericUpDown_ValueChanged);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(446, 287);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(127, 37);
            this.CloseButton.TabIndex = 9;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // CurrentlyOpenedLabel
            // 
            this.CurrentlyOpenedLabel.AutoSize = true;
            this.CurrentlyOpenedLabel.Location = new System.Drawing.Point(12, 314);
            this.CurrentlyOpenedLabel.MaximumSize = new System.Drawing.Size(500, 0);
            this.CurrentlyOpenedLabel.Name = "CurrentlyOpenedLabel";
            this.CurrentlyOpenedLabel.Size = new System.Drawing.Size(90, 13);
            this.CurrentlyOpenedLabel.TabIndex = 10;
            this.CurrentlyOpenedLabel.Text = "Currently opened:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 336);
            this.ControlBox = false;
            this.Controls.Add(this.CurrentlyOpenedLabel);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.ToNumericUpDown);
            this.Controls.Add(this.ToLabel);
            this.Controls.Add(this.FromNumericUpDown);
            this.Controls.Add(this.DeleteFromLabel);
            this.Controls.Add(this.FileContentBox);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.FileTextBox);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.LoadButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(735, 375);
            this.MinimumSize = new System.Drawing.Size(735, 375);
            this.Name = "MainForm";
            this.Text = "Multiliner";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.FromNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ToNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox FileTextBox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.RichTextBox FileContentBox;
        private System.Windows.Forms.Label DeleteFromLabel;
        private System.Windows.Forms.NumericUpDown FromNumericUpDown;
        private System.Windows.Forms.Label ToLabel;
        private System.Windows.Forms.NumericUpDown ToNumericUpDown;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Label CurrentlyOpenedLabel;
    }
}

