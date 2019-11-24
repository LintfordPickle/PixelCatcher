using System;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public interface IPixelCatcherView {

        event EventHandler ShowAboutBox;

        ApplicationContext GetApplicationContext();
    }
}
