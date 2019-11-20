using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher {
    public partial class CaptureMonitorForm : Form
    {
        private int startClickX;
        private int startClickY;
        private int stopClickX;
        private int stopClickY;
        private Bitmap originalScreenBitmap;
        private Bitmap darkerScreenBitmap;
        private Rectangle captureRectangle;

        public CaptureMonitorForm()
        {
            InitializeComponent();
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startClickX = e.X;
                startClickY = e.Y;

            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                stopClickX = e.X;
                stopClickY = e.Y;

                pictureBox.Invalidate();
            } else {
                startClickX = e.X;
                startClickY = e.Y;
                stopClickX = e.X;
                stopClickY = e.Y;
                pictureBox.Invalidate();
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            stopClickX = e.X;
            stopClickY = e.Y;

            captureRectangle.Width = Math.Abs(stopClickX - startClickX);
            captureRectangle.Height = Math.Abs(stopClickY - startClickY);

            CreateCaptureImage();
        }

        Rectangle GetCaptureRectangle()
        {
            var x = Math.Min(startClickX, stopClickX);
            var y = Math.Min(startClickY, stopClickY);
            int width = Math.Abs(stopClickX - startClickX);
            int height = Math.Abs(stopClickY - startClickY);

            return new Rectangle(x, y, width, height);
        }

        void CreateCaptureImage() {
            this.Cursor = Cursors.Default;

            bool validRect = startClickX != stopClickX && startClickY != stopClickY;
            if (!validRect) {
                Close();
                return;
            }

            // Only get the rectangle portion of the dekstop we want
            Bitmap croppedBitmap = BitmapService.CropBitmap(originalScreenBitmap, GetCaptureRectangle());

            // When creating the new Screenshot form, make sure to place it on the relevant part of the desktop
            var topLeftOfScreenshotX = Math.Min(startClickX, stopClickX);
            var topLeftOfScreenshotY = Math.Min(startClickY, stopClickY);

            ScreenshotForm newScreenshotForm = new ScreenshotForm(croppedBitmap, topLeftOfScreenshotX, topLeftOfScreenshotY);
            newScreenshotForm.Show();

            Close();
        }

        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Close();
        }

        private void CaptureMonitorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Close();
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            int topLeftX = Math.Min(startClickX, stopClickX);
            int topLeftY = Math.Min(startClickY, stopClickY);

            bool validRect = startClickX != stopClickX && startClickY != stopClickY;

            // Create font and brush.
            Font drawFont = new Font("Courier New", 11, FontStyle.Bold);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            // Set format of string.
            StringFormat drawFormat = new StringFormat();

            String startCoordString = $"{startClickX},{startClickY}";
            var startStringSize = e.Graphics.MeasureString(startCoordString, drawFont);

            String endCoordString = $"{stopClickX},{stopClickY}";
            var endStringSize = e.Graphics.MeasureString(endCoordString, drawFont);

            float xPos = topLeftX;
            float yPos = (startClickY < stopClickY) ? startClickY - startStringSize.Height : topLeftY - startStringSize.Height;

            if(startClickX < stopClickX)
                e.Graphics.DrawString(startCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);
            else
                e.Graphics.DrawString(endCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);

            if (!validRect) return;

            int bottomRightX = Math.Max(startClickX, stopClickX);
            int bottomRightY = Math.Max(startClickY, stopClickY);

            int screenshotWidth = Math.Abs(startClickX - stopClickX);
            int screenshotHeight = Math.Abs(startClickY - stopClickY);
            string screenshotWidthMsg = $"{screenshotWidth} px";
            string screenshotHeightMsg = $"{screenshotHeight} px";

            // Create pen.
            Pen blackPen = new Pen(Color.Red, 2);
            blackPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            // Draw rectangle to screen.
            e.Graphics.DrawRectangle(blackPen, GetCaptureRectangle());

            // Get the selected rectangle area from the original (bright) bitmap
            Bitmap adjustedBitmap = BitmapService.CropBitmap(originalScreenBitmap, GetCaptureRectangle());

            // if no cropping occured, default to the darker capture
            if (adjustedBitmap.Width == originalScreenBitmap.Width && adjustedBitmap.Height == originalScreenBitmap.Height) {
                adjustedBitmap = darkerScreenBitmap;
            }

            e.Graphics.DrawImage(adjustedBitmap, topLeftX, topLeftY);

            // Draw string to screen.
            // Render the width of the area
            xPos = bottomRightX - endStringSize.Width;
            yPos = (startClickY < stopClickY) ? stopClickY : bottomRightY;

            if (startClickX < stopClickX)
                e.Graphics.DrawString(endCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);
            else
                e.Graphics.DrawString(startCoordString, drawFont, drawBrush, xPos, yPos, drawFormat);

            xPos = bottomRightX - screenshotWidth;
            yPos = (startClickY < stopClickY) ? stopClickY : bottomRightY;
            e.Graphics.DrawString(screenshotWidthMsg, drawFont, drawBrush, xPos, yPos, drawFormat);

            var heightMsgSize = e.Graphics.MeasureString(screenshotHeightMsg, drawFont);
            xPos = bottomRightX;
            yPos = ((startClickY < stopClickY) ? stopClickY : bottomRightY) - screenshotHeight;
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            e.Graphics.DrawString(screenshotHeightMsg, drawFont, drawBrush, xPos, yPos, drawFormat);
        }

        private void CaptureMonitorForm_Load(object sender, EventArgs e)
        {
            //  First capture a screenshot before the creation of our own window
            originalScreenBitmap = ScreenCaptureService.FullScreenshotAsBitmap();

            // Get a darker version of the bitmap to display
            darkerScreenBitmap = BitmapService.ModifyBitmap(originalScreenBitmap, 0.8f, 0.8f, 1.0f);

            // Setup this form so it covers the entire desktop area, spanning all monitors
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;

            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            Size = new Size(screenWidth, screenHeight);
            Location = new Point(screenLeft, screenTop);

            // Update the pixture box
            pictureBox.Size = new Size(screenWidth, screenHeight);
            pictureBox.Location = new Point(0, 0);
            pictureBox.Image = darkerScreenBitmap;
            pictureBox.Invalidate();

            this.Cursor = Cursors.Cross;
        }
    }
}
