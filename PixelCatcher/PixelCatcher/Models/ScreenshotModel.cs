using System.ComponentModel;
using System.Drawing;
using PixelCatcher.Helpers;

namespace PixelCatcher.Models {
    public class ScreenshotModel : INotifyPropertyChanged {
        private Bitmap screenshotBitmap;
        private Point topLeftPosition;
        private string title;

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
            set {
                topLeftPosition = value;
                FirePropertyChanged("TopLeftPosition");
            }
        }

        public string Title {
            get { return title; }
            set {
                if (title != value) {
                    title = value;
                    FirePropertyChanged("Title");
                }
            }
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

        public Bitmap GetBakedBitmap() {
            if (string.IsNullOrWhiteSpace(title) || screenshotBitmap == null) {
                return screenshotBitmap;
            }

            var baked = new Bitmap(screenshotBitmap);
            using (var g = Graphics.FromImage(baked)) {
                TitleRenderer.DrawTitleBanner(g, title, 8, 4, baked.Width - 16);
            }
            return baked;
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
