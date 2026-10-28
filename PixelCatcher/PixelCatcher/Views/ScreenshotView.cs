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
        private string title;
        private TextBox titleInputBox;

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

        public string Title {
            get => title;
            set {
                if (title != value) {
                    title = value;
                    TitleChanged?.Invoke(this, EventArgs.Empty);
                    RefreshVisualState();
                }
            }
        }

        public event EventHandler CopyScreenshotToClipboard;
        public event EventHandler SaveScreenshotToFile;
        public event EventHandler TitleChanged;

        public ScreenshotView() {
            InitializeComponent();
            DoubleBuffered = true;
            KeyPreview = true;
            BorderSize = 3;

            titleInputBox = new TextBox {
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(28, 32, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            titleInputBox.KeyDown += TitleInputBox_KeyDown;
            titleInputBox.LostFocus += TitleInputBox_LostFocus;
            pictureBox.Controls.Add(titleInputBox);

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

        public Bitmap GetExportBitmap() {
            if (string.IsNullOrWhiteSpace(Title) || screenshotBitmap == null) {
                return screenshotBitmap;
            }

            var baked = new Bitmap(screenshotBitmap);
            using (var g = Graphics.FromImage(baked)) {
                PixelCatcher.Helpers.TitleRenderer.DrawTitleBanner(g, Title, 8, 4, baked.Width - 16);
            }
            return baked;
        }

        public void ShowTitleInput() {
            int startX = (GroupNumber.HasValue && GroupNumber.Value > 0) ? 34 : 8;
            int startY = 4;
            int maxWidth = Math.Max(120, Math.Min(350, pictureBox.Width - startX - 8));
            if (maxWidth < 60) {
                startX = 4;
                maxWidth = Math.Max(30, pictureBox.Width - 8);
            }

            titleInputBox.Location = new Point(startX, startY);
            titleInputBox.Size = new Size(maxWidth, 24);
            titleInputBox.Text = Title ?? string.Empty;
            titleInputBox.Visible = true;
            titleInputBox.BringToFront();
            titleInputBox.Focus();
            titleInputBox.SelectAll();
            pictureBox.Invalidate();
        }

        private void CommitTitleInput() {
            if (titleInputBox == null || !titleInputBox.Visible) return;

            string newTitle = titleInputBox.Text.Trim();
            titleInputBox.Visible = false;
            Title = string.IsNullOrEmpty(newTitle) ? null : newTitle;
            Focus();
            RefreshVisualState();
        }

        private void CancelTitleInput() {
            if (titleInputBox == null || !titleInputBox.Visible) return;

            titleInputBox.Visible = false;
            Focus();
            RefreshVisualState();
        }

        private void TitleInputBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F3) {
                CommitTitleInput();
                e.Handled = true;
                e.SuppressKeyPress = true;
            } else if (e.KeyCode == Keys.Escape) {
                CancelTitleInput();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TitleInputBox_LostFocus(object sender, EventArgs e) {
            CommitTitleInput();
        }

        private void setTitleToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowTitleInput();
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

            // Draw Title Banner if set and not currently editing in textbox
            if ((titleInputBox == null || !titleInputBox.Visible) && !string.IsNullOrWhiteSpace(Title)) {
                int startX = (GroupNumber.HasValue && GroupNumber.Value > 0) ? 34 : 8;
                PixelCatcher.Helpers.TitleRenderer.DrawTitleBanner(e.Graphics, Title, startX, 4, pictureBox.Width - startX - 8);
            }

            // Amiga Easter Egg Runner
            if (PixelCatcher.Helpers.AmigaSpriteRunner.Instance.IsRunning) {
                PixelCatcher.Helpers.AmigaSpriteRunner.Instance.DrawRunner(this, e.Graphics);
            }
        }

        public void InvalidateRunner(Rectangle rect) {
            if (!IsDisposed && pictureBox != null && !pictureBox.IsDisposed) {
                pictureBox.Invalidate(rect);
            }
        }

        public Rectangle ImageScreenBounds {
            get {
                if (IsDisposed || pictureBox == null || pictureBox.IsDisposed) {
                    return Bounds;
                }
                Point screenTopLeft = PointToScreen(pictureBox.Location);
                return new Rectangle(screenTopLeft.X, screenTopLeft.Y, pictureBox.Width, pictureBox.Height);
            }
        }

        public int Area => ImageScreenBounds.Width * ImageScreenBounds.Height;

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.F3) {
                ShowTitleInput();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

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
            if (titleInputBox != null && titleInputBox.Visible) {
                CommitTitleInput();
            }

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
            if (titleInputBox != null && titleInputBox.Visible) {
                CommitTitleInput();
            }

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
