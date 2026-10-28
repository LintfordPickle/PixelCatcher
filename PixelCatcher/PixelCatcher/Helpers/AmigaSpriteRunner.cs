using PixelCatcher.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PixelCatcher.Helpers {
    public class AmigaSpriteRunner : IDisposable {
        private static readonly Lazy<AmigaSpriteRunner> instance =
            new Lazy<AmigaSpriteRunner>(() => new AmigaSpriteRunner());

        public static AmigaSpriteRunner Instance => instance.Value;

        public const int SpriteWidth = 24;
        public const int SpriteHeight = 18;

        public bool Enabled { get; private set; }
        public string CharacterName { get; private set; }
        public int IntervalSeconds { get; private set; }
        public int Speed { get; private set; }

        public bool IsRunning { get; private set; }
        public int RunnerX { get; private set; }
        public int RunnerY { get; private set; }
        public int CurrentFrameIndex { get; private set; }
        public ScreenshotView CurrentTargetScreenshot { get; private set; }

        private readonly Timer periodicTimer;
        private readonly Timer animationTimer;
        private Bitmap[] currentFrames;
        private Bitmap[] zoolFrames;
        private Bitmap[] superfrogFrames;

        public AmigaSpriteRunner() {
            LoadConfiguration();
            InitializeSprites();

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

                string charVal = ConfigurationManager.AppSettings["EasterEggCharacter"];
                CharacterName = string.IsNullOrEmpty(charVal) ? "Zool" : charVal.Trim();

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
                CharacterName = "Zool";
                IntervalSeconds = 120;
                Speed = 5;
            }
        }

        private void InitializeSprites() {
            zoolFrames = BuildZoolFrames();
            superfrogFrames = BuildSuperfrogFrames();

            currentFrames = string.Equals(CharacterName, "Superfrog", StringComparison.OrdinalIgnoreCase)
                ? superfrogFrames
                : zoolFrames;
        }

        public void TriggerRun(ScreenshotView target = null) {
            if (!Enabled) return;

            if (target == null || target.IsDisposed) {
                target = Models.ScreenshotGroupManager.Instance.ActiveScreenshot;
            }

            if (target == null || target.IsDisposed) return;

            CurrentTargetScreenshot = target;
            RunnerX = target.ClientSize.Width + 10;
            RunnerY = target.BorderSize + 1;
            CurrentFrameIndex = 0;
            IsRunning = true;

            animationTimer.Start();
        }

        private void PeriodicTimer_Tick(object sender, EventArgs e) {
            if (!IsRunning && Enabled) {
                TriggerRun();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e) {
            if (CurrentTargetScreenshot == null || CurrentTargetScreenshot.IsDisposed) {
                StopRunning();
                return;
            }

            int previousX = RunnerX;
            RunnerX -= Speed;
            CurrentFrameIndex = (CurrentFrameIndex + 1) % 4;

            // Invalidate only the region where the runner is moving to maintain 0% CPU overhead
            int invalidateLeft = Math.Max(0, RunnerX - 5);
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
        }

        public void DrawRunner(Graphics g) {
            if (!IsRunning || currentFrames == null || CurrentFrameIndex < 0 || CurrentFrameIndex >= currentFrames.Length) return;

            var frame = currentFrames[CurrentFrameIndex];
            if (frame == null) return;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(frame, RunnerX, RunnerY, SpriteWidth, SpriteHeight);
        }

        #region Sprite Definitions

        private static Bitmap[] BuildZoolFrames() {
            var palette = new Dictionary<char, Color> {
                { 'R', Color.FromArgb(192, 57, 43) },  // Crimson Suit
                { 'r', Color.FromArgb(231, 76, 60) },  // Bright Red highlight
                { 'K', Color.FromArgb(28, 40, 51) },   // Black Mask & Gloves/Boots
                { 'Y', Color.FromArgb(241, 196, 15) }, // Yellow Glowing Eyes
                { 'G', Color.FromArgb(46, 204, 113) }, // Green Belt & Headband
                { 'g', Color.FromArgb(39, 174, 96) },  // Darker Green
                { 'S', Color.FromArgb(189, 195, 199) },// Katana blade
                { 'W', Color.FromArgb(250, 250, 250) } // Katana shine
            };

            string[] f0 = {
                "..................S.....",
                "........RRRR......SS....",
                ".......RrRRRR.....SS....",
                "......RrYYYYr....SS.....",
                "......RrYYYYr...SS..GG..",
                "......RKKKKr...SS.GGg...",
                ".......KKKKr..SS.GGg....",
                ".......GGGGG..SKGGg.....",
                ".......GGGGG.SKG........",
                "......RRRRRRRRK.........",
                ".....RRRRRRRRRR.........",
                "....KKRRRRRRRR..........",
                "....KK.RRRRRRRR.........",
                "...KK..RR...RRR.........",
                "..KK...RR....RR.........",
                ".......KK.....KK........",
                "......KK.......KK.......",
                ".....KK.........KK......"
            };

            string[] f1 = {
                "..................S.....",
                "........RRRR......SS.GG.",
                ".......RrRRRR.....SS.GG.",
                "......RrYYYYr....SS.GG..",
                "......RrYYYYr...SS..G...",
                "......RKKKKr...SS.......",
                ".......KKKKr..SS........",
                ".......GGGGG.SSK........",
                ".......GGGGGSK..........",
                ".....KKRRRRRRRR.........",
                "....KKRRRRRRRRRR........",
                "...KK..RRRRRRRR.........",
                ".......RRRRRRRR.........",
                "........RRRRRR..........",
                ".........RR.RR..........",
                "........KK...KK.........",
                ".......KK.....KK........",
                "........................"
            };

            string[] f2 = {
                "..................S.....",
                "........RRRR......SS....",
                ".......RrRRRR.....SS....",
                "......RrYYYYr....SS.....",
                "......RrYYYYr...SS..GG..",
                "......RKKKKr...SS.GGg...",
                ".......KKKKr..SS.GGg....",
                ".......GGGGG..SKGGg.....",
                ".......GGGGG.SKG........",
                "......RRRRRRRRK.........",
                ".....RRRRRRRRRRK........",
                ".....RRRRRRRR..KK.......",
                "....RRRRRRRR....KK......",
                "...RRR...RR......KK.....",
                "..RR.....RR.......KK....",
                "..KK.....KK.............",
                ".KK.......KK............",
                "KK.........KK..........."
            };

            string[] f3 = {
                "...................S....",
                "........RRRR.......SS...",
                ".......RrRRRR......SS...",
                "......RrYYYYr.....SS.GG.",
                "......RrYYYYr....SS.GGg.",
                "......RKKKKr....SS.GGg..",
                ".......KKKKr...SS..G....",
                ".......GGGGG..SSK.......",
                ".......GGGGGS.K.........",
                "......RRRRRRRR..........",
                ".....RRRRRRRRRR.........",
                "....KRRRRRRRRRRK........",
                "...KK..RRRRRR..KK.......",
                "..KK....RRRR....KK......",
                ".........RR..............",
                ".........KK..............",
                "........KK...............",
                ".......KK..............."
            };

            return new Bitmap[] {
                BuildSpriteBitmap(f0, palette),
                BuildSpriteBitmap(f1, palette),
                BuildSpriteBitmap(f2, palette),
                BuildSpriteBitmap(f3, palette)
            };
        }

        private static Bitmap[] BuildSuperfrogFrames() {
            var palette = new Dictionary<char, Color> {
                { 'G', Color.FromArgb(39, 174, 96) },  // Frog Green
                { 'g', Color.FromArgb(46, 204, 113) }, // Light Frog Green
                { 'W', Color.FromArgb(255, 255, 255) },// White Eyes & Gloves
                { 'K', Color.FromArgb(20, 20, 20) },   // Black Pupils & Mouth
                { 'Y', Color.FromArgb(241, 196, 15) }, // Yellow Cape
                { 'y', Color.FromArgb(249, 231, 159) },// Cape highlight
                { 'R', Color.FromArgb(231, 76, 60) },  // Red cape lining / boots
                { 'O', Color.FromArgb(230, 126, 34) }  // Orange buckle
            };

            string[] f0 = {
                "..................YYYY..",
                ".......GGGG.......YYyy..",
                "......GWWGG......YYyy...",
                ".....GWWKG......YYyy....",
                ".....GWWKG.....YYyy.....",
                "......GGG......YYy......",
                "......GGGG....YYy.......",
                ".....GGGGGG..YYy........",
                "....WGGOGGG.YYy.........",
                "...W.GGOGGGYYy..........",
                ".....GGGGGGYy...........",
                "....GGGGGGGG............",
                "...GGG....GGG...........",
                "..GGG......GGG..........",
                "..GG........GG..........",
                ".RR..........RR.........",
                "RR............RR........",
                "RR.............RR......."
            };

            string[] f1 = {
                "................YYYYyy..",
                ".......GGGG......YYYYyy.",
                "......GWWGG.......YYYyy.",
                ".....GWWKG.........YYYy.",
                ".....GWWKG..........YYy.",
                "......GGG............Y..",
                "......GGGG..............",
                "....WWGGGGGG............",
                "...WW.GGOGGG............",
                "......GGOGGG............",
                ".....GGGGGGGG...........",
                "....GGGGGGGGGG..........",
                "....GGGG..GGGG..........",
                "...GGG......GGG.........",
                "...RR........RR.........",
                "..RR..........RR........",
                "........................",
                "........................"
            };

            string[] f2 = {
                "........................",
                ".......GGGG.............",
                "......GWWGG.............",
                ".....GWWKG.......YYy....",
                ".....GWWKG......YYyy....",
                "......GGG......YYYYy....",
                "......GGGG....YYYYyy....",
                ".....GGGGGG..YYYYyy.....",
                "....GGGGOGGWYYYYyy......",
                "...GGGGOGGG.WYYyy.......",
                "..GG.GGGGGG..YYy........",
                ".GG..GGGGGGG............",
                "GG....GG..GGG...........",
                "GG.........GGG..........",
                "RR..........GG..........",
                "RR...........RR.........",
                ".RR...........RR........",
                "...............RR......."
            };

            string[] f3 = {
                "..................YYY...",
                ".......GGGG......YYYy...",
                "......GWWGG.....YYYYy...",
                ".....GWWKG.....YYYYy....",
                ".....GWWKG.....YYYy.....",
                "......GGG.....YYYy......",
                "......GGGG...YYYy.......",
                ".....GGGGGG.YYYYy.......",
                "....WGGOGGGWYYy.........",
                "...W.GGOGGG.YY..........",
                ".....GGGGGG.............",
                "....GGGGGGGG............",
                "...GGGG..GGGG...........",
                "..GGG......GGG..........",
                "..RR........RR..........",
                ".RR..........RR.........",
                "RR............RR........",
                "........................"
            };

            return new Bitmap[] {
                BuildSpriteBitmap(f0, palette),
                BuildSpriteBitmap(f1, palette),
                BuildSpriteBitmap(f2, palette),
                BuildSpriteBitmap(f3, palette)
            };
        }

        private static Bitmap BuildSpriteBitmap(string[] rows, Dictionary<char, Color> palette) {
            var bmp = new Bitmap(SpriteWidth, SpriteHeight);
            for (int y = 0; y < Math.Min(rows.Length, SpriteHeight); y++) {
                string row = rows[y];
                for (int x = 0; x < Math.Min(row.Length, SpriteWidth); x++) {
                    char c = row[x];
                    if (palette.TryGetValue(c, out var color)) {
                        bmp.SetPixel(x, y, color);
                    } else {
                        bmp.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
            return bmp;
        }

        #endregion

        public void Dispose() {
            periodicTimer?.Stop();
            periodicTimer?.Dispose();
            animationTimer?.Stop();
            animationTimer?.Dispose();

            if (zoolFrames != null) {
                foreach (var b in zoolFrames) b?.Dispose();
            }
            if (superfrogFrames != null) {
                foreach (var b in superfrogFrames) b?.Dispose();
            }
        }
    }
}
