using PixelCatcher.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class ScreenshotView : Form, IScreenshotView {
        private Bitmap screenshotBitmap;
        private int windowPositionOffsetX;
        private int windowPositionOffsetY;
        private int screenshotWidth;
        private int screenshotHeight;

        public int BorderSize { get; set; }

        public event EventHandler CopyScreenshotToClipboard;
        public event EventHandler SaveScreenshotToFile;

        public ScreenshotView() {
            InitializeComponent();
            BorderSize = 1;
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

            pictureBox.Location = new Point(BorderSize, BorderSize);
            pictureBox.Image = this.screenshotBitmap;
            pictureBox.Size = new Size(screenshotWidth, screenshotHeight);

            BackColor = GetRandomColor();
            ClientSize = new Size(screenshotWidth + BorderSize * 2, screenshotHeight + BorderSize * 2);

            // Set initial position so the cropped image aligns exactly over the captured screen area
            StartPosition = FormStartPosition.Manual;
            Location = new Point(screenLeft - BorderSize, screenTop - BorderSize);
        }

        private Color GetRandomColor() {
            Random rnd = new Random();
            return Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                windowPositionOffsetX = e.X;
                windowPositionOffsetY = e.Y;
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                int newWindowPositionX = Cursor.Position.X;
                int newWindowPositionY = Cursor.Position.Y;

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
            // Keep the exact border size
            ClientSize = new Size(screenshotWidth + BorderSize * 2, screenshotHeight + BorderSize * 2);
        }

        protected override void OnFormClosed(FormClosedEventArgs e) {
            base.OnFormClosed(e);
            pictureBox.Image = null;
            screenshotBitmap?.Dispose();
            screenshotBitmap = null;
        }
    }
}
