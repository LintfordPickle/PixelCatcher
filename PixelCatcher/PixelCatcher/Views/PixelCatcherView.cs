using PixelCatcher.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class PixelCatcherView : ApplicationContext, IPixelCatcherView {

        private System.ComponentModel.IContainer components = null;

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem openSpritesFolderToolStripMenuItem;

        public event EventHandler ShowAboutBox;

        public PixelCatcherView() {
            InitializeComponent();

        }

        public ApplicationContext GetApplicationContext() {
            return this;
        }

        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            exitToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            openSpritesFolderToolStripMenuItem = new ToolStripMenuItem();
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            try {
                notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            } catch {
                notifyIcon1.Icon = SystemIcons.Application;
            }
            notifyIcon1.Text = "PixelCatcher";
            notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
            aboutToolStripMenuItem,
            openSpritesFolderToolStripMenuItem,
            new ToolStripSeparator(),
            exitToolStripMenuItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(190, 92);
            // 
            // openSpritesFolderToolStripMenuItem
            // 
            openSpritesFolderToolStripMenuItem.Name = "openSpritesFolderToolStripMenuItem";
            openSpritesFolderToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            openSpritesFolderToolStripMenuItem.Text = "Open Sprites Folder...";
            openSpritesFolderToolStripMenuItem.Click += new System.EventHandler(openSpritesFolderToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += new System.EventHandler(exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += new System.EventHandler(aboutToolStripMenuItem_Click);
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                if (notifyIcon1 != null) {
                    notifyIcon1.Visible = false;
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void PixelCatcherMain_FormClosing(object sender, FormClosingEventArgs e) {
            notifyIcon1.Icon = null;
            notifyIcon1.ContextMenuStrip = null;
            notifyIcon1.Visible = false;
        }

        private void openSpritesFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                string dir = PixelCatcher.Helpers.AmigaSpriteRunner.GetOrCreateSpritesDirectory();
                if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir)) {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                        FileName = dir,
                        UseShellExecute = true
                    });
                }
            } catch { }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            ExitThread();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowAboutBox?.Invoke(this, e);
        }
    }
}
