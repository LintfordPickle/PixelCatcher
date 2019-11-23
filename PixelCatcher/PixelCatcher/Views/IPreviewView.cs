using System.Drawing;

namespace PixelCatcher.Views {
    public interface IPreviewView {
        void SetPreviewSize(Size previewSize);
        void SetPreviewMagnification(float previewScale);
    }
}
