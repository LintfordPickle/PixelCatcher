using PixelCatcher.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher {
    public partial class ScreenshotOffsetView : Form, IScreenshotView {
        private Bitmap screenshotBitmap;
        private int windowPositionOffsetX;
        private int windowPositionOffsetY;
        private int screenshotWidth;
        private int screenshotHeight;
        private Point mousePosition;

        public event EventHandler CopyScreenshotToClipboard;
        public event EventHandler SaveScreenshotToFile;

        public ScreenshotOffsetView() {
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

            pictureBox.Location = new Point(1, 1);
            pictureBox.Image = this.screenshotBitmap;
            pictureBox.Size = new Size(screenshotWidth - 2, screenshotHeight - 2);

            BackColor = GetRandomColor();
            Size = new Size(screenshotWidth + 1, screenshotHeight + 1);

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

            mousePosition = new Point(e.X, e.Y);
            pictureBox.Invalidate();

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

        private void pictureBox_Paint(object sender, PaintEventArgs e) {
            Pen pen = new Pen(Color.Red, 2);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            e.Graphics.DrawRectangle(pen, new Rectangle(-1, -1, mousePosition.X, mousePosition.Y));

            Font drawFont = new Font("Courier New", 10);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            StringFormat drawFormat = new StringFormat();

            var offsetText = $"{mousePosition.X},{mousePosition.Y}";
            var offsetTextSize = e.Graphics.MeasureString(offsetText, drawFont);

            float xPos = (mousePosition.X + offsetTextSize.Width > screenshotBitmap.Width) ? mousePosition.X - offsetTextSize.Width : mousePosition.X;
            float yPos = (mousePosition.Y < offsetTextSize.Height) ? mousePosition.Y : mousePosition.Y - offsetTextSize.Height;

            e.Graphics.DrawString(offsetText, drawFont, drawBrush, xPos, yPos, drawFormat);

        }

        private void ScreenshotForm_Shown(object sender, EventArgs e) {
            Size = new Size(screenshotWidth, screenshotHeight);
        }
    }
}
