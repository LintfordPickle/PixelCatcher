using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PixelCatcher.Presenters {
    public class ScreenshotPresenter {
        public ScreenshotModel ScreenshotModel;
        public IScreenshotView ScreenshotView;

        public ScreenshotPresenter(IScreenshotView screenshotView, ScreenshotModel screenshotModel) {
            ScreenshotView = screenshotView;
            ScreenshotModel = screenshotModel;

            ScreenshotView.CopyScreenshotToClipboard += ScreenshotView_CopyScreenshotToClipboard;
            ScreenshotView.SaveScreenshotToFile += ScreenshotView_SaveScreenshotToFile;
        }

        private void ScreenshotView_SaveScreenshotToFile(object sender, EventArgs e) {
            var saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = $"Screenshot_{DateTime.Now.ToString("yyyyMMdd_hhmmfff")}";
            saveFileDialog1.Filter = "Png Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save to Image File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "") {
                var imageFormat = ImageFormat.Jpeg;

                // Saves the Image via a FileStream created by the OpenFile method.
                var filepath = saveFileDialog1.FileName;

                switch (saveFileDialog1.FilterIndex) {
                    case 1:
                        imageFormat = ImageFormat.Png;
                        break;
                    case 2:
                        imageFormat = ImageFormat.Jpeg;
                        break;
                    case 3:
                        imageFormat = ImageFormat.Bmp;
                        break;
                    case 4:
                        imageFormat = ImageFormat.Gif;
                        break;
                }

                BitmapService.SaveBitmapToFile(ScreenshotModel.ScreenshotBitmap, filepath, imageFormat);
            }
        }

        private void ScreenshotView_CopyScreenshotToClipboard(object sender, EventArgs e) {
            Clipboard.SetImage(ScreenshotModel.ScreenshotBitmap);
        }
    }
}
