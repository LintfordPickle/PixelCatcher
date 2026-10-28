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
            using (var pixelCatcherView = new PixelCatcherView())
            using (var aboutView = new AboutView()) {
                var aboutInformation = new AboutInformation();
                using (var presenter = new PixelCatcherPresenter(aboutView, pixelCatcherView, aboutInformation)) {
                    Application.Run(pixelCatcherView);
                }
            }
        }
    }
}
