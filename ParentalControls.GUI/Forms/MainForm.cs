using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.Win32;
using System.Threading;
using ParentalControls.Common;
using System.Runtime.InteropServices;
using System.Net;

namespace ParentalControls.GUI.Forms
{
    public partial class MainForm : Form
    {

        public const string ALARMS_FILE = "alarms.bas";
        public const string CREDENTIALS_FILE = "creds.crd";

        AlarmsFile file;
        ParentalControlsCredentialsFile file2;

        public MainForm()
        {
            InitializeComponent();

            file = AlarmsFile.Load(ALARMS_FILE);
            file2 = ParentalControlsCredentialsFile.Load(CREDENTIALS_FILE);
        }

        void Enable(bool value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(Enable), new object[] { value });
                return;
            }
            this.Enabled = value;
        }

        public void FirstInstall()
        {
            Enable(false);
            RegistryKey reg = ParentalControlsRegistry.GetRegistryKey();
            reg.SetValue("Path", Application.StartupPath, RegistryValueKind.String);
            reg.SetValue("AlarmFile", Application.StartupPath + @"\" + ALARMS_FILE);

            TaskDialog dialog = new TaskDialog();

            dialog.Caption = Application.ProductName+" Setup";
            dialog.InstructionText = Application.ProductName+" is mostly setup!";
            dialog.Text = "What you need to do is setup a password for any cases that you need to force close an alarm.";

            TaskDialogButton button = new TaskDialogButton("btnOne", "Continue");
            button.Click += (aa, ab) =>
            {
                TaskDialogButton tdb = (TaskDialogButton)aa;
                ((TaskDialog)tdb.HostingDialog).Close(TaskDialogResult.Ok);

                if (file2.ParentalControlsCredentials.Count > 0)
                {
                    TaskDialog dadialog = new TaskDialog();
                    dadialog.InstructionText = "Setup Complete!";
                    dadialog.Text = "You are now done setting up Parental Controls!";

                    dadialog.StandardButtons = TaskDialogStandardButtons.Ok;
                    dadialog.Show();

                    return;
                }

                NetworkCredential cred = null;
                WindowsSecurity.GetCredentialsVistaAndUp("Please enter new credentials for Parental Controls", "Set username and password for stopping an alarm.", out cred);
                while (cred == null || (string.IsNullOrWhiteSpace(cred.UserName) || string.IsNullOrWhiteSpace(cred.Password)))
                {
                    WindowsSecurity.GetCredentialsVistaAndUp("Please enter new credentials for Parental Controls", "Set username and password for stopping an alarm. (Credentials must not be empty)", out cred);
                }
                ParentalControlsCredential c;
                try
                {
                    c = (ParentalControlsCredential)cred;
                    c.HashedPassword = SHA256Hash.Hash(c.HashedPassword);
                    file2.Add(c);
                    file2.Save();
                }
                catch {  }
                TaskDialog ndialog = new TaskDialog();
                ndialog.Caption = Application.ProductName + " Setup";
                ndialog.InstructionText = "Want to test your credentials?";
                ndialog.FooterText = "Fun Fact: You can create as many accounts as you want!";
                ndialog.FooterIcon = TaskDialogStandardIcon.Information;

                TaskDialogCommandLink linka = new TaskDialogCommandLink("linkA", "Test Credentials");

                linka.Click += (ba, bb) =>
                {
                    TaskDialogButton tb = (TaskDialogButton)ba;
                    ((TaskDialog)tb.HostingDialog).Close(TaskDialogResult.Yes);

                    WindowsSecurity.GetCredentialsVistaAndUp("Please enter credentials for \"Parental Controls\"", "Force disabling \"TestAlarm\"", out cred);
                    c = new ParentalControlsCredential();
                    try
                    {
                        c = (ParentalControlsCredential)cred;
                        c.HashedPassword = SHA256Hash.Hash(c.HashedPassword);
                    }
                    catch { }

                    bool wevalidated = true;

                    while (cred == null || !file2.Validate(c) || (string.IsNullOrWhiteSpace(cred.UserName) || string.IsNullOrWhiteSpace(cred.Password)))
                    {
                        TaskDialog ddialog = new TaskDialog();

                        ddialog.InstructionText = "Credentials Invalid";
                        ddialog.Text = "You want to stop testing credentials?";

                        ddialog.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;

                        if (ddialog.Show() == TaskDialogResult.Yes)
                        {
                            wevalidated = false;
                            break;
                        }
                        else
                            WindowsSecurity.GetCredentialsVistaAndUp("Please enter credentials for \"Parental Controls\"", "Force disabling \"TestAlarm\"", out cred);
                    }
                    TaskDialog dadialog = new TaskDialog();
                    if (wevalidated)
                    {
                        dadialog.InstructionText = "Credentials Valid!";
                    }
                    else
                    {
                        dadialog.InstructionText = "Setup Complete!";
                    }
                    dadialog.Text = "You are now done setting up Parental Controls!";

                    dadialog.StandardButtons = TaskDialogStandardButtons.Ok;
                    dadialog.Show();
                };

                TaskDialogCommandLink linkb = new TaskDialogCommandLink("linkB", "Skip Test");

                linkb.Click += (ba, bb) =>
                {
                    TaskDialogButton tb = (TaskDialogButton)ba;
                    ((TaskDialog)tb.HostingDialog).Close(TaskDialogResult.No);

                    TaskDialog dadialog = new TaskDialog();
                    dadialog.InstructionText = "Setup Complete!";
                    dadialog.Text = "You are now done setting up Parental Controls!";

                    dadialog.StandardButtons = TaskDialogStandardButtons.Ok;
                    dadialog.Show();
                };

                ndialog.Controls.Add(linka);
                ndialog.Controls.Add(linkb);

                ndialog.Show();
                file2.Save();
            };
            dialog.Controls.Add(button);
            
            dialog.Show();
            Enable(true);
            // Kind of an hacky way of making this window get focused after showing dialogs.
            SwitchToSelf();
        }

        /// <summary>
        /// Forces Windows to focus on this window, used in first startup.
        /// </summary>
        private void SwitchToSelf()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { SwitchToThisWindow(this.Handle); }));
            }
            else
            {
                SwitchToThisWindow(this.Handle);
            }
        }

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr windowHandle, bool altTab = true);

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if ((bool)Properties.Settings.Default["FirstTimeStartup"] == false)
                {
                    Thread t = new Thread(FirstInstall);
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FirstTimeSetup failed: {0}", ex);
            }

            RefreshItems(true);
        }

        void RefreshItems(bool shouldDisable = false)
        {
            if(shouldDisable)
                foreach (Control c in groupBox1.Controls)
                    c.Enabled = false;

            listBox1.Items.Clear();
            foreach (Alarm alarm in file.Alarms)
            {
                listBox1.Items.Add(alarm);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void Save()
        {
            if(file.IsValidForSaving())
                file.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        static IEnumerable<Enum> GetFlags(Enum input)
        {
            return Enum.GetValues(input.GetType()).Cast<Enum>().Where(input.HasFlag);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                checkedListBox1.Enabled = textBox1.Enabled = domainUpDown1.Enabled = numericUpDown1.Enabled = numericUpDown2.Enabled = checkBox1.Enabled = button1.Enabled = button3.Enabled = true;

                Alarm alarm = (Alarm)listBox1.SelectedItem;

                textBox1.Text = alarm.Name;
                checkBox1.Checked = alarm.Enabled;

                checkedListBox1.SelectedItems.Clear();
                checkedListBox1.Items.Clear();
                checkedListBox1.Items.AddRange(Enum.GetNames(typeof(DayOfWeek)));
                foreach (Enum en in GetFlags(alarm.RepeatDays))
                {
                    DayOfWeek week = (DayOfWeek)en;
                    checkedListBox1.SelectedItems.Add(Enum.GetName(typeof(DayOfWeek), week));
                }

                if(alarm.AlarmTime.Hour > 12)
                {
                    numericUpDown1.Value = alarm.AlarmTime.Hour - 12;
                    domainUpDown1.Text = "PM";
                }
                else
                {
                    numericUpDown1.Value = alarm.AlarmTime.Hour;
                    domainUpDown1.Text = "AM";
                }

                numericUpDown2.Value = alarm.AlarmTime.Minutes;
            }
            catch 
            {
                checkedListBox1.Enabled = textBox1.Enabled = domainUpDown1.Enabled = numericUpDown1.Enabled = numericUpDown2.Enabled = checkBox1.Enabled = button1.Enabled = button3.Enabled = false;
            }
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void credentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CredentialEditor editor = new CredentialEditor();
            editor.ShowDialog(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WindowsBlocker blocker = new WindowsBlocker();
            blocker.Show();
        }

    }
}
