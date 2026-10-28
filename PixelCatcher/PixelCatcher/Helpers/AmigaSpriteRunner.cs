using PixelCatcher.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
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
                    var matches = Regex.Matches(json, @"[""']?([a-zA-Z0-9_]+)[""']?\s*:\s*[""']?\s*([0-9]+(?:\.[0-9]+)?)\s*[""']?");
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

    public class WindowRunnerInstance {
        public ScreenshotView Window { get; set; }
        public int LocalX { get; set; }
        public int PreviousLocalX { get; set; }
        public int LocalY { get; set; }
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
        public int SpriteWidth { get; private set; } = 16;
        public int SpriteHeight { get; private set; } = 16;
        public int CurrentFrameIndex { get; private set; }
        public string CurrentSpriteName { get; private set; }

        public ScreenshotView CurrentTargetScreenshot => activeRunners.Count > 0 ? activeRunners[0].Window : null;
        public int RunnerX => activeRunners.Count > 0 ? activeRunners[0].LocalX : 0;
        public int RunnerY => activeRunners.Count > 0 ? activeRunners[0].LocalY : 0;
        public IReadOnlyList<WindowRunnerInstance> ActiveRunners => activeRunners.AsReadOnly();

        private readonly Timer periodicTimer;
        private readonly Timer animationTimer;
        private readonly List<WindowRunnerInstance> activeRunners = new List<WindowRunnerInstance>();
        private readonly List<ScreenshotView> windowSequence = new List<ScreenshotView>();
        private int nextWindowIndex = 0;
        private int gapRemaining = 0;
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

        public void TriggerRun(ScreenshotView preferredTarget = null) {
            if (!Enabled || IsRunning) return;

            var activeScreenshots = Models.ScreenshotGroupManager.Instance.GetAllActiveScreenshots();
            if (activeScreenshots == null || activeScreenshots.Count == 0) return;

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

            activeRunners.Clear();
            windowSequence.Clear();
            gapRemaining = 0;

            // Sort active screenshots strictly from right to left across the desktop
            windowSequence.AddRange(activeScreenshots
                .OrderByDescending(s => s.ImageScreenBounds.Right)
                .ThenByDescending(s => s.Area));

            // Start first window from its right edge
            var first = windowSequence[0];
            int startX = first.ImageScreenBounds.Width + 5;
            activeRunners.Add(new WindowRunnerInstance {
                Window = first,
                LocalX = startX,
                PreviousLocalX = startX,
                LocalY = first.BorderSize + 1
            });

            nextWindowIndex = 1;
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
            if (!IsRunning || currentFrames == null || currentFrames.Length == 0) {
                StopRunning();
                return;
            }

            // Step animation frame
            CurrentFrameIndex = (CurrentFrameIndex + 1) % currentFrames.Length;

            // 1. Move all active window runners leftwards and invalidate
            for (int i = activeRunners.Count - 1; i >= 0; i--) {
                var runner = activeRunners[i];
                if (runner.Window == null || runner.Window.IsDisposed || !runner.Window.Visible) {
                    activeRunners.RemoveAt(i);
                    continue;
                }

                runner.PreviousLocalX = runner.LocalX;
                runner.LocalX -= Speed;

                int invalidateLeft = Math.Max(0, runner.LocalX - Speed - 2);
                int invalidateWidth = (runner.PreviousLocalX - runner.LocalX) + SpriteWidth + 10;
                runner.Window.InvalidateRunner(new Rectangle(invalidateLeft, runner.LocalY, invalidateWidth, SpriteHeight + 4));
            }

            // 2. Check if next window should be spawned
            while (nextWindowIndex < windowSequence.Count) {
                var prevWindow = windowSequence[nextWindowIndex - 1];
                var nextWindow = windowSequence[nextWindowIndex];

                if (nextWindow == null || nextWindow.IsDisposed || !nextWindow.Visible) {
                    nextWindowIndex++;
                    continue;
                }

                Rectangle prevBounds = prevWindow.ImageScreenBounds;
                Rectangle nextBounds = nextWindow.ImageScreenBounds;

                bool overlaps = nextBounds.Right > prevBounds.Left;

                if (overlaps) {
                    // Scenario 1: Overlapping screenshots
                    // Wait until prevWindow has completely finished and exited activeRunners
                    bool prevStillActive = activeRunners.Any(r => r.Window == prevWindow);
                    if (!prevStillActive) {
                        // Start nextWindow from its right-hand edge so it enters smoothly
                        int startX = nextBounds.Width + 5;
                        activeRunners.Add(new WindowRunnerInstance {
                            Window = nextWindow,
                            LocalX = startX,
                            PreviousLocalX = startX,
                            LocalY = nextWindow.BorderSize + 1
                        });
                        nextWindowIndex++;
                    }
                    break;
                } else {
                    // Scenario 2: Adjacent or separated by a gap (nextBounds.Right <= prevBounds.Left)
                    int gap = prevBounds.Left - nextBounds.Right;
                    var prevRunner = activeRunners.FirstOrDefault(r => r.Window == prevWindow);

                    if (prevRunner != null) {
                        // prevWindow is still running
                        if (prevRunner.LocalX <= -gap) {
                            // Fluid transition: spawn second animation instance entering nextWindow
                            int startX = nextBounds.Width + (prevRunner.LocalX + gap);
                            activeRunners.Add(new WindowRunnerInstance {
                                Window = nextWindow,
                                LocalX = startX,
                                PreviousLocalX = startX,
                                LocalY = nextWindow.BorderSize + 1
                            });
                            nextWindowIndex++;
                        }
                        break;
                    } else {
                        // prevWindow already finished (gap >= SpriteWidth)
                        gapRemaining -= Speed;
                        if (gapRemaining <= 0) {
                            int startX = nextBounds.Width + gapRemaining;
                            activeRunners.Add(new WindowRunnerInstance {
                                Window = nextWindow,
                                LocalX = startX,
                                PreviousLocalX = startX,
                                LocalY = nextWindow.BorderSize + 1
                            });
                            nextWindowIndex++;
                        }
                        break;
                    }
                }
            }

            // 3. Remove runners that have fully exited past their window's left edge
            for (int i = activeRunners.Count - 1; i >= 0; i--) {
                var runner = activeRunners[i];
                if (runner.LocalX < -SpriteWidth - 2) {
                    // If next window was waiting for a gap >= SpriteWidth, calculate remaining gap
                    if (nextWindowIndex < windowSequence.Count && windowSequence[nextWindowIndex - 1] == runner.Window) {
                        var nextWindow = windowSequence[nextWindowIndex];
                        if (nextWindow != null && !nextWindow.IsDisposed && nextWindow.Visible) {
                            int gap = runner.Window.ImageScreenBounds.Left - nextWindow.ImageScreenBounds.Right;
                            if (gap > SpriteWidth) {
                                gapRemaining = gap - SpriteWidth;
                            } else {
                                gapRemaining = 0;
                            }
                        }
                    }

                    if (!runner.Window.IsDisposed) {
                        runner.Window.InvalidateRunner(new Rectangle(0, runner.LocalY, SpriteWidth + 20, SpriteHeight + 4));
                    }
                    activeRunners.RemoveAt(i);
                }
            }

            // 4. If all runners and all windows are finished, stop animation
            if (activeRunners.Count == 0 && nextWindowIndex >= windowSequence.Count) {
                StopRunning();
            }
        }

        private void StopRunning() {
            animationTimer.Stop();
            IsRunning = false;
            foreach (var runner in activeRunners) {
                if (runner.Window != null && !runner.Window.IsDisposed) {
                    runner.Window.InvalidateRunner(new Rectangle(0, runner.LocalY, SpriteWidth + 20, SpriteHeight + 4));
                }
            }
            activeRunners.Clear();
            windowSequence.Clear();
            gapRemaining = 0;
            DisposeCurrentFrames();
        }

        public void DrawRunner(ScreenshotView window, Graphics g) {
            if (!IsRunning || currentFrames == null || CurrentFrameIndex < 0 || CurrentFrameIndex >= currentFrames.Length) return;

            var runner = activeRunners.FirstOrDefault(r => r.Window == window);
            if (runner == null) return;

            var frame = currentFrames[CurrentFrameIndex];
            if (frame == null) return;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(frame, runner.LocalX, runner.LocalY, SpriteWidth, SpriteHeight);
        }

        public void DrawRunner(Graphics g) {
            if (activeRunners.Count > 0) {
                DrawRunner(activeRunners[0].Window, g);
            }
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
