using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.Drawing;
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

            ScreenshotView.Title = ScreenshotModel.Title;
            ScreenshotView.TitleChanged += (s, e) => {
                ScreenshotModel.Title = ScreenshotView.Title;
            };

            ScreenshotModel.PropertyChanged += (s, e) => {
                if (e.PropertyName == "Title" && ScreenshotView.Title != ScreenshotModel.Title) {
                    ScreenshotView.Title = ScreenshotModel.Title;
                }
            };

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

                    Bitmap exportBitmap = ScreenshotView.GetExportBitmap();
                    try {
                        BitmapService.SaveBitmapToFile(exportBitmap, saveFileDialog.FileName, imageFormat);
                    } finally {
                        if (exportBitmap != null && exportBitmap != ScreenshotModel.ScreenshotBitmap) {
                            exportBitmap.Dispose();
                        }
                    }
                }
            }
        }

        private void ScreenshotView_CopyScreenshotToClipboard(object sender, EventArgs e) {
            Bitmap exportBitmap = ScreenshotView.GetExportBitmap();
            Clipboard.SetImage(exportBitmap);
        }
    }
}
