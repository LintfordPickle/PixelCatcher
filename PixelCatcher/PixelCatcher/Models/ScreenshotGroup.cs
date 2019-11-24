using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCatcher.Models {
    public class ScreenshotGroup {
        Dictionary<int, List<ScreenshotModel>> screenshotGroups;

        public ScreenshotGroup() {
            screenshotGroups = new Dictionary<int, List<ScreenshotModel>>();
            for (int i = 0; i < 9; i++) {
                screenshotGroups.Add(i, new List<ScreenshotModel>());
            }
        }

        public void applyOffsetMoveToGroup(int groupId, Point offset) {
            List<ScreenshotModel> groupList = null;
            screenshotGroups.TryGetValue(groupId, out groupList);

            if(groupList != null) {
                foreach(ScreenshotModel screenshot in groupList) {
                    var newX = screenshot.TopLeftPosition.X + offset.X;
                    var newY = screenshot.TopLeftPosition.Y + offset.Y;

                    screenshot.TopLeftPosition = new Point(newX, newY);
                }
            }
        }

        public void AddScreenshotToGroup(int groupId, ScreenshotModel screenshot) {
            List<ScreenshotModel> groupList = null;
            screenshotGroups.TryGetValue(groupId, out groupList);

            groupList.Add(screenshot);
        }

        public void RemoveScreenshotFromGroup(int groupId, ScreenshotModel screenshot) {

        }

        public void RemoveFromAllGroups(ScreenshotModel screenshot) {

        }

    }
}
