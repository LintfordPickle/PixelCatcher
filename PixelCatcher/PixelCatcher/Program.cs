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

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                MessageBox.Show($"Unhandled Exception:\n{e.ExceptionObject}", "PixelCatcher Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            StartPixelCapture();
        }

        static void StartPixelCapture() {
            using (var pixelCatcherView = new PixelCatcherView()) {
                var aboutInformation = new AboutInformation();
                using (var presenter = new PixelCatcherPresenter(null, pixelCatcherView, aboutInformation)) {
                    Application.Run(pixelCatcherView);
                }
            }
        }
    }
}
