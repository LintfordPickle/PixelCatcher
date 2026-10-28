using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PixelCatcher {
    class BitmapService {
        public static Bitmap CropBitmap(Bitmap source, Rectangle srcRect) {
            if (source == null || srcRect.Width <= 0 || srcRect.Height <= 0) {
                return new Bitmap(1, 1);
            }

            var sourceBounds = new Rectangle(0, 0, source.Width, source.Height);
            var intersection = Rectangle.Intersect(sourceBounds, srcRect);
            if (intersection.Width <= 0 || intersection.Height <= 0) {
                return new Bitmap(Math.Max(1, srcRect.Width), Math.Max(1, srcRect.Height));
            }

            var bmp = new Bitmap(srcRect.Width, srcRect.Height);
            using (var gr = Graphics.FromImage(bmp)) {
                var destRect = new Rectangle(intersection.X - srcRect.X, intersection.Y - srcRect.Y, intersection.Width, intersection.Height);
                gr.DrawImage(source, destRect, intersection, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        public static Bitmap ModifyBitmap(Bitmap originalBitmap, float brightness, float contrast, float gamma) {
            Bitmap adjustedImage = new Bitmap(originalBitmap);

            float adjustedBrightness = brightness - 1.0f;
            float[][] ptsArray = {
                new float[] {contrast, 0, 0, 0, 0},
                new float[] {0, contrast, 0, 0, 0},
                new float[] {0, 0, contrast, 0, 0},
                new float[] {0, 0, 0, 1.0f, 0},
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
            };

            using (ImageAttributes imageAttributes = new ImageAttributes()) {
                imageAttributes.ClearColorMatrix();
                imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

                using (Graphics g = Graphics.FromImage(adjustedImage)) {
                    g.DrawImage(originalBitmap, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height),
                        0, 0, originalBitmap.Width, originalBitmap.Height,
                        GraphicsUnit.Pixel, imageAttributes);
                }
            }

            return adjustedImage;
        }

        public static void SaveBitmapToFile(Bitmap bitmap, string filepath, ImageFormat format) {
            bitmap.Save(filepath, format);
        }
    }
}
