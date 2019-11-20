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
    }
}
