using PixelCatcher.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher {
    public partial class ScreenshotView : Form, IScreenshotView {
        private Bitmap screenshotBitmap;
        private int windowPositionOffsetX;
        private int windowPositionOffsetY;
        private int screenshotWidth;
        private int screenshotHeight;

        public event EventHandler CopyScreenshotToClipboard;
        public event EventHandler SaveScreenshotToFile;

        public ScreenshotView() {
            InitializeComponent();
        }

        public void SetBitmap(Bitmap screenshotBitmap, Point topLeft) {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;

            this.screenshotBitmap = screenshotBitmap;

            int screenLeft = topLeft.X;
            int screenTop = topLeft.Y;
            screenshotWidth = this.screenshotBitmap.Width;
            screenshotHeight = this.screenshotBitmap.Height;

            // TODO: Add option to support screenshot border sizes and colors
            int borderSize = 2;
            pictureBox.Location = new Point(borderSize, borderSize);
            pictureBox.Image = this.screenshotBitmap;
            pictureBox.Size = new Size(screenshotWidth - borderSize*2, screenshotHeight - borderSize*2);

            BackColor = GetRandomColor();
            Size = new Size(screenshotWidth + borderSize*2, screenshotHeight + borderSize*2);

            // Set the initial position of the screen capture
            StartPosition = FormStartPosition.Manual;
            Location = new Point(screenLeft - 1, screenTop - 1);
        }

        private Color GetRandomColor() {
            Random rnd = new Random();
            return Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                // Mark the start position
                windowPositionOffsetX = e.X;
                windowPositionOffsetY = e.Y;
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                int newWindowPositionX = Cursor.Position.X;
                int newWindowPositionY = Cursor.Position.Y;

                // Update the new location of the window
                Location = new Point(newWindowPositionX - windowPositionOffsetX, newWindowPositionY - windowPositionOffsetY);
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e) {

        }

        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e) {
            Close();
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e) {

            CopyScreenshotToClipboard?.Invoke(this, e);
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveScreenshotToFile?.Invoke(this, e);
        }

        private void ScreenshotForm_Shown(object sender, EventArgs e) {
            Size = new Size(screenshotWidth, screenshotHeight);
        }
    }
}
