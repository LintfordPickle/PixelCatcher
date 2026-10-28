using PixelCatcher.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PixelCatcher.Helpers {
    public class SpriteAnimationMeta {
        public int FrameWidth { get; set; } = 16;
        public int FrameHeight { get; set; } = 16;
        public int NumberOfFrames { get; set; } = 1;
        public int FrameSpeed { get; set; } = 5;

        public static SpriteAnimationMeta Load(string jsonPath, int fallbackHeight, int imageWidth) {
            var meta = new SpriteAnimationMeta {
                FrameHeight = fallbackHeight > 0 ? fallbackHeight : 16,
                FrameWidth = fallbackHeight > 0 ? fallbackHeight : 16,
                FrameSpeed = 5
            };

            if (!string.IsNullOrEmpty(jsonPath) && File.Exists(jsonPath)) {
                try {
                    string json = File.ReadAllText(jsonPath);
                    var matches = Regex.Matches(json, @"[""']?([a-zA-Z0-9_]+)[""']?\s*:\s*([0-9]+(?:\.[0-9]+)?)");
                    var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (Match m in matches) {
                        if (m.Groups.Count >= 3 && int.TryParse(m.Groups[2].Value, out int val)) {
                            dict[m.Groups[1].Value] = val;
                        }
                    }

                    if (dict.TryGetValue("frameWidth", out int fw) || dict.TryGetValue("frame_width", out fw) || dict.TryGetValue("width", out fw)) {
                        meta.FrameWidth = fw;
                    }
                    if (dict.TryGetValue("frameHeight", out int fh) || dict.TryGetValue("frame_height", out fh) || dict.TryGetValue("height", out fh)) {
                        meta.FrameHeight = fh;
                    }
                    if (dict.TryGetValue("numberOfFrames", out int nf) || dict.TryGetValue("number_of_frames", out nf) || dict.TryGetValue("frameCount", out nf) || dict.TryGetValue("frames", out nf)) {
                        meta.NumberOfFrames = nf;
                    }
                    if (dict.TryGetValue("frameSpeed", out int fs) || dict.TryGetValue("frame_speed", out fs) || dict.TryGetValue("speed", out fs)) {
                        meta.FrameSpeed = fs;
                    }
                } catch { }
            }

            if (meta.FrameHeight <= 0) meta.FrameHeight = fallbackHeight > 0 ? fallbackHeight : 16;
            if (meta.FrameWidth <= 0) meta.FrameWidth = meta.FrameHeight;
            if (meta.FrameSpeed <= 0) meta.FrameSpeed = 5;

            int maxFrames = meta.FrameWidth > 0 ? Math.Max(1, imageWidth / meta.FrameWidth) : 1;
            if (meta.NumberOfFrames <= 0 || meta.NumberOfFrames > maxFrames) {
                meta.NumberOfFrames = maxFrames;
            }

            return meta;
        }
    }

    public class AmigaSpriteRunner : IDisposable {
        private static readonly Lazy<AmigaSpriteRunner> instance =
            new Lazy<AmigaSpriteRunner>(() => new AmigaSpriteRunner());

        public static AmigaSpriteRunner Instance => instance.Value;

        private static readonly Random random = new Random();

        public bool Enabled { get; private set; }
        public string SpritesFolder { get; private set; } = "Sprites";
        public int IntervalSeconds { get; private set; }
        public int Speed { get; private set; } = 5;

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

                string intervalVal = ConfigurationManager.AppSettings["EasterEggIntervalSeconds"];
                if (int.TryParse(intervalVal, out int interval)) {
                    IntervalSeconds = Math.Max(0, interval);
                } else {
                    IntervalSeconds = 120;
                }
            } catch {
                Enabled = true;
                SpritesFolder = "Sprites";
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
            var frames = LoadFramesFromStrip(chosenFile, out SpriteAnimationMeta meta);
            if (frames == null || frames.Length == 0 || meta == null) return;

            DisposeCurrentFrames();
            currentFrames = frames;
            SpriteWidth = meta.FrameWidth;
            SpriteHeight = meta.FrameHeight;
            Speed = meta.FrameSpeed;
            CurrentSpriteName = Path.GetFileNameWithoutExtension(chosenFile);

            CurrentTargetScreenshot = target;
            RunnerX = target.ClientSize.Width + 5;
            RunnerY = target.BorderSize + 1;
            CurrentFrameIndex = 0;
            IsRunning = true;

            animationTimer.Start();
        }

        private static Bitmap[] LoadFramesFromStrip(string filePath, out SpriteAnimationMeta meta) {
            meta = null;
            try {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var source = Image.FromStream(fs)) {
                    int h = source.Height;
                    int w = source.Width;
                    if (h <= 0 || w <= 0) return null;

                    string jsonPath = Path.ChangeExtension(filePath, ".json");
                    meta = SpriteAnimationMeta.Load(jsonPath, h, w);

                    int frameWidth = meta.FrameWidth;
                    int frameHeight = meta.FrameHeight;
                    int count = meta.NumberOfFrames;
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
