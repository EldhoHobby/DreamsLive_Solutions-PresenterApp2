namespace DreamsLive_Solutions_PresenterApp1
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
            this.tnBrowse = new System.Windows.Forms.Button();
            this.lblImagePath = new System.Windows.Forms.Label();
            this.cmbDisplays = new System.Windows.Forms.ComboBox();
            this.btnStartPresentation = new System.Windows.Forms.Button();
            this.picPreview = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // tnBrowse
            // 
            this.tnBrowse.Location = new System.Drawing.Point(130, 65);
            this.tnBrowse.Name = "tnBrowse";
            this.tnBrowse.Size = new System.Drawing.Size(75, 23);
            this.tnBrowse.TabIndex = 0;
            this.tnBrowse.Text = "Browse...";
            this.tnBrowse.UseVisualStyleBackColor = true;
            this.tnBrowse.Click += new System.EventHandler(this.tnBrowse_Click);
            // 
            // lblImagePath
            // 
            this.lblImagePath.AutoSize = true;
            this.lblImagePath.Location = new System.Drawing.Point(130, 108);
            this.lblImagePath.Name = "lblImagePath";
            this.lblImagePath.Size = new System.Drawing.Size(113, 13);
            this.lblImagePath.TabIndex = 1;
            this.lblImagePath.Text = "Selected Image: None";
            // 
            // cmbDisplays
            // 
            this.cmbDisplays.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDisplays.FormattingEnabled = true;
            this.cmbDisplays.Location = new System.Drawing.Point(147, 159);
            this.cmbDisplays.Name = "cmbDisplays";
            this.cmbDisplays.Size = new System.Drawing.Size(121, 21);
            this.cmbDisplays.TabIndex = 2;
            // 
            // btnStartPresentation
            // 
            this.btnStartPresentation.Location = new System.Drawing.Point(321, 159);
            this.btnStartPresentation.Name = "btnStartPresentation";
            this.btnStartPresentation.Size = new System.Drawing.Size(108, 23);
            this.btnStartPresentation.TabIndex = 3;
            this.btnStartPresentation.Text = "Start Presentation";
            this.btnStartPresentation.UseVisualStyleBackColor = true;
            this.btnStartPresentation.Click += new System.EventHandler(this.btnStartPresentation_Click);
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(488, 238);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(300, 200);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 4;
            this.picPreview.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.btnStartPresentation);
            this.Controls.Add(this.cmbDisplays);
            this.Controls.Add(this.lblImagePath);
            this.Controls.Add(this.tnBrowse);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button tnBrowse;
        private System.Windows.Forms.Label lblImagePath;
        private System.Windows.Forms.ComboBox cmbDisplays;
        private System.Windows.Forms.Button btnStartPresentation;
        private System.Windows.Forms.PictureBox picPreview;
    }
}

