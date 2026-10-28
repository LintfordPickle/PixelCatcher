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

        // Cached GDI drawing objects to eliminate handle leaks and allocations during paint
        private readonly SolidBrush brushPanelBackground = new SolidBrush(Color.FromArgb(47, 54, 64));
        private readonly SolidBrush drawBrushBlack = new SolidBrush(Color.Black);
        private readonly SolidBrush drawBrushWhite = new SolidBrush(Color.White);
        private readonly Font drawFont = new Font("Courier New", 10);
        private readonly StringFormat drawFormat = new StringFormat();
        private readonly Pen boundaryPen = new Pen(Color.Red, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };

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

            // Update the picture box to show the darker desktop bitmap
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

            PaintCaptureArea(e);
            PaintCaptureAreaBoundary(e);
        }

        /// <summary>
        /// Checks if any of the given rectangle's dimensions are 0, which we define as an empty rectangle.
        /// </summary>
        private bool isRectangleEmpty(Rectangle rect) {
            return rect.Width <= 0 || rect.Height <= 0;
        }

        public string GetPixelColorRGB() {
            return $"R:{pixelUnderCursorColor.R:D3} G:{pixelUnderCursorColor.G:D3} B:{pixelUnderCursorColor.B:D3}";
        }

        public string GetPixelColorHex() {
            return $"#{pixelUnderCursorColor.R:X2}{pixelUnderCursorColor.G:X2}{pixelUnderCursorColor.B:X2}";
        }

        private void PaintCaptureAreaBoundary(PaintEventArgs e) {
            e.Graphics.DrawRectangle(boundaryPen, SelectedArea);
        }

        private void PaintCaptureArea(PaintEventArgs e) {
            if (originalDesktopCaptureBitmap == null || isRectangleEmpty(SelectedArea)) return;

            // Zero-allocation: draw the un-dimmed region directly from originalDesktopCaptureBitmap
            e.Graphics.DrawImage(
                originalDesktopCaptureBitmap,
                SelectedArea,
                SelectedArea,
                GraphicsUnit.Pixel);
        }

        private void PaintPreviewImage(PaintEventArgs e) {
            // Draw background panel for the info box
            e.Graphics.FillRectangle(brushPanelBackground, new Rectangle(3, 3, 350, 68));

            var pixelColorStringRGB = GetPixelColorRGB();
            var pixelColorStringHex = GetPixelColorHex();
            var rectangleAreaInfo = $"{SelectedArea.X} {SelectedArea.Y} {SelectedArea.Width} {SelectedArea.Height}";

            var fontHeight = drawFont.GetHeight();
            var xPos = 5 + previewSize.Width + 5f;
            var yPos = 5f;

            // Text shadows (black) and foreground (white)
            e.Graphics.DrawString(pixelColorStringRGB, drawFont, drawBrushBlack, xPos - 2, yPos + 2, drawFormat);
            e.Graphics.DrawString(pixelColorStringRGB, drawFont, drawBrushWhite, xPos, yPos, drawFormat);

            e.Graphics.DrawString(pixelColorStringHex, drawFont, drawBrushBlack, xPos - 2, yPos + fontHeight + 2, drawFormat);
            e.Graphics.DrawString(pixelColorStringHex, drawFont, drawBrushWhite, xPos, yPos + fontHeight, drawFormat);

            e.Graphics.DrawString(rectangleAreaInfo, drawFont, drawBrushBlack, xPos - 2, yPos + fontHeight * 2 + 2, drawFormat);
            e.Graphics.DrawString(rectangleAreaInfo, drawFont, drawBrushWhite, xPos, yPos + fontHeight * 2, drawFormat);

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
            if (originalDesktopCaptureBitmap == null) return;

            var previewWidth = (int)(previewSize.Width / previewScale);
            var previewHeight = (int)(previewSize.Height / previewScale);
            var lPreviewAreaRectangle = new Rectangle(
                StopClickPoint.X - previewWidth / 2,
                StopClickPoint.Y - previewHeight / 2,
                previewWidth,
                previewHeight);

            var oldPreview = previewBitmap;
            previewBitmap = BitmapService.CropBitmap(originalDesktopCaptureBitmap, lPreviewAreaRectangle);
            oldPreview?.Dispose();

            if (previewBitmap != null && previewBitmap.Width > 0 && previewBitmap.Height > 0) {
                int centerDotX = (int)(previewWidth * 0.5f);
                int centerDotY = (int)(previewHeight * 0.5f);
                if (centerDotX >= 0 && centerDotX < previewBitmap.Width && centerDotY >= 0 && centerDotY < previewBitmap.Height) {
                    previewBitmap.SetPixel(centerDotX, centerDotY, Color.Red);
                }
            }
        }

        public void SetPreviewSize(Size previewSize) {
            this.previewSize = previewSize;
        }

        public void SetPreviewMagnification(float previewScale) {
            this.previewScale = previewScale;
        }

        protected override void OnFormClosed(FormClosedEventArgs e) {
            base.OnFormClosed(e);

            pictureBox.Image = null;

            darkDesktopCaptureBitmap?.Dispose();
            darkDesktopCaptureBitmap = null;

            previewBitmap?.Dispose();
            previewBitmap = null;
        }

        // Clean up cached GDI drawing resources when the form is disposed
        private void DisposeGdiObjects() {
            brushPanelBackground.Dispose();
            drawBrushBlack.Dispose();
            drawBrushWhite.Dispose();
            drawFont.Dispose();
            drawFormat.Dispose();
            boundaryPen.Dispose();
            previewBitmap?.Dispose();
            darkDesktopCaptureBitmap?.Dispose();
        }
    }
}
