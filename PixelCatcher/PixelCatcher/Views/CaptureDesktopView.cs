using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class CaptureDesktopView : Form, ICaptureDesktopView {

        public event MouseEventHandler CaptureMouseDown;
        public event MouseEventHandler CaptureMouseMove;
        public event MouseEventHandler CaptureMouseUp;

        public Point StartClickPoint { get; set; }
        public Point StopClickPoint { get; set; }
        public Bitmap originalDesktopCaptureBitmap { get; private set; }
        public Bitmap darkDesktopCaptureBitmap { get; private set; }
        public Rectangle SelectedArea { get; private set; }
        public Color pixelUnderCursorColor { get; set; }

        public CaptureDesktopView() {
            InitializeComponent();
        }

        public void SetDesktopCaptureBitmap(Bitmap originalDesktopBitmap, Rectangle desktopArea) {
            originalDesktopCaptureBitmap = originalDesktopBitmap;
            // Get a darker version of the bitmap to display
            darkDesktopCaptureBitmap = BitmapService.ModifyBitmap(originalDesktopCaptureBitmap, 0.9f, 0.9f, 1.0f);

            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Setup this form so it covers the entire desktop area, spanning all monitors
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;

            Size = new Size(screenWidth, screenHeight);
            Location = new Point(screenLeft, screenTop);

            // Update the pixture box to show the darker desktop bitmap
            pictureBox.Size = new Size(screenWidth, screenHeight);
            pictureBox.Location = new Point(0, 0);
            pictureBox.Image = darkDesktopCaptureBitmap;
            pictureBox.Invalidate();

            this.Cursor = Cursors.Cross;
        }

        public void SetSelectedAreaRectangle(Rectangle selectedAreaRectangle) {
            SelectedArea = selectedAreaRectangle;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            CaptureMouseDown(this, e);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            CaptureMouseMove(this, e);
            pictureBox.Invalidate();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e) {
            CaptureMouseUp(this, e);
            this.Cursor = Cursors.Default;

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

            if (isRectangleEmpty(SelectedArea)) {
                return;
            }

            PaintCaptureAreaBoundary(e);
            PaintCaptureArea(e);

        }

        private void PaintCaptureAreaInfo(PaintEventArgs e) {

            int topLeftX = Math.Min(StartClickPoint.X, StopClickPoint.X);
            int topLeftY = Math.Min(StartClickPoint.Y, StopClickPoint.Y);
            int bottomRightX = Math.Max(StartClickPoint.X, StopClickPoint.X);
            int bottomRightY = Math.Max(StartClickPoint.Y, StopClickPoint.Y);

            // Create font and brush.
            Font drawFont = new Font("Courier New", 10);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            StringFormat drawFormat = new StringFormat();

            var startCoordString = $"{topLeftX},{topLeftY}";
            var endCoordString = $"{bottomRightX},{bottomRightY}";

            var startStringSize = e.Graphics.MeasureString(startCoordString, drawFont);
            var endStringSize = e.Graphics.MeasureString(endCoordString, drawFont);

            float xPos = topLeftX;
            float yPos = (topLeftY < bottomRightY) ? topLeftY - startStringSize.Height : topLeftY - startStringSize.Height;

            if (StartClickPoint.X < StopClickPoint.X)
                e.Graphics.DrawString(startCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);
            else
                e.Graphics.DrawString(endCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);

            if (isRectangleEmpty(SelectedArea)) {
                return;
            }

            int screenshotWidth = Math.Abs(StartClickPoint.X - StopClickPoint.X);
            int screenshotHeight = Math.Abs(StartClickPoint.Y - StopClickPoint.Y);
            string screenshotWidthMsg = $"{screenshotWidth} px";
            string screenshotHeightMsg = $"{screenshotHeight} px";

            xPos = bottomRightX - endStringSize.Width;
            yPos = (StartClickPoint.Y < StopClickPoint.Y) ? StopClickPoint.Y : bottomRightY;

            if (StartClickPoint.X < StopClickPoint.X)
                e.Graphics.DrawString(endCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);
            else
                e.Graphics.DrawString(startCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);

            xPos = bottomRightX - screenshotWidth;
            yPos = (StartClickPoint.Y < StopClickPoint.Y) ? StopClickPoint.Y : bottomRightY;
            e.Graphics.DrawString(screenshotWidthMsg, drawFont, drawBrush, xPos, yPos, drawFormat);

            var heightMsgSize = e.Graphics.MeasureString(screenshotHeightMsg, drawFont);
            xPos = bottomRightX;
            yPos -= screenshotHeight;
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            e.Graphics.DrawString(screenshotHeightMsg, drawFont, drawBrush, xPos, yPos, drawFormat);

        }

        /// <summary>
        /// Checks if any of the given rectangle's dimensions are 0, which we define a an empty rectangle.
        /// </summary>
        /// <returns>True if any of the rectangle's dimensions are 0, false otherwise.</returns>
        private bool isRectangleEmpty(Rectangle rect) {
            return rect.Width == 0 || rect.Height == 0;
        }

        public string GetPixelColorRGB() {
            return $"R:{pixelUnderCursorColor.R.ToString("D3")} G:{pixelUnderCursorColor.G.ToString("D3")} B:{pixelUnderCursorColor.B.ToString("D3")}";
        }

        public string GetPixelColorHex() {
            return $"#{pixelUnderCursorColor.R.ToString("X2")}{pixelUnderCursorColor.G.ToString("X2")}{pixelUnderCursorColor.B.ToString("X2")}";
        }

        private void PaintCaptureAreaBoundary(PaintEventArgs e) {
            Pen pen = new Pen(Color.Red, 2);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            e.Graphics.DrawRectangle(pen, SelectedArea);
        }

        private void PaintCaptureArea(PaintEventArgs e) {
            // Get the selected rectangle area from the original (bright) bitmap
            Bitmap adjustedBitmap = BitmapService.CropBitmap(originalDesktopCaptureBitmap, SelectedArea);

            // if no cropping occured, default to the darker capture
            if (adjustedBitmap.Width == originalDesktopCaptureBitmap.Width && adjustedBitmap.Height == originalDesktopCaptureBitmap.Height) {
                adjustedBitmap = darkDesktopCaptureBitmap;
            }

            e.Graphics.DrawImage(adjustedBitmap, SelectedArea.X, SelectedArea.Y);
        }
    }
}
