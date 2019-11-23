using PixelCatcher.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCatcher.Presenters {
    class CaptureDesktopPresenter : ICaptureDesktopPresenter {
        private ICaptureMonitorView captureMonitorView { get; set; }
        public int StartClickX { get; set; }
        public int StartClickY { get; set; }
        public int StopClickX { get; set; }
        public int StopClickY { get; set; }
        public Color PixelColorUnderMouse { get; set; }
        public Bitmap OriginalScreenBitmap { get; private set; }
        public Bitmap DarkerScreenBitmap { get; private set; }
        public Rectangle CaptureRectangle { get; private set; }

        public CaptureDesktopPresenter(ICaptureMonitorView captureMonitorView) {
            this.captureMonitorView = captureMonitorView;
        }

        public Rectangle GetSelectedCaptureArea() {
            var x = Math.Min(StartClickX, StopClickX);
            var y = Math.Min(StartClickY, StopClickY);
            int width = Math.Abs(StopClickX - StartClickX);
            int height = Math.Abs(StopClickY - StartClickY);

            return new Rectangle(x, y, width, height);
        }

        public void CreateScreenshot() {
            if (!GetIsSelectedRectangleValid()) {
                Close();
                return;
            }

            // Only get the rectangle portion of the dekstop we want
            Bitmap croppedBitmap = BitmapService.CropBitmap(OriginalScreenBitmap, GetSelectedCaptureArea());

            // When creating the new Screenshot form, make sure to place it on the relevant part of the desktop
            var topLeftPoint = GetTopLeftPoint();

            // TODO: Add Screenshots to groups (for grouped moving etc.)
            // Create a new ScreenshotForm and display the jazz
            ScreenshotForm newScreenshotForm = new ScreenshotForm(croppedBitmap, topLeftPoint.X, topLeftPoint.Y);
            newScreenshotForm.Show();

            Close();
        }

        public void Close() {
            captureMonitorView.Close();
        }

        public void SetStartClickPosition(int x, int y) {
            StartClickX = x;
            StartClickY = y;
            UpdatePixelColorUnderMouse(StartClickX, StartClickY);
        }

        private void UpdatePixelColorUnderMouse(int x, int y) {
            PixelColorUnderMouse = OriginalScreenBitmap.GetPixel(x, y);
        }

        public void SetStopClickPosition(int x, int y) {
            StopClickX = x;
            StopClickY = y;
        }

        public Point GetTopLeftPoint() {
            return new Point(Math.Min(StartClickX, StopClickX), Math.Min(StartClickY, StopClickY));
        }

        public Point GetBottomRightPoint() {
            return new Point(Math.Max(StartClickX, StopClickX), Math.Max(StartClickY, StopClickY));
        }

        public bool GetIsSelectedRectangleValid() {
            return StartClickX != StopClickX && StartClickY != StopClickY;
        }

        public int GetScreenshotWidth() {
            return Math.Abs(StartClickX - StopClickX);
        }

        public int GetScreenshotHeight() {
            return Math.Abs(StartClickY - StopClickY);
        }

        public void CreateFullCaptureAreaShot() {
            //  First capture a screenshot before the creation of our own window
            OriginalScreenBitmap = ScreenCaptureService.FullScreenshotAsBitmap();

            // Get a darker version of the bitmap to display
            DarkerScreenBitmap = BitmapService.ModifyBitmap(OriginalScreenBitmap, 0.9f, 0.9f, 1.0f);
        }

        public Bitmap GetDesktopDarkBitmap() {
            return DarkerScreenBitmap;
        }

        public Bitmap GetDesktopBitmap() {
            return OriginalScreenBitmap;
        }

        public Point GetStartClickPoint() {
            return new Point(StartClickX, StartClickY);
        }

        public Point GetEndClickPoint() {
            return new Point(StopClickX, StopClickY);
        }

        public string GetPixelColorRGB() {
            return PixelColorUnderMouse.ToString();
        }

        public string GetPixelColorHex() {
            return PixelColorUnderMouse.R.ToString("X2") + PixelColorUnderMouse.G.ToString("X2") + PixelColorUnderMouse.B.ToString("X2");
        }

        public string GetPixelColorCMYK() {
            throw new NotImplementedException();
        }
    }
}
