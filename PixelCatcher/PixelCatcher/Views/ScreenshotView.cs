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
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                var badgeRect = new Rectangle(5, 5, 22, 22);

                // Drop shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0))) {
                    e.Graphics.FillEllipse(shadowBrush, new Rectangle(6, 6, 22, 22));
                }

                // Badge circle in group color
                using (var badgeBrush = new SolidBrush(GroupColor)) {
                    e.Graphics.FillEllipse(badgeBrush, badgeRect);
                }

                // White border around badge (thicker if active)
                Color outlineColor = IsActive ? Color.White : Color.FromArgb(220, 255, 255, 255);
                float penWidth = IsActive ? 2f : 1.2f;
                using (var outlinePen = new Pen(outlineColor, penWidth)) {
                    e.Graphics.DrawEllipse(outlinePen, badgeRect);
                }

                // Group number text
                using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold)) {
                    TextRenderer.DrawText(
                        e.Graphics,
                        GroupNumber.Value.ToString(),
                        font,
                        badgeRect,
                        Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

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
