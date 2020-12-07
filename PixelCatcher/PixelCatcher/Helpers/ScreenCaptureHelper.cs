using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PixelCatcher {
    class ScreenCaptureService {
        public static Bitmap FullScreenshotAsBitmap() {
            var bounds = SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);

            var leftMostScreen = Screen.AllScreens.OrderBy(s => s.Bounds.Left).First();
            var leftMostLocation = leftMostScreen.Bounds.Location;

            using (Graphics g = Graphics.FromImage(bitmap)) {
                g.CopyFromScreen(new Point(leftMostScreen.Bounds.Left, leftMostScreen.Bounds.Top), Point.Empty, bounds.Size);
            }

            return bitmap;

        }

    }
}
