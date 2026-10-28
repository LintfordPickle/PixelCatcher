using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace PixelCatcher.Helpers {
    public static class TitleRenderer {
        public static void DrawTitleBanner(Graphics g, string title, int startX, int startY, int maxWidth) {
            if (string.IsNullOrWhiteSpace(title) || g == null) {
                return;
            }

            var prevSmoothing = g.SmoothingMode;
            var prevTextHint = g.TextRenderingHint;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            try {
                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(title, font);
                    int padH = 8;
                    int padV = 3;
                    int bannerWidth = (int)Math.Ceiling(textSize.Width) + padH * 2;
                    int bannerHeight = (int)Math.Ceiling(textSize.Height) + padV * 2;

                    if (maxWidth > 0 && bannerWidth > maxWidth) {
                        bannerWidth = maxWidth;
                    }

                    if (bannerWidth < 10 || bannerHeight < 8) {
                        return;
                    }

                    Rectangle bannerRect = new Rectangle(startX, startY, bannerWidth, bannerHeight);

                    using (var path = GetRoundedRectanglePath(bannerRect, 4)) {
                        using (var brush = new SolidBrush(Color.FromArgb(215, 20, 24, 30))) {
                            g.FillPath(brush, path);
                        }
                        using (var borderPen = new Pen(Color.FromArgb(90, 255, 255, 255), 1f)) {
                            g.DrawPath(borderPen, path);
                        }
                    }

                    int textWidth = Math.Max(0, bannerWidth - padH * 2);
                    int textHeight = Math.Max(0, bannerHeight - padV * 2);
                    if (textWidth > 0 && textHeight > 0) {
                        Rectangle textRect = new Rectangle(startX + padH, startY + padV, textWidth, textHeight);
                        using (var sf = new StringFormat {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        }) {
                            using (var textBrush = new SolidBrush(Color.White)) {
                                g.DrawString(title, font, textBrush, textRect, sf);
                            }
                        }
                    }
                }
            } finally {
                g.SmoothingMode = prevSmoothing;
                g.TextRenderingHint = prevTextHint;
            }
        }

        public static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius) {
            var path = new GraphicsPath();
            int r = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            if (r <= 0) {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = r * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
