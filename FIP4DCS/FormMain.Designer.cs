namespace FIP4DCS
{
    partial class FormMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.buttonStartStopFip = new System.Windows.Forms.Button();
            this.comboBoxProfiles = new System.Windows.Forms.ComboBox();
            this.buttonShowProfile = new System.Windows.Forms.Button();
            this.pictureBoxFip = new System.Windows.Forms.PictureBox();
            this.checkBoxRenderPreview = new System.Windows.Forms.CheckBox();
            this.checkBoxRenderFip = new System.Windows.Forms.CheckBox();
            this.buttonRefreshProfiles = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.maskedTextBoxDataIP = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.maskedTextBoxDataPort = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.maskedTextBoxListenIP = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.maskedTextBoxListenPort = new System.Windows.Forms.MaskedTextBox();
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFip)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStartStopFip
            // 
            resources.ApplyResources(this.buttonStartStopFip, "buttonStartStopFip");
            this.buttonStartStopFip.Name = "buttonStartStopFip";
            this.buttonStartStopFip.UseVisualStyleBackColor = true;
            this.buttonStartStopFip.Click += new System.EventHandler(this.buttonStartStopFip_Click);
            // 
            // comboBoxProfiles
            // 
            this.comboBoxProfiles.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxProfiles, "comboBoxProfiles");
            this.comboBoxProfiles.Name = "comboBoxProfiles";
            // 
            // buttonShowProfile
            // 
            resources.ApplyResources(this.buttonShowProfile, "buttonShowProfile");
            this.buttonShowProfile.Name = "buttonShowProfile";
            this.buttonShowProfile.UseVisualStyleBackColor = true;
            this.buttonShowProfile.Click += new System.EventHandler(this.buttonShowProfile_Click);
            // 
            // pictureBoxFip
            // 
            resources.ApplyResources(this.pictureBoxFip, "pictureBoxFip");
            this.pictureBoxFip.Name = "pictureBoxFip";
            this.pictureBoxFip.TabStop = false;
            // 
            // checkBoxRenderPreview
            // 
            resources.ApplyResources(this.checkBoxRenderPreview, "checkBoxRenderPreview");
            this.checkBoxRenderPreview.Checked = true;
            this.checkBoxRenderPreview.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRenderPreview.Name = "checkBoxRenderPreview";
            this.checkBoxRenderPreview.UseVisualStyleBackColor = true;
            // 
            // checkBoxRenderFip
            // 
            resources.ApplyResources(this.checkBoxRenderFip, "checkBoxRenderFip");
            this.checkBoxRenderFip.Checked = true;
            this.checkBoxRenderFip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRenderFip.Name = "checkBoxRenderFip";
            this.checkBoxRenderFip.UseVisualStyleBackColor = true;
            // 
            // buttonRefreshProfiles
            // 
            resources.ApplyResources(this.buttonRefreshProfiles, "buttonRefreshProfiles");
            this.buttonRefreshProfiles.Name = "buttonRefreshProfiles";
            this.buttonRefreshProfiles.UseVisualStyleBackColor = true;
            this.buttonRefreshProfiles.Click += new System.EventHandler(this.buttonRefreshProfiles_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // maskedTextBoxDataIP
            // 
            resources.ApplyResources(this.maskedTextBoxDataIP, "maskedTextBoxDataIP");
            this.maskedTextBoxDataIP.Name = "maskedTextBoxDataIP";
            this.maskedTextBoxDataIP.Validated += new System.EventHandler(this.maskedTextBoxDataIP_Validated);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // maskedTextBoxDataPort
            // 
            resources.ApplyResources(this.maskedTextBoxDataPort, "maskedTextBoxDataPort");
            this.maskedTextBoxDataPort.Name = "maskedTextBoxDataPort";
            this.maskedTextBoxDataPort.Validated += new System.EventHandler(this.maskedTextBoxDataPort_Validated);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // maskedTextBoxListenIP
            // 
            resources.ApplyResources(this.maskedTextBoxListenIP, "maskedTextBoxListenIP");
            this.maskedTextBoxListenIP.Name = "maskedTextBoxListenIP";
            this.maskedTextBoxListenIP.Validated += new System.EventHandler(this.maskedTextBoxListenIP_Validated);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // maskedTextBoxListenPort
            // 
            resources.ApplyResources(this.maskedTextBoxListenPort, "maskedTextBoxListenPort");
            this.maskedTextBoxListenPort.Name = "maskedTextBoxListenPort";
            this.maskedTextBoxListenPort.Validated += new System.EventHandler(this.maskedTextBoxListenPort_Validated);
            // 
            // buttonSaveSettings
            // 
            resources.ApplyResources(this.buttonSaveSettings, "buttonSaveSettings");
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.UseVisualStyleBackColor = true;
            this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonSaveSettings);
            this.Controls.Add(this.maskedTextBoxListenPort);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.maskedTextBoxListenIP);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.maskedTextBoxDataPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.maskedTextBoxDataIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonRefreshProfiles);
            this.Controls.Add(this.checkBoxRenderFip);
            this.Controls.Add(this.checkBoxRenderPreview);
            this.Controls.Add(this.pictureBoxFip);
            this.Controls.Add(this.buttonShowProfile);
            this.Controls.Add(this.comboBoxProfiles);
            this.Controls.Add(this.buttonStartStopFip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFip)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStartStopFip;
        private System.Windows.Forms.ComboBox comboBoxProfiles;
        private System.Windows.Forms.Button buttonShowProfile;
        private System.Windows.Forms.PictureBox pictureBoxFip;
        private System.Windows.Forms.CheckBox checkBoxRenderPreview;
        private System.Windows.Forms.CheckBox checkBoxRenderFip;
        private System.Windows.Forms.Button buttonRefreshProfiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxDataIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxDataPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxListenIP;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxListenPort;
        private System.Windows.Forms.Button buttonSaveSettings;
    }
}

