using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FIP4DCS
{
    public partial class FormMain : Form
    {
        private ModularFip mf = new ModularFip("FIP4DCS");
        private List<Profile> profiles = new List<Profile>();
        private DCSBios.DCSBios dcs;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer timerFps = new System.Windows.Forms.Timer();
        private int fps = 0;

        public FormMain()
        {
            InitializeComponent();
            SetMiniMode(Properties.Settings.Default.MiniMode);
            ToggleIPSettings(Properties.Settings.Default.ShowIPSettings);
            if (mf.InitOk) ToggleStartStopbutton(2);
            else ToggleStartStopbutton(0);

            CheckProfileDirectories();

            string dataIP = "239.255.50.10";
            int dataPort = 5010;
            string listenIP = "127.0.0.1";
            int listenPort = 7778;

            
            if (Properties.Settings.Default.DCSBiosDataIP != "") dataIP = Properties.Settings.Default.DCSBiosDataIP;
            if (Properties.Settings.Default.DCSBiosDataPort > 0) dataPort = Properties.Settings.Default.DCSBiosDataPort;
            if (Properties.Settings.Default.DCSBiosListenIP != "") listenIP = Properties.Settings.Default.DCSBiosListenIP;
            if (Properties.Settings.Default.DCSBiosListenPort > 0) listenPort = Properties.Settings.Default.DCSBiosListenPort;

            
            maskedTextBoxDataIP.Text = dataIP;
            maskedTextBoxDataPort.Text = dataPort.ToString();
            maskedTextBoxListenIP.Text = listenIP;
            maskedTextBoxListenPort.Text = listenPort.ToString();
            
            dcs = new DCSBios.DCSBios(dataIP, dataPort, listenIP, listenPort);
        }

        private void CheckProfileDirectories()
        {
            comboBoxProfiles.Items.Clear();
            try
            {
                var directories = Directory.EnumerateDirectories(Directory.GetCurrentDirectory() + "\\Gauges");

                foreach (var directory in directories)
                {
                    profiles.Add(new Profile(new DcsFipHtmlProfile(directory)));
                    comboBoxProfiles.Items.Add(profiles[profiles.Count() - 1]);
                }

                string sgPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "saved games");
                directories = Directory.EnumerateDirectories(sgPath + "\\DCS" + "\\Gauges");
                foreach (var directory in directories)
                {
                    profiles.Add(new Profile(new DcsFipHtmlProfile(directory)));
                    comboBoxProfiles.Items.Add(profiles[profiles.Count() - 1]);
                }
            }
            catch { }

            comboBoxProfiles.SelectedIndex = 0;
        }

        private void buttonStartStopFip_Click(object sender, EventArgs e)
        {
            if (!mf.InitOk)
            {
                ToggleStartStopbutton(1);
                mf.Initialize();
                if (mf.InitOk) ToggleStartStopbutton(2);
                else ToggleStartStopbutton(0);
            }
            else
            {
                ToggleStartStopbutton(1);
                mf.Close();
                ToggleStartStopbutton(0);
            }
        }

        private void ToggleStartStopbutton(uint status = 0)
        {
            if (status == 0)
            {
                buttonStartStopFip.BackColor = Color.Red;
                buttonStartStopFip.ForeColor = Color.White;
                buttonStartStopFip.Text = Properties.Resources.StartFip;
            }
            if (status == 1)
            {
                buttonStartStopFip.BackColor = Color.Yellow;
                buttonStartStopFip.ForeColor = Color.Black;
                buttonStartStopFip.Text = Properties.Resources.StopFip;
            }
            if (status == 2)
            {
                buttonStartStopFip.BackColor = Color.Green;
                buttonStartStopFip.ForeColor = Color.White;
                buttonStartStopFip.Text = Properties.Resources.StopFip;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            mf.Close();
            if (timer.Enabled) timer.Stop();
            dcs.StopDcsUdp();
        }

        private void buttonShowProfile_Click(object sender, EventArgs e)
        {
            if (!timer.Enabled)
            {
                buttonShowProfile.Text = Properties.Resources.StopProfile;
                comboBoxProfiles.Enabled = false;
                if (timer.Interval == 100)
                {
                    timer.Tick += RefreshScreens;
                    timer.Interval = 1;

                    timerFps.Tick += ResetFps;
                    timerFps.Interval = 1000;
                }

                foreach (Profile item in profiles)
                {
                    if (item.ToString() == comboBoxProfiles.Text)
                    {
                        item.HtmlProfile.PrepareProfile();
                        break;
                    }
                }
                dcs.StartDcsUdp();
                timer.Start();
                timerFps.Start();
            }
            else
            {
                timer.Stop(); 
                timerFps.Stop(); 
                dcs.StopDcsUdp(); 
                comboBoxProfiles.Enabled = true;
                buttonShowProfile.Text = Properties.Resources.StartProfile;
            }
        }

        private void ResetFps(object sender, EventArgs e)
        {
            foreach (Profile item in profiles)
            {
                item.FPS = fps;
            }
            fps = 0;
        }

        private void RefreshScreens(object sender, EventArgs e)
        {
            if (comboBoxProfiles.Text == "") return;
            fps++;
            dcs.PrepareList();
            foreach (Profile item in profiles)
            {
                if (item.ToString() == comboBoxProfiles.Text)
                {
                    item.HtmlProfile.miniBios = dcs.miniBios;
                    if (item.HtmlProfile.sendDCSBIOS != "")
                    {
                        dcs.Send(item.HtmlProfile.sendDCSBIOS);
                        item.HtmlProfile.sendDCSBIOS = "";
                    }

                    item.HtmlProfile.PrepareRender("index.cshtml", null);
                    if (checkBoxRenderFip.Checked && mf.InitOk)
                    {
                        mf.SetRedrawDelegate(item.HtmlProfile.Render, 0);
                        mf.SetButtonsDelegate(item.HtmlProfile.Buttons, 0);

                        mf.RefreshAllPages();
                    }
                    if (checkBoxRenderPreview.Checked)
                    {
                        if (!checkBoxRenderFip.Checked || !mf.InitOk) item.HtmlProfile.Render(Graphics.FromImage(new Bitmap(1, 1)), true);
                        if (item.HtmlProfile.LastImage != null) pictureBoxFip.Image = item.HtmlProfile.LastImage;
                    }
                    else
                    {
                        pictureBoxFip.Image = new Bitmap(1, 1);
                    }
                    return;
                }
            }
        }

        private void buttonRefreshProfiles_Click(object sender, EventArgs e)
        {
            CheckProfileDirectories();
        }

        private void maskedTextBoxDataIP_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.DCSBiosDataIP = maskedTextBoxDataIP.Text;
            buttonSaveSettings.Text = Properties.Resources.Save + "*";
        }

        private void maskedTextBoxDataPort_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.DCSBiosDataPort = int.Parse(maskedTextBoxDataPort.Text);
            buttonSaveSettings.Text = Properties.Resources.Save + "*";
        }

        private void maskedTextBoxListenIP_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.DCSBiosListenIP = maskedTextBoxListenIP.Text;
            buttonSaveSettings.Text = Properties.Resources.Save + "*";
        }

        private void maskedTextBoxListenPort_Validated(object sender, EventArgs e)
        {
            Properties.Settings.Default.DCSBiosListenPort = int.Parse(maskedTextBoxListenPort.Text);
            buttonSaveSettings.Text = Properties.Resources.Save + "*";
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            buttonSaveSettings.Text = Properties.Resources.Save;
        }

        private void miniModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMiniMode();
        }

        private void showIPSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowIPSettings = !Properties.Settings.Default.ShowIPSettings;
            ToggleIPSettings(Properties.Settings.Default.ShowIPSettings);
            Properties.Settings.Default.Save();
        }

        private void ToggleIPSettings(bool show = false)
        {
            if (show)
            {
                this.Width = 484;
            } else
            {
                this.Width = 316;
            }
            showIPSettingsToolStripMenuItem.Checked = show;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ToggleMiniMode()
        {
            Properties.Settings.Default.MiniMode = !Properties.Settings.Default.MiniMode;
            SetMiniMode(Properties.Settings.Default.MiniMode);
            Properties.Settings.Default.Save();
        }

        private void SetMiniMode(bool mini = false)
        {
            buttonStartStopFip.Visible = !mini;
            comboBoxProfiles.Visible = !mini;
            buttonShowProfile.Visible = !mini;
            checkBoxRenderFip.Visible = !mini;
            checkBoxRenderPreview.Visible = !mini;
            buttonRefreshProfiles.Visible = !mini;
            miniModeToolStripMenuItem.Checked = mini;
            if (mini)
            {
                pictureBoxFip.Location = new Point(pictureBoxFip.Location.X, 27);
                this.Height -= (96-27);
            } else
            {
                pictureBoxFip.Location = new Point(pictureBoxFip.Location.X, 96);
                this.Height += (96 - 27);
            }
        }

        private void startFIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonStartStopFip_Click(sender, e);
        }

        private void startProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonShowProfile_Click(sender, e);
        }
    }
}
