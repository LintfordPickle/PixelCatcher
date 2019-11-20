using PixelCatcher.Views;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PixelCatcher
{
    public partial class ScreenshotForm : Form
    {
        private Bitmap bitmap;
        private int windowPositionOffsetX;
        private int windowPositionOffsetY;
        private int screenshotWidth;
        private int screenshotHeight;

        public ScreenshotForm(Bitmap screenshotBitmap, int topLeftX, int topLeftY) {
            this.bitmap = screenshotBitmap;

            InitializeComponent();

            setupBitmap(topLeftX, topLeftY);
        }

        private void setupBitmap(int topLeftX, int topLeftY) {
            WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.None;

            int screenLeft = topLeftX;
            int screenTop = topLeftY;
            screenshotWidth = bitmap.Width;
            screenshotHeight = bitmap.Height;

            TopMost = true;

            pictureBox.Location = new Point(1, 1);
            pictureBox.Image = bitmap;
            pictureBox.Size = new Size(screenshotWidth - 2, screenshotHeight - 2);

            BackColor = GetRandomColor();
            Size = new Size(screenshotWidth+1, screenshotHeight+1);

            // Set the initial position of the screen capture
            this.StartPosition = FormStartPosition.Manual;
            Location = new Point(screenLeft -1, screenTop-1);

            Console.WriteLine("Screenshot Form:");
            Console.WriteLine($"       Form Size {Size.Width},{Size.Height}");
            Console.WriteLine($" PictureBox Size {pictureBox.Width},{pictureBox.Height}");
            Console.WriteLine($"");
            Console.WriteLine($"");

        }

        private Color GetRandomColor()
        {
            Random rnd = new Random();
            return Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                // Mark the start position
                windowPositionOffsetX = e.X;
                windowPositionOffsetY = e.Y;
            }
            Console.WriteLine($"       Form Size {Size.Width},{Size.Height}");
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

        private void copyToClipboardToolStripMenuItem_Click(object sender, System.EventArgs e) {
            Clipboard.SetImage(bitmap);
        }

        private void saveToFileToolStripMenuItem_Click(object sender, System.EventArgs e) {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = $"Screenshot_{DateTime.Now.ToString("yyyyMMdd_hhmmfff")}";
            saveFileDialog1.Filter = "Png Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save to Image File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "") {
                ImageFormat imageFormat = ImageFormat.Jpeg;

                // Saves the Image via a FileStream created by the OpenFile method.
                var filepath = saveFileDialog1.FileName;

                switch (saveFileDialog1.FilterIndex) {
                    case 1:
                        imageFormat = ImageFormat.Png;
                        break;
                    case 2:
                        imageFormat = ImageFormat.Jpeg;
                        break;
                    case 3:
                        imageFormat = ImageFormat.Bmp;
                        break;
                    case 4:
                        imageFormat = ImageFormat.Gif;
                        break;
                }

                BitmapService.SaveBitmapToFile(bitmap, filepath, imageFormat);
            }
        }

        private void ScreenshotForm_KeyDown(object sender, KeyEventArgs e) {

        }

        private void ScreenshotForm_Shown(object sender, EventArgs e)
        {
            Size = new Size(screenshotWidth, screenshotHeight);
        }
    }
}
