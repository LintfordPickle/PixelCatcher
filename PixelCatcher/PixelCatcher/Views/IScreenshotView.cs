using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCatcher.Views {
    public interface IScreenshotView {

        event EventHandler CopyScreenshotToClipboard;
        event EventHandler SaveScreenshotToFile;

        void SetBitmap(Bitmap screenshotBitmap, Point topLeft);
        void Show();
        void Close();
    }
}
