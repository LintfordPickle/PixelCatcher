using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.Drawing.Imaging;
using System.IO;
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
            using (var saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.FileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}";
                saveFileDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg|Bitmap Image (*.bmp)|*.bmp|GIF Image (*.gif)|*.gif";
                saveFileDialog.DefaultExt = "png";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.Title = "Save Screenshot to File";

                if (saveFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(saveFileDialog.FileName)) {
                    var extension = Path.GetExtension(saveFileDialog.FileName)?.ToLowerInvariant();
                    ImageFormat imageFormat;

                    switch (extension) {
                        case ".jpg":
                        case ".jpeg":
                            imageFormat = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            imageFormat = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            imageFormat = ImageFormat.Gif;
                            break;
                        case ".png":
                        default:
                            imageFormat = ImageFormat.Png;
                            break;
                    }

                    BitmapService.SaveBitmapToFile(ScreenshotModel.ScreenshotBitmap, saveFileDialog.FileName, imageFormat);
                }
            }
        }

        private void ScreenshotView_CopyScreenshotToClipboard(object sender, EventArgs e) {
            Clipboard.SetImage(ScreenshotModel.ScreenshotBitmap);
        }
    }
}
