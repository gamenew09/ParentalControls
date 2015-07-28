using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParentalControls.GUI.Forms
{
    public partial class WindowsBlocker : Form
    {
        public WindowsBlocker()
        {
            InitializeComponent();
            foreach (Control c in Controls)
            {
                c.BackColor = Color.Transparent;
            }
            foreach (Control c in panel1.Controls)
            {
                c.BackColor = Color.Transparent;
            }
        }

        private void WindowsBlocker_Load(object sender, EventArgs e)
        {
            screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics sg = Graphics.FromImage(screenshot);

            sg.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);

            //this.BackColor = Color.LimeGreen;
            //this.TransparencyKey = Color.LimeGreen;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.TopMost = true;
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            panel1.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - (panel1.Size.Width / 2), Screen.PrimaryScreen.Bounds.Height / 2 - (panel1.Size.Height / 2));
        }

        Bitmap screenshot = null;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Transparent);
            g.DrawImage(screenshot, 0, 0);
            Color c = Color.FromArgb(127, Color.Black);
            using (Brush b = new SolidBrush(c))
                g.FillRectangle(b, e.ClipRectangle);
            //base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
