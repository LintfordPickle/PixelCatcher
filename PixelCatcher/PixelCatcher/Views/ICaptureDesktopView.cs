using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public interface ICaptureDesktopView {

        event MouseEventHandler CaptureMouseDown;
        event MouseEventHandler CaptureMouseMove;
        event MouseEventHandler CaptureMouseUp;

        event FormClosedEventHandler FormClosed;

        bool IsDisposed { get; }

        Point StartClickPoint { get; set; }
        Point StopClickPoint { get; set; }
        Color pixelUnderCursorColor { get; set; }

        void SetDesktopCaptureBitmap(Bitmap originalDesktopBitmap, Rectangle desktopArea);

        void SetSelectedAreaRectangle(Rectangle selectedAreaRectangle);

        void BringToFront();
        void Show();
        void Close();
    }
}
