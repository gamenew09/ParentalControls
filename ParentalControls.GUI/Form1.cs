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

        AlarmsFile file;

        public Form1()
        {
            InitializeComponent();

            file = AlarmsFile.Load(ALARMS_FILE);
        }

        public void FirstInstall()
        {
            RegistryKey reg = ParentalControlsRegistry.GetRegistryKey();
            reg.SetValue("Path", Application.StartupPath, RegistryValueKind.String);
            reg.SetValue("AlarmFile", Application.StartupPath + @"\" + ALARMS_FILE);

            TaskDialog dialog = new TaskDialog();

            dialog.Caption = Application.ProductName+" Setup";
            dialog.InstructionText = Application.ProductName+" is mostly setup!";
            dialog.Text = "What you need to do is setup a password for any cases that you need to force close an alarm.";

            TaskDialogButton button = new TaskDialogButton("btnOne", "Continue");
            button.Click += (ab, bb) =>
            {
                TaskDialogButton tdb = (TaskDialogButton)ab;
                ((TaskDialog)tdb.HostingDialog).Close(TaskDialogResult.Ok);

                NetworkCredential cred = null;
                WindowsSecurity.GetCredentialsVistaAndUp("Please enter new credentials for Parental Controls", "Set username and password for stopping an alarm.", out cred);
                while (cred == null || (string.IsNullOrWhiteSpace(cred.UserName) || string.IsNullOrWhiteSpace(cred.Password)))
                {
                    WindowsSecurity.GetCredentialsVistaAndUp("Please enter new credentials for Parental Controls", "Set username and password for stopping an alarm. (Credentials must not be empty)", out cred);
                }
                TaskDialog ndialog = new TaskDialog();
                ndialog.Caption = Application.ProductName + " Setup";
                ndialog.InstructionText = "Setup complete!";
                ndialog.Text = "Fun Fact: You can create as many accounts as you want!";

                ndialog.Show();
            };
            dialog.Controls.Add(button);
            
            dialog.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if ((bool)Properties.Settings.Default["FirstTimeStartup"] == false)
            {
                Thread t = new Thread(FirstInstall);
                t.Start();
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
