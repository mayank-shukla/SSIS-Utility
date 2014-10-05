namespace BIDSCompare
{
    partial class CopyXML
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CopyXML));
            this.txt_InputXML = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.bwParseXML = new System.ComponentModel.BackgroundWorker();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // txt_InputXML
            // 
            this.txt_InputXML.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_InputXML.Font = new System.Drawing.Font("Verdana", 9.75F);
            this.txt_InputXML.ForeColor = System.Drawing.Color.Black;
            this.txt_InputXML.Location = new System.Drawing.Point(1, 2);
            this.txt_InputXML.MaxLength = 999999999;
            this.txt_InputXML.Multiline = true;
            this.txt_InputXML.Name = "txt_InputXML";
            this.txt_InputXML.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_InputXML.Size = new System.Drawing.Size(755, 473);
            this.txt_InputXML.TabIndex = 3;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(5, 481);
            this.progressBar1.Maximum = 10000;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(742, 10);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // btnSubmit
            // 
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmit.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnSubmit.Location = new System.Drawing.Point(680, 497);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(67, 23);
            this.btnSubmit.TabIndex = 12;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // bwParseXML
            // 
            this.bwParseXML.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwParseXML_DoWork);
            this.bwParseXML.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwParseXML_ProgressChanged);
            this.bwParseXML.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwParseXML_RunWorkerCompleted);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // CopyXML
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(753, 522);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.txt_InputXML);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(759, 550);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(759, 550);
            this.Name = "CopyXML";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Copy SSIS XML";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_InputXML;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnSubmit;
        private System.ComponentModel.BackgroundWorker bwParseXML;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}