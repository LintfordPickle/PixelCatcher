using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher {
    class ScreenCaptureService {
        public static Bitmap FullScreenshotAsBitmap() {
            var bounds = SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }

            return bitmap;

        }

    }
}
