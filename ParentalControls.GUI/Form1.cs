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

namespace ParentalControls.GUI
{
    public partial class Form1 : Form
    {

        public const string ALARMS_FILE = "alarms.bas";
        public const string CREDENTIALS_FILE = "creds.crd";

        AlarmsFile file;
        ParentalControlsCredentialsFile file2;

        public Form1()
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

                NetworkCredential cred = null;
                WindowsSecurity.GetCredentialsVistaAndUp("Please enter new credentials for Parental Controls", "Set username and password for stopping an alarm.", out cred);
                while (cred == null || (string.IsNullOrWhiteSpace(cred.UserName) || string.IsNullOrWhiteSpace(cred.Password)))
                {
                    WindowsSecurity.GetCredentialsVistaAndUp("Please enter new credentials for Parental Controls", "Set username and password for stopping an alarm. (Credentials must not be empty)", out cred);
                }
                file2.Add((ParentalControlsCredential)cred);
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

                    bool reallyvalid = true;

                    Console.WriteLine(file2.Validate((ParentalControlsCredential)cred));

                    while (cred == null || !file2.Validate((ParentalControlsCredential)cred) || (string.IsNullOrWhiteSpace(cred.UserName) || string.IsNullOrWhiteSpace(cred.Password)))
                    {
                        TaskDialog ddialog = new TaskDialog();

                        ddialog.InstructionText = "Credentials Invalid";
                        ddialog.Text = "You want to stop testing credentials?";

                        ddialog.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;

                        Console.WriteLine("test123");

                        if (ddialog.Show() == TaskDialogResult.Yes)
                        {
                            reallyvalid = false;
                            break;
                        }
                        else
                        {
                            WindowsSecurity.GetCredentialsVistaAndUp("Please enter credentials for \"Parental Controls\"", "Force disabling \"TestAlarm\"", out cred);
                        }
                    }

                    TaskDialog dadialog = new TaskDialog();

                    dadialog.InstructionText = "Credentials Valid!";
                    dadialog.Text = "You are now done setting up Parental Controls!";

                    dadialog.StandardButtons = TaskDialogStandardButtons.Ok;
                    dadialog.Show();
                };

                TaskDialogCommandLink linkb = new TaskDialogCommandLink("linkB", "Skip Test");

                linkb.Click += (ba, bb) =>
                {
                    TaskDialogButton tb = (TaskDialogButton)ba;
                    ((TaskDialog)tb.HostingDialog).Close(TaskDialogResult.No);
                };

                ndialog.Controls.Add(linka);
                ndialog.Controls.Add(linkb);

                ndialog.Show();
                file2.Add(cred.UserName, cred.Password);
                file2.Save();
            };
            dialog.Controls.Add(button);
            
            dialog.Show();
            Enable(true);
        }

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
                Console.WriteLine(ex);
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

    }
}
