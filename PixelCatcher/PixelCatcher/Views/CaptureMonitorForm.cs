using System;
using System.Drawing;
using System.Windows.Forms;
using PixelCatcher.Presenters;

namespace PixelCatcher.Views {
    public partial class CaptureMonitorForm : Form, ICaptureMonitorView {

        private ICaptureDesktopPresenter desktopPresenter;

        public void AddPresenter(ICaptureDesktopPresenter desktopPresenter) {
            this.desktopPresenter = desktopPresenter;
        }

        public CaptureMonitorForm() {
            InitializeComponent();
        }

        private void CaptureMonitorForm_Load(object sender, EventArgs e) {
            desktopPresenter.CreateFullCaptureAreaShot();

            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Setup this form so it covers the entire desktop area, spanning all monitors
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;

            Size = new Size(screenWidth, screenHeight);
            Location = new Point(screenLeft, screenTop);

            // Update the pixture box
            pictureBox.Size = new Size(screenWidth, screenHeight);
            pictureBox.Location = new Point(0, 0);
            pictureBox.Image = desktopPresenter.GetDesktopDarkBitmap();
            pictureBox.Invalidate();

            this.Cursor = Cursors.Cross;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                desktopPresenter.SetStartClickPosition(e.X, e.Y);
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                desktopPresenter.SetStartClickPosition(e.X, e.Y);
            }
            desktopPresenter.SetStopClickPosition(e.X, e.Y);
            pictureBox.Invalidate();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e) {
            this.Cursor = Cursors.Default;
            desktopPresenter.SetStopClickPosition(e.X, e.Y);
            desktopPresenter.CreateScreenshot();
        }

        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e) {
            Close();
        }

        private void CaptureMonitorForm_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Escape) {
                Close();
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e) {
            PaintCaptureAreaInfo(e);
        }

        private void PaintCaptureAreaInfo(PaintEventArgs e) {
            PaintCaptureAreaBoundary(e);

            PaintCaptureArea(e);

            var startClickPoint = desktopPresenter.GetStartClickPoint();
            var stopClickPoint = desktopPresenter.GetEndClickPoint();
            var topLeftPoint = desktopPresenter.GetTopLeftPoint();
            var bottomRightPoint = desktopPresenter.GetBottomRightPoint();

            // Create font and brush.
            Font drawFont = new Font("Courier New", 10);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            StringFormat drawFormat = new StringFormat();

            var startCoordString = $"{topLeftPoint.X},{topLeftPoint.Y}";
            var endCoordString = $"{bottomRightPoint.X},{bottomRightPoint.Y}";

            var startStringSize = e.Graphics.MeasureString(startCoordString, drawFont);
            var endStringSize = e.Graphics.MeasureString(endCoordString, drawFont);

            float xPos = topLeftPoint.X;
            float yPos = (topLeftPoint.Y < bottomRightPoint.Y) ? topLeftPoint.Y - startStringSize.Height : topLeftPoint.Y - startStringSize.Height;

            if (startClickPoint.X < stopClickPoint.X)
                e.Graphics.DrawString(startCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);
            else
                e.Graphics.DrawString(endCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);

            if (!desktopPresenter.GetIsSelectedRectangleValid()) {
                var pixelColorStringRGB = desktopPresenter.GetPixelColorRGB();
                var pixelColorStringHex = desktopPresenter.GetPixelColorHex();
                
                e.Graphics.DrawString(pixelColorStringRGB, drawFont, drawBrush, xPos, yPos - startStringSize.Height, drawFormat);
                e.Graphics.DrawString(pixelColorStringHex, drawFont, drawBrush, xPos, yPos - startStringSize.Height*2, drawFormat);

                return;
            }

            int screenshotWidth = desktopPresenter.GetScreenshotWidth();
            int screenshotHeight = desktopPresenter.GetScreenshotHeight();
            string screenshotWidthMsg = $"{screenshotWidth} px";
            string screenshotHeightMsg = $"{screenshotHeight} px";

            xPos = bottomRightPoint.X - endStringSize.Width;
            yPos = (startClickPoint.Y < stopClickPoint.Y) ? stopClickPoint.Y : bottomRightPoint.Y;

            if (startClickPoint.X < stopClickPoint.X)
                e.Graphics.DrawString(endCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);
            else
                e.Graphics.DrawString(startCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);

            xPos = bottomRightPoint.X - screenshotWidth;
            yPos = (startClickPoint.Y < stopClickPoint.Y) ? stopClickPoint.Y : bottomRightPoint.Y;
            e.Graphics.DrawString(screenshotWidthMsg, drawFont, drawBrush, xPos, yPos, drawFormat);

            var heightMsgSize = e.Graphics.MeasureString(screenshotHeightMsg, drawFont);
            xPos = bottomRightPoint.X;
            yPos -= screenshotHeight;
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            e.Graphics.DrawString(screenshotHeightMsg, drawFont, drawBrush, xPos, yPos, drawFormat);

        }

        private void PaintCaptureAreaBoundary(PaintEventArgs e) {
            if (!desktopPresenter.GetIsSelectedRectangleValid()) {
                return;
            }

            // Draw rectangle to screen.
            Pen pen = new Pen(Color.Red, 2);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            e.Graphics.DrawRectangle(pen, desktopPresenter.GetSelectedCaptureArea());
        }

        private void PaintCaptureArea(PaintEventArgs e) {
            if(!desktopPresenter.GetIsSelectedRectangleValid()) {
                return;
            }

            var topLeftPoint = desktopPresenter.GetTopLeftPoint();

            // Get the selected rectangle area from the original (bright) bitmap
            Bitmap adjustedBitmap = BitmapService.CropBitmap(desktopPresenter.GetDesktopBitmap(), desktopPresenter.GetSelectedCaptureArea());
            var originalBitmapWidth = desktopPresenter.GetDesktopBitmap().Width;
            var originalBitmapHeight = desktopPresenter.GetDesktopBitmap().Height;

            // if no cropping occured, default to the darker capture
            if (adjustedBitmap.Width == originalBitmapWidth && adjustedBitmap.Height == originalBitmapHeight) {
                adjustedBitmap = desktopPresenter.GetDesktopDarkBitmap();
            }

            e.Graphics.DrawImage(adjustedBitmap, topLeftPoint.X, topLeftPoint.Y);
        }

    }
}
