using System;
using System.Drawing;

namespace PixelCatcher.Views {
    public interface IScreenshotView {

        event EventHandler CopyScreenshotToClipboard;
        event EventHandler SaveScreenshotToFile;

        void SetBitmap(Bitmap screenshotBitmap, Point topLeft);
        void Show();
        void Close();
    }
}
