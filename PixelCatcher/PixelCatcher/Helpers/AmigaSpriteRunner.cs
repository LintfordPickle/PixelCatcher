using PixelCatcher.Views;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace PixelCatcher.Helpers {
    public class AmigaSpriteRunner : IDisposable {
        private static readonly Lazy<AmigaSpriteRunner> instance =
            new Lazy<AmigaSpriteRunner>(() => new AmigaSpriteRunner());

        public static AmigaSpriteRunner Instance => instance.Value;

        private static readonly Random random = new Random();

        public bool Enabled { get; private set; }
        public string SpritesFolder { get; private set; }
        public int ConfiguredFrameWidth { get; private set; }
        public int ConfiguredFrameHeight { get; private set; }
        public int IntervalSeconds { get; private set; }
        public int Speed { get; private set; }

        public bool IsRunning { get; private set; }
        public int RunnerX { get; private set; }
        public int RunnerY { get; private set; }
        public int SpriteWidth { get; private set; } = 16;
        public int SpriteHeight { get; private set; } = 16;
        public int CurrentFrameIndex { get; private set; }
        public string CurrentSpriteName { get; private set; }
        public ScreenshotView CurrentTargetScreenshot { get; private set; }

        private readonly Timer periodicTimer;
        private readonly Timer animationTimer;
        private Bitmap[] currentFrames;

        public AmigaSpriteRunner() {
            LoadConfiguration();

            periodicTimer = new Timer();
            animationTimer = new Timer { Interval = 45 }; // ~22 FPS classic Amiga feel

            animationTimer.Tick += AnimationTimer_Tick;
            periodicTimer.Tick += PeriodicTimer_Tick;

            if (Enabled && IntervalSeconds > 0) {
                periodicTimer.Interval = IntervalSeconds * 1000;
                periodicTimer.Start();
            }
        }

        public void LoadConfiguration() {
            try {
                string enabledVal = ConfigurationManager.AppSettings["EasterEggEnabled"];
                Enabled = string.IsNullOrEmpty(enabledVal) || !string.Equals(enabledVal, "false", StringComparison.OrdinalIgnoreCase);

                string folderVal = ConfigurationManager.AppSettings["EasterEggSpritesFolder"];
                SpritesFolder = string.IsNullOrWhiteSpace(folderVal) ? "Sprites" : folderVal.Trim();

                string fwVal = ConfigurationManager.AppSettings["EasterEggFrameWidth"];
                if (int.TryParse(fwVal, out int fw) && fw > 0) {
                    ConfiguredFrameWidth = fw;
                } else {
                    ConfiguredFrameWidth = 0; // auto-detect
                }

                string fhVal = ConfigurationManager.AppSettings["EasterEggFrameHeight"];
                if (int.TryParse(fhVal, out int fh) && fh > 0) {
                    ConfiguredFrameHeight = fh;
                } else {
                    ConfiguredFrameHeight = 0; // auto-detect
                }

                string intervalVal = ConfigurationManager.AppSettings["EasterEggIntervalSeconds"];
                if (int.TryParse(intervalVal, out int interval)) {
                    IntervalSeconds = Math.Max(0, interval);
                } else {
                    IntervalSeconds = 120;
                }

                string speedVal = ConfigurationManager.AppSettings["EasterEggSpeed"];
                if (int.TryParse(speedVal, out int speed)) {
                    Speed = Math.Max(2, Math.Min(15, speed));
                } else {
                    Speed = 5;
                }
            } catch {
                Enabled = true;
                SpritesFolder = "Sprites";
                ConfiguredFrameWidth = 16;
                ConfiguredFrameHeight = 16;
                IntervalSeconds = 120;
                Speed = 5;
            }
        }

        public string ResolveSpritesDirectory() {
            if (string.IsNullOrWhiteSpace(SpritesFolder)) return null;

            string folder = SpritesFolder.Trim();
            if (Path.IsPathRooted(folder)) {
                return Directory.Exists(folder) ? folder : null;
            }

            // 1. Check relative to executing assembly's directory (where PixelCatcher.exe lives)
            try {
                string assemblyDir = Path.GetDirectoryName(typeof(AmigaSpriteRunner).Assembly.Location);
                if (!string.IsNullOrEmpty(assemblyDir)) {
                    string asmCombined = Path.Combine(assemblyDir, folder);
                    if (Directory.Exists(asmCombined)) return asmCombined;
                }
            } catch { }

            // 2. Check relative to AppDomain base directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(baseDir)) {
                string combined = Path.Combine(baseDir, folder);
                if (Directory.Exists(combined)) return combined;
            }

            // 3. Check relative to working directory
            string workingDir = Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(workingDir)) {
                string workingCombined = Path.Combine(workingDir, folder);
                if (Directory.Exists(workingCombined)) return workingCombined;
            }

            return null;
        }

        public void TriggerRun(ScreenshotView target = null) {
            if (!Enabled || IsRunning) return;

            if (target == null || target.IsDisposed) {
                target = Models.ScreenshotGroupManager.Instance.ActiveScreenshot;
            }

            if (target == null || target.IsDisposed) return;

            string dir = ResolveSpritesDirectory();
            if (string.IsNullOrEmpty(dir)) return;

            string[] pngFiles;
            try {
                pngFiles = Directory.GetFiles(dir, "*.png");
            } catch {
                return;
            }

            if (pngFiles == null || pngFiles.Length == 0) return;

            string chosenFile = pngFiles[random.Next(pngFiles.Length)];
            var frames = LoadFramesFromStrip(chosenFile, ConfiguredFrameWidth, ConfiguredFrameHeight, out int frameWidth, out int frameHeight);
            if (frames == null || frames.Length == 0) return;

            DisposeCurrentFrames();
            currentFrames = frames;
            SpriteWidth = frameWidth;
            SpriteHeight = frameHeight;
            CurrentSpriteName = Path.GetFileNameWithoutExtension(chosenFile);

            CurrentTargetScreenshot = target;
            RunnerX = target.ClientSize.Width + 5;
            RunnerY = target.BorderSize + 1;
            CurrentFrameIndex = 0;
            IsRunning = true;

            animationTimer.Start();
        }

        private static Bitmap[] LoadFramesFromStrip(string filePath, int customWidth, int customHeight, out int frameWidth, out int frameHeight) {
            frameWidth = 16;
            frameHeight = 16;
            try {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var source = Image.FromStream(fs)) {
                    int h = source.Height;
                    int w = source.Width;
                    if (h <= 0 || w <= 0) return null;

                    frameHeight = customHeight > 0 ? customHeight : h;
                    frameWidth = customWidth > 0 ? customWidth : frameHeight;

                    if (frameWidth <= 0 || frameHeight <= 0) return null;

                    int count = w / frameWidth;
                    if (count <= 0) return null;

                    var frames = new Bitmap[count];

                    for (int i = 0; i < count; i++) {
                        var bmp = new Bitmap(frameWidth, frameHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        using (var g = Graphics.FromImage(bmp)) {
                            int sliceW = Math.Min(frameWidth, Math.Max(0, w - i * frameWidth));
                            int sliceH = Math.Min(frameHeight, h);
                            g.DrawImage(source,
                                new Rectangle(0, 0, sliceW, sliceH),
                                new Rectangle(i * frameWidth, 0, sliceW, sliceH),
                                GraphicsUnit.Pixel);
                        }
                        frames[i] = bmp;
                    }
                    return frames;
                }
            } catch {
                return null;
            }
        }

        private void PeriodicTimer_Tick(object sender, EventArgs e) {
            if (!IsRunning && Enabled) {
                TriggerRun();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e) {
            if (CurrentTargetScreenshot == null || CurrentTargetScreenshot.IsDisposed || currentFrames == null || currentFrames.Length == 0) {
                StopRunning();
                return;
            }

            int previousX = RunnerX;
            RunnerX -= Speed;
            CurrentFrameIndex = (CurrentFrameIndex + 1) % currentFrames.Length;

            // Invalidate only the region where the runner is moving to maintain 0% idle CPU overhead
            int invalidateLeft = Math.Max(0, RunnerX - Speed - 2);
            int invalidateWidth = (previousX - RunnerX) + SpriteWidth + 10;
            CurrentTargetScreenshot.InvalidateRunner(new Rectangle(invalidateLeft, RunnerY, invalidateWidth, SpriteHeight + 4));

            if (RunnerX < -SpriteWidth - 10) {
                StopRunning();
            }
        }

        private void StopRunning() {
            animationTimer.Stop();
            IsRunning = false;
            if (CurrentTargetScreenshot != null && !CurrentTargetScreenshot.IsDisposed) {
                CurrentTargetScreenshot.InvalidateRunner(new Rectangle(0, RunnerY, SpriteWidth + 20, SpriteHeight + 4));
            }
            CurrentTargetScreenshot = null;
            DisposeCurrentFrames();
        }

        public void DrawRunner(Graphics g) {
            if (!IsRunning || currentFrames == null || CurrentFrameIndex < 0 || CurrentFrameIndex >= currentFrames.Length) return;

            var frame = currentFrames[CurrentFrameIndex];
            if (frame == null) return;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(frame, RunnerX, RunnerY, SpriteWidth, SpriteHeight);
        }

        private void DisposeCurrentFrames() {
            if (currentFrames != null) {
                foreach (var b in currentFrames) {
                    b?.Dispose();
                }
                currentFrames = null;
            }
        }

        public void Dispose() {
            periodicTimer?.Stop();
            periodicTimer?.Dispose();
            animationTimer?.Stop();
            animationTimer?.Dispose();
            DisposeCurrentFrames();
        }
    }
}
