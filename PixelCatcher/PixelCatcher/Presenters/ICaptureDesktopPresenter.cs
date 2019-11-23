using System.Drawing;

namespace PixelCatcher.Presenters {
    public interface ICaptureDesktopPresenter {
        void SetStartClickPosition(int x, int y);
        void SetStopClickPosition(int x, int y);
        Point GetStartClickPoint();
        Point GetEndClickPoint();
        Point GetTopLeftPoint();
        Point GetBottomRightPoint();
        Rectangle GetSelectedCaptureArea();
        int GetScreenshotWidth();
        int GetScreenshotHeight();
        bool GetIsSelectedRectangleValid();
        void CreateNewScreenshot();
        void CreateFullCaptureAreaShot();
        Bitmap GetDesktopBitmap();
        void Close();
    }
}
