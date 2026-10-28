using System;
using System.Drawing;

namespace PixelCatcher.Views {
    public interface IScreenshotView {

        event EventHandler CopyScreenshotToClipboard;
        event EventHandler SaveScreenshotToFile;
        event EventHandler TitleChanged;

        string Title { get; set; }
        Bitmap GetExportBitmap();

        void SetBitmap(Bitmap screenshotBitmap, Point topLeft);
        void Show();
        void Close();
    }
}
