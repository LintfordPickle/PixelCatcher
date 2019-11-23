using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PixelCatcher.Presenters {
    class PixelCatcherPresenter : IPixelCatcherPresenter, IMessageFilter {

        public const int MYACTION_HOTKEY_ID = 1;

        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;

        private static CaptureMonitorForm singeltonCaptureForm;
        private static AboutForm singeltonAboutForm;
        private AboutInformation aboutInformation;

        public PixelCatcherPresenter(AboutInformation aboutInformation) {
            this.aboutInformation = aboutInformation;


            RegisterGlobalHotkey();
        }

        public void StartScreenCapture() {
            if (singeltonCaptureForm != null && !singeltonCaptureForm.IsDisposed) {
                Console.WriteLine("(1)");
                singeltonCaptureForm.BringToFront();
            } else {
                // The view shoudn't know anything about the present
                singeltonCaptureForm = new CaptureMonitorForm();

                var lCaptureDesktopPresenter = new CaptureDesktopPresenter(singeltonCaptureForm);
                
                singeltonCaptureForm.FormClosed += delegate { singeltonCaptureForm = null; };
                singeltonCaptureForm.Show();
            }
        }

        private void RegisterGlobalHotkey() {
            // TODO: Make the global hotkeys configurable
            if (User32Helper.RegisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, Keys.PrintScreen) == 0) {
                // Is the error that the hotkey is registered?
                if (Marshal.GetLastWin32Error() == User32Helper.ERROR_HOTKEY_ALREADY_REGISTERED) {
                    MessageBox.Show("Cannot register hotkey.");
                } else {
                    throw new Win32Exception();
                }
            }
        }

        public bool PreFilterMessage(ref Message m) {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID) {
                StartScreenCapture();
                return true;
            }
            return false;
        }

        public void ShowAboutForm() {
            if (singeltonAboutForm != null && singeltonAboutForm.IsDisposed) {
                singeltonAboutForm.BringToFront();
            } else {
                singeltonAboutForm = new AboutForm();
                singeltonAboutForm.FormClosed += delegate { singeltonAboutForm = null; };
                singeltonAboutForm.Show();
            }
        }

    }
}
