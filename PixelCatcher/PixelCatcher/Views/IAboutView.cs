using System;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public interface IAboutView {

        event EventHandler visitAboutUrl;

        event FormClosedEventHandler FormClosed;

        void Show();
        void BringToFront();
        bool IsDisposed { get; }

    }
}
