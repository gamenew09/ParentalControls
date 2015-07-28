using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ParentalControls.Common;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ParentalControls.GUI.Forms
{
    public partial class CredentialEditor : Form
    {

        ParentalControlsCredentialsFile file;

        public CredentialEditor()
        {
            InitializeComponent();

            Console.WriteLine("Path: {0}", ParentalControlsRegistry.GetRegistryKey(true).GetValue("Path").ToString() + @"\" + MainForm.CREDENTIALS_FILE);

            file = ParentalControlsCredentialsFile.Load(ParentalControlsRegistry.GetRegistryKey(true).GetValue("Path").ToString() + @"\" + MainForm.CREDENTIALS_FILE);

            foreach (ParentalControlsCredential cred in file.ParentalControlsCredentials)
            {
                comboBox1.Items.Add(cred);
            }

            foreach (Control c in groupBox1.Controls)
            {
                c.Enabled = false;
            }
        }

        protected void RefreshItems(bool disableControls)
        {
            comboBox1.Items.Clear();
            foreach (ParentalControlsCredential cred in file.ParentalControlsCredentials)
            {
                comboBox1.Items.Add(cred);
            }
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                foreach (Control c in groupBox1.Controls)
                {
                    c.Enabled = true;
                }

                textBox1.Text = ((ParentalControlsCredential)comboBox1.SelectedItem).Username;

            }
            else
            {
                foreach (Control c in groupBox1.Controls)
                {
                    c.Enabled = false;
                }
            }
            button3.Enabled = (file.ParentalControlsCredentials.Count > 1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                ParentalControlsCredential cred = new ParentalControlsCredential(textBox1.Text, textBox2.Text, false);
                if(!file.Remove((ParentalControlsCredential)comboBox1.SelectedItem))
                {
                    TaskDialog dialog = new TaskDialog();

                    dialog.Caption = this.Text;
                    dialog.InstructionText = "Failed to Change Credential";
                    dialog.Text = "An unknown error occured while changing \""+((ParentalControlsCredential)comboBox1.SelectedItem).Username+"\"";
                    dialog.Opened += (a, b) =>
                    {
                        dialog.Icon = TaskDialogStandardIcon.Error;
                    };

                    dialog.Show();
                }
                else
                {
                    file.Add(cred);
                    RefreshItems(false);
                    comboBox1.SelectedItem = cred;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string username = ((ParentalControlsCredential)comboBox1.SelectedItem).Username;
            if (file.ParentalControlsCredentials.Count > 1 && comboBox1.SelectedItem != null)
            {
                if (file.Remove((ParentalControlsCredential)comboBox1.SelectedItem))
                {
                    TaskDialog dialog = new TaskDialog();

                    dialog.Caption = this.Text;
                    dialog.InstructionText = "Removed Credentials";
                    dialog.Text = "Removed \"" + username + "\" from Credential List.";
                    dialog.Opened += (a, b) =>
                    {
                        dialog.Icon = TaskDialogStandardIcon.Information;
                    };

                    RefreshItems(false);
                    comboBox1.SelectedItem = null;
                    comboBox1_SelectedIndexChanged(comboBox1, new EventArgs());

                    dialog.Show();
                }
            }
        }

        private void CredentialEditor_Load(object sender, EventArgs e)
        {

        }

        private void CredentialEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            file.Save();
        }
    }
}
