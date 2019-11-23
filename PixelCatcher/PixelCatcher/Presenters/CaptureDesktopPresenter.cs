using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelCatcher.Presenters {
    class CaptureDesktopPresenter : ICaptureDesktopPresenter {
        private ICaptureDesktopView captureMonitorView { get; set; }

        public int StartClickX { get; set; }
        public int StartClickY { get; set; }
        public int StopClickX { get; set; }
        public int StopClickY { get; set; }
        public Bitmap OriginalScreenBitmap { get; private set; }
        public Rectangle CaptureRectangle { get; private set; }
        public Rectangle DesktopRectangle { get; private set; }

        public CaptureDesktopPresenter(ICaptureDesktopView captureMonitorView) {
            this.captureMonitorView = captureMonitorView;

            CreateFullCaptureAreaShot();

            captureMonitorView.SetDesktopCaptureBitmap(OriginalScreenBitmap, DesktopRectangle);

            captureMonitorView.CaptureMouseDown += CaptureMonitorView_CaptureMouseDown;
            captureMonitorView.CaptureMouseMove += CaptureMonitorView_CaptureMouseMove;
            captureMonitorView.CaptureMouseUp += CaptureMonitorView_CaptureMouseUp;

        }

        private void CaptureMonitorView_CaptureMouseUp(object sender, MouseEventArgs e) {
            SetStopClickPosition(e.X, e.Y);
            CreateNewScreenshot();
        }

        private void CaptureMonitorView_CaptureMouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                SetStartClickPosition(e.X, e.Y);
            }
        }

        private void CaptureMonitorView_CaptureMouseMove(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                SetStartClickPosition(e.X, e.Y);
            }
            SetStopClickPosition(e.X, e.Y);
        }

        public Rectangle GetSelectedCaptureArea() {
            var x = Math.Min(StartClickX, StopClickX);
            var y = Math.Min(StartClickY, StopClickY);
            int width = Math.Abs(StopClickX - StartClickX);
            int height = Math.Abs(StopClickY - StartClickY);

            return new Rectangle(x, y, width, height);
        }

        public void CreateNewScreenshot() {
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

            var screenshotModel = new ScreenshotModel(croppedBitmap, topLeftPoint);
            var newScreenshotForm = new ScreenshotView();

            var screenshotPresenter = new ScreenshotPresenter(newScreenshotForm, screenshotModel);

            newScreenshotForm.SetBitmap(croppedBitmap, topLeftPoint);
            newScreenshotForm.Show();

            Close();
        }

        public void Close() {
            captureMonitorView.Close();
        }

        private void UpdatePixelColorUnderMouse(int x, int y) {
            captureMonitorView.pixelUnderCursorColor = OriginalScreenBitmap.GetPixel(x, y);
        }

        public void SetStartClickPosition(int x, int y) {
            StartClickX = x;
            StartClickY = y;
            captureMonitorView.StartClickPoint = new Point(StartClickX, StartClickY);
            UpdatePixelColorUnderMouse(StartClickX, StartClickY);
        }

        public void SetStopClickPosition(int x, int y) {
            StopClickX = x;
            StopClickY = y;

            // Update the selected area
            captureMonitorView.StopClickPoint = new Point(StopClickX, StopClickY);
            captureMonitorView.SetSelectedAreaRectangle(GetSelectedCaptureArea());
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
            DesktopRectangle = SystemInformation.VirtualScreen;
            OriginalScreenBitmap = ScreenCaptureService.FullScreenshotAsBitmap();

            
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
    }
}
