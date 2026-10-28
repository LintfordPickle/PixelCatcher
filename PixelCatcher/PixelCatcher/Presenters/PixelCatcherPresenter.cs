using PixelCatcher.Models;
using PixelCatcher.Views;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PixelCatcher.Presenters {
    class PixelCatcherPresenter : IPixelCatcherPresenter, IMessageFilter, IDisposable {

        public const int MYACTION_HOTKEY_ID = 1;

        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;

        private static ICaptureDesktopView singeltonCaptureForm;
        private IAboutView singeltonAboutForm;
        private readonly AboutInformation aboutInformation;
        private readonly IPixelCatcherView pixelCatcherView;
        private bool isHotkeyRegistered;

        public PixelCatcherPresenter(IAboutView aboutView, IPixelCatcherView pixelCatcherView, AboutInformation aboutInformation) {
            this.aboutInformation = aboutInformation;
            this.pixelCatcherView = pixelCatcherView;

            singeltonAboutForm = aboutView;
            if (singeltonAboutForm != null) {
                HookAboutForm(singeltonAboutForm);
            }

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
                singeltonCaptureForm = new CaptureDesktopFullView();

                var lCaptureDesktopPresenter = new CaptureDesktopPresenter(singeltonCaptureForm);

                singeltonCaptureForm.FormClosed += delegate { singeltonCaptureForm = null; };
                singeltonCaptureForm.Show();
            }
        }

        private void RegisterGlobalHotkey() {
            if (User32Helper.RegisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, Keys.PrintScreen) != 0) {
                isHotkeyRegistered = true;
            } else {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == User32Helper.ERROR_HOTKEY_ALREADY_REGISTERED) {
                    MessageBox.Show(
                        "The global hotkey (Ctrl + Shift + PrintScreen) is already in use by another application.",
                        "PixelCatcher - Hotkey Conflict",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                } else {
                    MessageBox.Show(
                        $"Failed to register hotkey (Win32 Error: {errorCode}).",
                        "PixelCatcher - Hotkey Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
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

        private void HookAboutForm(IAboutView form) {
            form.FormClosed += delegate { singeltonAboutForm = null; };
            form.visitAboutUrl += delegate {
                try {
                    System.Diagnostics.Process.Start(AboutInformation.websiteUrl);
                } catch {
                    // Ignore if default browser cannot be opened
                }
            };
        }

        public void ShowAboutForm() {
            if (singeltonAboutForm == null || singeltonAboutForm.IsDisposed) {
                singeltonAboutForm = new AboutView();
                HookAboutForm(singeltonAboutForm);
            }

            singeltonAboutForm.Show();
            singeltonAboutForm.BringToFront();
        }

        public void Dispose() {
            if (isHotkeyRegistered) {
                User32Helper.UnregisterHotKey(IntPtr.Zero, MYACTION_HOTKEY_ID);
                isHotkeyRegistered = false;
            }
            Application.RemoveMessageFilter(this);

            if (singeltonAboutForm != null && !singeltonAboutForm.IsDisposed) {
                if (singeltonAboutForm is IDisposable d) {
                    d.Dispose();
                }
                singeltonAboutForm = null;
            }
        }
    }
}
