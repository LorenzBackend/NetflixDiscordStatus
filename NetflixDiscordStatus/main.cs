using NetflixDiscordStatus.Api;
using NetflixDiscordStatus.Misc;
using NetflixDiscordStatus.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetflixDiscordStatus
{
    //NetflixDiscordStatus by LrnzCode
    //https://github.com/lrnzcode/NetflixDiscordStatus
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
            MyEventHandler.onInitResult += onInitResult;
            MyEventHandler.onUnexpectedError += onExpectedError;
        }

        private static string currentVersion = "v1.0.2\n";
        private void main_Load(object sender, EventArgs e)
        {
            CheckUpdate();

            Thread th = new Thread(Init);
            th.Start();
        }

        private void CheckUpdate()
        {
            try
            {
                string version = new WebClient().DownloadString("https://raw.githubusercontent.com/lrnzcode/NetflixDiscordStatus/master/version");
               
                if (!currentVersion.Equals(version)) 
                {
                    DialogResult result = MessageBox.Show("NetflixDiscordStatus has a new update do you want to download the new version now?", "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start("https://github.com/lrnzcode/NetflixDiscordStatus/releases");
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Check Failed");
            }
        }

        private static bool IsProcessRunning(string name)
        {
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        private void Init()
        {
            BeginInvoke(new Action(() =>
            {
                if (IsProcessRunning("chrome") || IsProcessRunning("chromedriver"))
                {
                    DialogResult result = MessageBox.Show("Google Chrome or the Chrome driver must be closed, should Chrome be closed now?", "Google Chrome is Running", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        ShutdownDriver();
                    }
                    else
                    {
                        lblState.Text = "Close Google Chrome and try again";
                        btnRetry.Show();
                        return;
                    }
                }

                Thread.Sleep(100);
                Core.CheckNetflixPorifle();
            }));
        }

        private void onInitResult(string message, bool success)
        {
            BeginInvoke(new Action(() =>
            {
                lblState.Text = message;

                if (success)
                {
                    btnRetry.Hide();
                    btnHide.Show();
                    remeber.Show();

                    if (Settings.Default.runInback) this.Hide();
                }
                else
                {
                    ShutdownDriver();
                    btnRetry.Show();
                }

            }));
        }

        private void onExpectedError(string message)
        {
            ShutdownDriver();
            BeginInvoke(new Action(() =>
            {
                ShutdownDriver();
                remeber.Hide();
                btnHide.Hide();
                btnRetry.Show();
                lblState.Text = message;
            }));
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            if (remeber.Checked)
            {
                Settings.Default.runInback = true;
                Settings.Default.Save();
            }

            this.Hide();
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            Init();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CloseApp();
        }

        private void ShutdownDriver()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C taskkill /IM chromedriver.exe /f";
                process.StartInfo = startInfo;
                process.Start();


                Process process2 = new Process();
                ProcessStartInfo startInfo2 = new ProcessStartInfo();
                startInfo2.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo2.FileName = "cmd.exe";
                startInfo2.Arguments = "/C taskkill /IM chrome.exe /f";
                process2.StartInfo = startInfo2;
                process2.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Shutdown Driver", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CloseApp()
        {
            NetflixState.Shutdown();
            ShutdownDriver();
            Environment.Exit(0);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.google.com/chrome/");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseApp();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void resetAutoBackgrorundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.runInback = false;
            Settings.Default.Save();
            remeber.Show();
            btnHide.Show();
            this.Show();
        }

        private void lblState_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show(lblState.Text, lblState);
        }
    }
}
