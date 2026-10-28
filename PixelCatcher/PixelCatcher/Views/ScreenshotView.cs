using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class ScreenshotView : Form, IScreenshotView {
        private Bitmap screenshotBitmap;
        private int windowPositionOffsetX;
        private int windowPositionOffsetY;
        private int screenshotWidth;
        private int screenshotHeight;
        private bool isActive;

        public int BorderSize { get; set; }
        public int? GroupNumber { get; set; }
        public Color GroupColor { get; set; } = ScreenshotGroupManager.GetGroupColor(0);

        public bool IsActive {
            get => isActive;
            set {
                if (isActive != value) {
                    isActive = value;
                    RefreshVisualState();
                }
            }
        }

        public event EventHandler CopyScreenshotToClipboard;
        public event EventHandler SaveScreenshotToFile;

        public ScreenshotView() {
            InitializeComponent();
            DoubleBuffered = true;
            KeyPreview = true;
            BorderSize = 3;

            pictureBox.Paint += PictureBox_Paint;
            MouseDown += Form_MouseDown;
            MouseMove += Form_MouseMove;
        }

        public void SetBitmap(Bitmap screenshotBitmap, Point topLeft) {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;

            this.screenshotBitmap = screenshotBitmap;

            int screenLeft = topLeft.X;
            int screenTop = topLeft.Y;
            screenshotWidth = this.screenshotBitmap.Width;
            screenshotHeight = this.screenshotBitmap.Height;

            pictureBox.Location = new Point(BorderSize, BorderSize);
            pictureBox.Image = this.screenshotBitmap;
            pictureBox.Size = new Size(screenshotWidth, screenshotHeight);

            RefreshVisualState();
            ClientSize = new Size(screenshotWidth + BorderSize * 2, screenshotHeight + BorderSize * 2);

            // Set initial position so the cropped image aligns exactly over the captured screen area
            StartPosition = FormStartPosition.Manual;
            Location = new Point(screenLeft - BorderSize, screenTop - BorderSize);

            ScreenshotGroupManager.Instance.Register(this);
        }

        public void RefreshVisualState() {
            if (GroupNumber.HasValue && GroupNumber.Value > 0) {
                BackColor = GroupColor;
            } else {
                BackColor = IsActive ? Color.FromArgb(52, 152, 219) : Color.FromArgb(100, 110, 125);
            }
            pictureBox.Invalidate();
            Invalidate();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e) {
            if (GroupNumber.HasValue && GroupNumber.Value > 0) {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                using (var path = new System.Drawing.Drawing2D.GraphicsPath()) {
                    using (var fontFamily = new FontFamily("Segoe UI")) {
                        path.AddString(
                            GroupNumber.Value.ToString(),
                            fontFamily,
                            (int)FontStyle.Bold,
                            18f,
                            new Point(6, 4),
                            StringFormat.GenericDefault);
                    }

                    // Stroke / outline around the number
                    using (var strokePen = new Pen(Color.FromArgb(220, 0, 0, 0), 3f) {
                        LineJoin = System.Drawing.Drawing2D.LineJoin.Round
                    }) {
                        e.Graphics.DrawPath(strokePen, path);
                    }

                    // Inner accent stroke if active
                    if (IsActive) {
                        using (var activePen = new Pen(Color.White, 1.2f) {
                            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
                        }) {
                            e.Graphics.DrawPath(activePen, path);
                        }
                    }

                    // Body fill of the number in group color
                    using (var fillBrush = new SolidBrush(GroupColor)) {
                        e.Graphics.FillPath(fillBrush, path);
                    }
                }
            }

            // Amiga Easter Egg Runner (Zool / Superfrog)
            if (PixelCatcher.Helpers.AmigaSpriteRunner.Instance.IsRunning &&
                PixelCatcher.Helpers.AmigaSpriteRunner.Instance.CurrentTargetScreenshot == this) {
                PixelCatcher.Helpers.AmigaSpriteRunner.Instance.DrawRunner(e.Graphics);
            }
        }

        public void InvalidateRunner(Rectangle rect) {
            if (!IsDisposed && pictureBox != null && !pictureBox.IsDisposed) {
                pictureBox.Invalidate(rect);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            if (e.Control && (e.KeyCode == Keys.Z || e.KeyCode == Keys.M)) {
                PixelCatcher.Helpers.AmigaSpriteRunner.Instance.TriggerRun(this);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Control) {
                int? number = null;
                if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) {
                    number = e.KeyCode - Keys.D0;
                } else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) {
                    number = e.KeyCode - Keys.NumPad0;
                }

                if (number.HasValue) {
                    if (number.Value == 0) {
                        ScreenshotGroupManager.Instance.Ungroup(this);
                    } else {
                        ScreenshotGroupManager.Instance.AssignGroup(this, number.Value);
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
            ScreenshotGroupManager.Instance.SetActive(this);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                Focus();
                ScreenshotGroupManager.Instance.SetActive(this);
                windowPositionOffsetX = e.X;
                windowPositionOffsetY = e.Y;
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                int targetX = Cursor.Position.X - windowPositionOffsetX;
                int targetY = Cursor.Position.Y - windowPositionOffsetY;

                int dx = targetX - Location.X;
                int dy = targetY - Location.Y;

                if (dx != 0 || dy != 0) {
                    Location = new Point(targetX, targetY);
                    ScreenshotGroupManager.Instance.MoveGroup(this, dx, dy);
                }
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                Focus();
                ScreenshotGroupManager.Instance.SetActive(this);
                windowPositionOffsetX = e.X;
                windowPositionOffsetY = e.Y;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                int targetX = Cursor.Position.X - windowPositionOffsetX;
                int targetY = Cursor.Position.Y - windowPositionOffsetY;

                int dx = targetX - Location.X;
                int dy = targetY - Location.Y;

                if (dx != 0 || dy != 0) {
                    Location = new Point(targetX, targetY);
                    ScreenshotGroupManager.Instance.MoveGroup(this, dx, dy);
                }
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e) {
        }

        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e) {
            Close();
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e) {
            CopyScreenshotToClipboard?.Invoke(this, e);
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveScreenshotToFile?.Invoke(this, e);
        }

        private void ScreenshotForm_Shown(object sender, EventArgs e) {
            ClientSize = new Size(screenshotWidth + BorderSize * 2, screenshotHeight + BorderSize * 2);
        }

        protected override void OnFormClosed(FormClosedEventArgs e) {
            base.OnFormClosed(e);
            ScreenshotGroupManager.Instance.Unregister(this);
            pictureBox.Image = null;
            screenshotBitmap?.Dispose();
            screenshotBitmap = null;
        }
    }
}
