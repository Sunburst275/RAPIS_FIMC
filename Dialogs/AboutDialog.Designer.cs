namespace RAPIS_FIMC
{
    partial class AboutDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
            this.TextBox = new System.Windows.Forms.RichTextBox();
            this.CloseButton = new System.Windows.Forms.Button();
            this.Sunburst275Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextBox
            // 
            this.TextBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.TextBox.Location = new System.Drawing.Point(13, 13);
            this.TextBox.Name = "TextBox";
            this.TextBox.ReadOnly = true;
            this.TextBox.Size = new System.Drawing.Size(234, 282);
            this.TextBox.TabIndex = 0;
            this.TextBox.Text = resources.GetString("TextBox.Text");
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(172, 301);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // Sunburst275Button
            // 
            this.Sunburst275Button.Location = new System.Drawing.Point(13, 301);
            this.Sunburst275Button.Name = "Sunburst275Button";
            this.Sunburst275Button.Size = new System.Drawing.Size(153, 23);
            this.Sunburst275Button.TabIndex = 2;
            this.Sunburst275Button.Text = "Sunburst275";
            this.Sunburst275Button.UseVisualStyleBackColor = true;
            this.Sunburst275Button.Click += new System.EventHandler(this.Sunburst275Button_Click);
            // 
            // AboutDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(259, 336);
            this.ControlBox = false;
            this.Controls.Add(this.Sunburst275Button);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.TextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(275, 375);
            this.MinimumSize = new System.Drawing.Size(275, 375);
            this.Name = "AboutDialog";
            this.Text = "About / Help";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox TextBox;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button Sunburst275Button;
    }
}