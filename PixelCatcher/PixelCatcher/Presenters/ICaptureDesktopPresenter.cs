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
        void CreateScreenshot();
        void CreateFullCaptureAreaShot();
        Bitmap GetDesktopDarkBitmap();
        Bitmap GetDesktopBitmap();
        void Close();
        string GetPixelColorRGB();
        string GetPixelColorHex();
        string GetPixelColorCMYK();
    }
}
