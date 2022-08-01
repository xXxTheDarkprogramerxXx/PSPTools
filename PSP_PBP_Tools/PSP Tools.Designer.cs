namespace PSP_PBP_Tools
{
    partial class PSP_Tools_Form
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnLoadISO = new System.Windows.Forms.Button();
            this.txtISOLock = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnLoadISO);
            this.groupBox1.Controls.Add(this.txtISOLock);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 230);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ISO/CSO";
            // 
            // btnLoadISO
            // 
            this.btnLoadISO.Location = new System.Drawing.Point(219, 49);
            this.btnLoadISO.Name = "btnLoadISO";
            this.btnLoadISO.Size = new System.Drawing.Size(34, 23);
            this.btnLoadISO.TabIndex = 2;
            this.btnLoadISO.Text = "...";
            this.btnLoadISO.UseVisualStyleBackColor = true;
            this.btnLoadISO.Click += new System.EventHandler(this.btnLoadISO_Click);
            // 
            // txtISOLock
            // 
            this.txtISOLock.Location = new System.Drawing.Point(7, 49);
            this.txtISOLock.Name = "txtISOLock";
            this.txtISOLock.Size = new System.Drawing.Size(206, 22);
            this.txtISOLock.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "ISO Reader";
            // 
            // PSP_Tools_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 411);
            this.Controls.Add(this.groupBox1);
            this.Name = "PSP_Tools_Form";
            this.Text = "PSP_Tools";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnLoadISO;
        private System.Windows.Forms.TextBox txtISOLock;
        private System.Windows.Forms.Label label1;
    }
}