using PixelCatcher.Presenters;

namespace PixelCatcher.Views {
    public interface ICaptureMonitorView {
        void AddPresenter(ICaptureDesktopPresenter desktopPresenter);
        void Close();
    }
}
