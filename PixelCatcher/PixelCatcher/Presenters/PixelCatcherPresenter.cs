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

        private static ICaptureDesktopView singeltonCaptureForm;
        private IAboutView singeltonAboutForm;
        private AboutInformation aboutInformation;
        private IPixelCatcherView pixelCatcherView;

        public PixelCatcherPresenter(IAboutView aboutView, IPixelCatcherView pixelCatcherView, AboutInformation aboutInformation) {
            this.aboutInformation = aboutInformation;
            this.pixelCatcherView = pixelCatcherView;
            
            singeltonAboutForm = aboutView;

            pixelCatcherView.ShowAboutBox += PixelCatcherView_ShowAboutBox;

            Application.AddMessageFilter(this);

            RegisterGlobalHotkey();
 
        }

        private void PixelCatcherView_ShowAboutBox(object sender, EventArgs e) {
            ShowAboutForm();
        }

        public void StartScreenCapture() {
            if (singeltonCaptureForm != null && !singeltonCaptureForm.IsDisposed) {
                singeltonCaptureForm.BringToFront();
            } else {

                // TODO: Add options to switch between full view and info view
                // singeltonCaptureForm = new CaptureDesktopFullView();
                singeltonCaptureForm = new CaptureDesktopView();

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
                singeltonAboutForm = new AboutView();
                singeltonAboutForm.FormClosed += delegate { singeltonAboutForm = null; };
                singeltonAboutForm.visitAboutUrl += delegate { System.Diagnostics.Process.Start(AboutInformation.websiteUrl); };
                singeltonAboutForm.Show();
            }
        }
    }
}
