using PixelCatcher.Views;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PixelCatcher {
    public partial class PixelCatcherMain : Form {

        public const int MYACTION_HOTKEY_ID = 1;

        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;

        private static CaptureMonitorForm singeltonCaptureForm;
        private static AboutForm singeltonAboutForm;

        #region Constructor
        public PixelCatcherMain() {
            InitializeComponent();

            RegisterGlobalHotkey();
        }
        #endregion

        private void RegisterGlobalHotkey() {
            // TODO: Make the global hotkeys configurable
            if (User32Helper.RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, Keys.PrintScreen) == 0)
            {
                // Is the error that the hotkey is registered?
                if (Marshal.GetLastWin32Error() == User32Helper.ERROR_HOTKEY_ALREADY_REGISTERED) {
                    MessageBox.Show("Cannot register hotkey.");
                }
                else {
                    throw new Win32Exception();

                }
            }
        }

        private void StartScreenCapture() {
            if(singeltonCaptureForm != null && singeltonCaptureForm.IsDisposed) {
                singeltonCaptureForm.BringToFront();
            } else {
                singeltonCaptureForm = new CaptureMonitorForm();
                singeltonCaptureForm.FormClosed += delegate { singeltonCaptureForm = null; };
                singeltonCaptureForm.Show();
            }
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID) {
                StartScreenCapture();

            }
            base.WndProc(ref m);
        }

        #region Events
        private void PixelCatcherMain_FormClosing(object sender, FormClosingEventArgs e) {
            notifyIcon1.Icon = null;
            notifyIcon1.ContextMenuStrip = null;
            notifyIcon1.Visible = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }
        #endregion

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            // StartScreenCapture();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            if (singeltonAboutForm != null && singeltonAboutForm.IsDisposed) {
                singeltonAboutForm.BringToFront();
            }
            else {
                singeltonAboutForm = new AboutForm();
                singeltonAboutForm.FormClosed += delegate { singeltonAboutForm = null; };
                singeltonAboutForm.Show();
            }
        }
    }
}
