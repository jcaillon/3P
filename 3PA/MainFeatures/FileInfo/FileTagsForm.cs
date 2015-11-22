using _3PA.MainFeatures.NppInterfaceForm;

namespace _3PA.MainFeatures.FileInfo {
    public partial class FileTagsForm : NppInterfaceYamuiForm {
        public FileTagsForm() {
            InitializeComponent();
            lblTitle.Text = @"<img src='logo30x30' style='padding-right: 10px'><span class='AppliTitle'>Update file information</span>";
        }

        public void UpdateForm() {
            fileTagsPage1.UpdateInfo();
        }
    }
}
