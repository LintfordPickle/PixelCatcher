using System;
using System.Windows.Forms;

namespace PixelCatcher.Views {
    public partial class AboutForm : Form {
        public AboutForm() {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e) {
            Close();
        }

        private void AboutForm_MouseDoubleClick(object sender, MouseEventArgs e) {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                VisitLink();
            } catch (Exception exception) {
                MessageBox.Show($"Unable to open link that was clicked.{Environment.NewLine}{exception.Message}");
            }
        }

        private void VisitLink() {
            System.Diagnostics.Process.Start("");

        }
    }
}
