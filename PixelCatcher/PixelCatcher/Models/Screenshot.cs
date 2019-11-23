using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PixelCatcher.Models {
    public class Screenshot : INotifyPropertyChanged {
        private Bitmap bitmap;
        private int screenshotWidth;
        private int screenshotHeight;

        private event PropertyChangedEventHandler propertyChanged;

        public Bitmap ScreenshotBitmap {
            get { return bitmap; }
            set {
                bitmap = value;
                FirePropertyChanged("ScreenshotBitmap");
            }
        }

        public int ScreenshotWidth {
            get { return screenshotWidth; }
            set {
                screenshotWidth = value;
                FirePropertyChanged("screenshotWidth");
            }
        }

        public int ScreenshotHeight {
            get { return screenshotHeight; }
            set {
                screenshotHeight = value;
                FirePropertyChanged("screenshotHeight");
            }
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
