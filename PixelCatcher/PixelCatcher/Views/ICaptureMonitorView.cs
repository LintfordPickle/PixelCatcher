using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public interface ICaptureMonitorView {

        event MouseEventHandler CaptureMouseDown;
        event MouseEventHandler CaptureMouseMove;
        event MouseEventHandler CaptureMouseUp;

        Point StartClickPoint { get; set; }
        Point StopClickPoint { get; set; }
        Color pixelUnderCursorColor { get; set; }

        void SetDesktopCaptureBitmap(Bitmap originalDesktopBitmap, Bitmap darkDesktopBitmap, Rectangle desktopArea);

        void SetSelectedAreaRectangle(Rectangle selectedAreaRectangle);

        void Close();
    }
}
