using PixelCatcher.Views;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PixelCatcher.Models {
    public class ScreenshotGroupManager {
        private static readonly Lazy<ScreenshotGroupManager> instance =
            new Lazy<ScreenshotGroupManager>(() => new ScreenshotGroupManager());

        public static ScreenshotGroupManager Instance => instance.Value;

        // Distinct colors for groups 1 to 9 (index 0 is neutral gray for ungrouped)
        public static readonly Color[] GroupColors = new Color[] {
            Color.FromArgb(127, 140, 141), // 0: Neutral Slate Gray
            Color.FromArgb(231, 76, 60),   // 1: Coral Red
            Color.FromArgb(46, 204, 113),  // 2: Emerald Green
            Color.FromArgb(52, 152, 219),  // 3: Dodger Blue
            Color.FromArgb(241, 196, 15),  // 4: Sunflower Yellow
            Color.FromArgb(155, 89, 182),  // 5: Amethyst Purple
            Color.FromArgb(230, 126, 34),  // 6: Carrot Orange
            Color.FromArgb(26, 188, 156),  // 7: Turquoise Cyan
            Color.FromArgb(233, 30, 99),   // 8: Hot Pink
            Color.FromArgb(0, 188, 212)    // 9: Sky Cyan
        };

        private readonly List<ScreenshotView> allScreenshots = new List<ScreenshotView>();
        private readonly Dictionary<int, List<ScreenshotView>> groups = new Dictionary<int, List<ScreenshotView>>();

        public ScreenshotView ActiveScreenshot { get; private set; }

        public ScreenshotGroupManager() {
            for (int i = 1; i <= 9; i++) {
                groups[i] = new List<ScreenshotView>();
            }
        }

        public void Register(ScreenshotView screenshot) {
            if (!allScreenshots.Contains(screenshot)) {
                allScreenshots.Add(screenshot);
            }
            SetActive(screenshot);
        }

        public void Unregister(ScreenshotView screenshot) {
            allScreenshots.Remove(screenshot);

            // Remove from any group it belongs to
            foreach (var groupList in groups.Values) {
                groupList.Remove(screenshot);
            }

            if (ActiveScreenshot == screenshot) {
                ActiveScreenshot = allScreenshots.Count > 0 ? allScreenshots[allScreenshots.Count - 1] : null;
                if (ActiveScreenshot != null) {
                    ActiveScreenshot.IsActive = true;
                }
            }
        }

        public void SetActive(ScreenshotView screenshot) {
            if (ActiveScreenshot == screenshot) return;

            var previous = ActiveScreenshot;
            ActiveScreenshot = screenshot;

            if (previous != null && !previous.IsDisposed) {
                previous.IsActive = false;
            }

            if (ActiveScreenshot != null && !ActiveScreenshot.IsDisposed) {
                ActiveScreenshot.IsActive = true;
            }
        }

        public void AssignGroup(ScreenshotView screenshot, int groupNumber) {
            if (screenshot == null || groupNumber < 1 || groupNumber > 9) return;

            // Remove from all groups first
            foreach (var groupList in groups.Values) {
                groupList.Remove(screenshot);
            }

            // Add to the designated group
            groups[groupNumber].Add(screenshot);

            // Update screenshot visual state
            screenshot.GroupNumber = groupNumber;
            screenshot.GroupColor = GroupColors[groupNumber];
            screenshot.RefreshVisualState();
        }

        public void Ungroup(ScreenshotView screenshot) {
            if (screenshot == null) return;

            foreach (var groupList in groups.Values) {
                groupList.Remove(screenshot);
            }

            screenshot.GroupNumber = null;
            screenshot.GroupColor = GroupColors[0];
            screenshot.RefreshVisualState();
        }

        public void MoveGroup(ScreenshotView source, int dx, int dy) {
            if (source == null || !source.GroupNumber.HasValue || (dx == 0 && dy == 0)) return;

            int groupId = source.GroupNumber.Value;
            if (!groups.TryGetValue(groupId, out var groupList)) return;

            foreach (var member in groupList) {
                if (member != source && !member.IsDisposed) {
                    member.Location = new Point(member.Location.X + dx, member.Location.Y + dy);
                }
            }
        }

        public static Color GetGroupColor(int groupNumber) {
            if (groupNumber >= 0 && groupNumber < GroupColors.Length) {
                return GroupColors[groupNumber];
            }
            return GroupColors[0];
        }
    }
}
