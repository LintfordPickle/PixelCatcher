using System;
using System.Windows.Forms;
using PixelCatcher.Views;

namespace PixelCatcher {
    public partial class PixelCatcherView : ApplicationContext, IPixelCatcherView {

        private System.ComponentModel.IContainer components = null;

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;

        public event EventHandler ShowAboutBox;

        public PixelCatcherView() {
            InitializeComponent();

        }

        public ApplicationContext GetApplicationContext() {
            return this;
        }

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PixelCatcherView));
            this.notifyIcon1 = new NotifyIcon(this.components);
            this.contextMenuStrip1 = new ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "PixelCatcher";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 70);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void PixelCatcherMain_FormClosing(object sender, FormClosingEventArgs e) {
            notifyIcon1.Icon = null;
            notifyIcon1.ContextMenuStrip = null;
            notifyIcon1.Visible = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            ExitThread();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowAboutBox?.Invoke(this, e);
        }
    }
}
