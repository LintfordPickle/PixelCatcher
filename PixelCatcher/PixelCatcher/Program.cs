using PixelCatcher.Models;
using PixelCatcher.Presenters;
using PixelCatcher.Views;
using System;
using System.Windows.Forms;

namespace PixelCatcher {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            StartPixelCapture();
        }

        static void StartPixelCapture() {
            var pixelCatcherView = new PixelCatcherView();
            var aboutInformation = new AboutInformation();
            var aboutView = new AboutView();

            var pixelCatcherPresenter = new PixelCatcherPresenter(aboutView, pixelCatcherView, aboutInformation);

            Application.Run(pixelCatcherView);

        }
    }
}
