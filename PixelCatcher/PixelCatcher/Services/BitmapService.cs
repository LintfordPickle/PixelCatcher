using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PixelCatcher {
    class BitmapService {
        public static Bitmap CropBitmap(Bitmap source, Rectangle srcRect) {
            if(srcRect == null || srcRect.Width == 0 || srcRect.Height == 0) {
                return source;
            }

            var bmp = new Bitmap(srcRect.Width, srcRect.Height);
            using (var gr = Graphics.FromImage(bmp)) {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), srcRect, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        public static Bitmap ModifyBitmap(Bitmap originalBitmap, float brightness, float contrast, float gamma) {
            Bitmap adjustedImage = new Bitmap(originalBitmap);

            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
                new float[] {contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            Graphics g = Graphics.FromImage(adjustedImage);

            g.DrawImage(originalBitmap, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
                , 0, 0, originalBitmap.Width, originalBitmap.Height,
                GraphicsUnit.Pixel, imageAttributes);

            return adjustedImage;
        }

        public static void SaveBitmapToFile(Bitmap bitmap,String filepath, ImageFormat format) {
            bitmap.Save(filepath, format);
        }
    }
}
