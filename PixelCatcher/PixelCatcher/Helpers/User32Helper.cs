using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PixelCatcher {
    class User32Helper {
        
        public const uint ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

        [DllImport("user32.dll")]
        public static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);
        [DllImport("user32.dll")]
        public static extern int UnregisterHotKey(IntPtr hWnd, int id);
    }
}
