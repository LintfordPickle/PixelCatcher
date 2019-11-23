using System;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class AboutView : Form, IAboutView {

        public event EventHandler visitAboutUrl;

        public AboutView() {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e) {
            Close();
        }

        private void AboutForm_MouseDoubleClick(object sender, MouseEventArgs e) {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            visitAboutUrl(this, e);
        }
    }
}
