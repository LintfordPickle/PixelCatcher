using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class CaptureDesktopFullView : Form, ICaptureDesktopView, IPreviewView {

        public event MouseEventHandler CaptureMouseDown;
        public event MouseEventHandler CaptureMouseMove;
        public event MouseEventHandler CaptureMouseUp;

        public Point StartClickPoint { get; set; }
        public Point StopClickPoint { get; set; }
        public Bitmap originalDesktopCaptureBitmap { get; private set; }
        public Bitmap darkDesktopCaptureBitmap { get; private set; }
        public Rectangle SelectedArea { get; private set; }
        public Color pixelUnderCursorColor { get; set; }

        public Bitmap previewBitmap { get; private set; }
        public Size previewSize { get; private set; }
        public float previewScale { get; private set; }

        public CaptureDesktopFullView() {
            InitializeComponent();

            previewSize = new Size(64, 64);
            previewScale = 3;
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

            Cursor = Cursors.Cross;
        }

        public void SetSelectedAreaRectangle(Rectangle selectedAreaRectangle) {
            SelectedArea = selectedAreaRectangle;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            CaptureMouseDown?.Invoke(this, e);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            CaptureMouseMove?.Invoke(this, e);
            UpdatePreviewBitmap();
            pictureBox.Invalidate();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e) {
            CaptureMouseUp?.Invoke(this, e);
            Cursor = Cursors.Default;

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
            PaintPreviewImage(e);

            if (isRectangleEmpty(SelectedArea)) {
                return;
            }

            PaintCaptureAreaBoundary(e);
            PaintCaptureArea(e);

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

        private void PaintPreviewImage(PaintEventArgs e) {
            var brushPanelBackground = new SolidBrush(Color.FromArgb(47, 54, 64));
            var drawBrushBlack = new SolidBrush(Color.Black);
            var drawBrushWhite = new SolidBrush(Color.White);

            // Draw a background panel for the info box
            e.Graphics.FillRectangle(brushPanelBackground, new Rectangle(3, 3, 350, 68));

            // Create font and brush.
            var drawFont = new Font("Courier New", 10);
            var drawFormat = new StringFormat();

            var pixelColorStringRGB = GetPixelColorRGB();
            var pixelColorStringHex = GetPixelColorHex();
            var rectangleAreaInfo = $"{SelectedArea.X} {SelectedArea.Y} {SelectedArea.Width} {SelectedArea.Height}";

            var fontHeight = drawFont.GetHeight();
            var xPos = 5 + previewSize.Width + 5f;
            var yPos = 5;

            e.Graphics.DrawString(pixelColorStringRGB, drawFont, drawBrushBlack, xPos - 2, yPos + 2, drawFormat);
            e.Graphics.DrawString(pixelColorStringRGB, drawFont, drawBrushWhite, xPos, yPos, drawFormat);

            e.Graphics.DrawString(pixelColorStringHex, drawFont, drawBrushBlack, xPos - 2, yPos + fontHeight + 2, drawFormat);
            e.Graphics.DrawString(pixelColorStringHex, drawFont, drawBrushWhite, xPos, yPos + fontHeight, drawFormat);

            e.Graphics.DrawString(rectangleAreaInfo, drawFont, drawBrushBlack, xPos - 2, yPos + fontHeight*2 + 2, drawFormat);
            e.Graphics.DrawString(rectangleAreaInfo, drawFont, drawBrushWhite, xPos, yPos + fontHeight*2, drawFormat);

            if (previewBitmap == null) {
                return;
            }

            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            e.Graphics.DrawImage(previewBitmap, 5, 5, previewSize.Width, previewSize.Height);

        }

        private void UpdatePreviewBitmap() {
            // The preview bitmap is always at the location of the end click (this is where the pointer currently 'is')
            var previewWidth = previewSize.Width / previewScale;
            var previewHeight = previewSize.Height / previewScale;
            var lPreviewAreaRectangle = new Rectangle((int)(StopClickPoint.X - previewWidth / 2), (int)(StopClickPoint.Y - previewHeight / 2), (int)previewWidth, (int)previewHeight);
            previewBitmap = BitmapService.CropBitmap(originalDesktopCaptureBitmap, lPreviewAreaRectangle);
            previewBitmap.SetPixel((int)(previewWidth * 0.5f)+1, (int)(previewHeight * 0.5f)+1, Color.Red);
        }

        public void SetPreviewSize(Size previewSize) {
            this.previewSize = previewSize;
        }

        public void SetPreviewMagnification(float previewScale) {
            this.previewScale = previewScale;
        }
    }
}
