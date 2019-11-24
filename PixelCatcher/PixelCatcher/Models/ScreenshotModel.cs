using System.ComponentModel;
using System.Drawing;

namespace PixelCatcher.Models {
    public class ScreenshotModel : INotifyPropertyChanged {
        private Bitmap screenshotBitmap;
        private Point topLeftPosition;

        private event PropertyChangedEventHandler propertyChanged;

        public Bitmap ScreenshotBitmap {
            get { return screenshotBitmap; }
            set {
                screenshotBitmap = value;
                FirePropertyChanged("ScreenshotBitmap");
            }
        }

        public Point TopLeftPosition {
            get { return topLeftPosition; }
        }

        public int ScreenshotWidth {
            get { return screenshotBitmap.Width; }
        }

        public int ScreenshotHeight {
            get { return screenshotBitmap.Height; }
        }

        public ScreenshotModel(Bitmap screenshotBitmap, Point topLeftPoint) {
            this.screenshotBitmap = screenshotBitmap;
            topLeftPosition = topLeftPoint;
        }

        public event PropertyChangedEventHandler PropertyChanged {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        private void FirePropertyChanged(string propertyName) {
            PropertyChangedEventHandler safeHandler = propertyChanged;

            if (safeHandler != null)
                safeHandler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
