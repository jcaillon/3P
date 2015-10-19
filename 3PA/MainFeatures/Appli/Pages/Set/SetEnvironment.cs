using System.Windows.Forms;
using YamuiFramework.Controls;
using _3PA.Images;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    public partial class SetEnvironment : YamuiPage {

        #region fields

        #endregion

        #region constructor
        public SetEnvironment() {
            InitializeComponent();

            yamuiImageButton1.BackGrndImage = ImageResources.SelectFile;
            yamuiImageButton4.BackGrndImage = ImageResources.SelectFile;
            yamuiImageButton6.BackGrndImage = ImageResources.SelectFile;
            yamuiImageButton8.BackGrndImage = ImageResources.SelectFile;
            yamuiImageButton10.BackGrndImage = ImageResources.SelectFile;

            yamuiImageButton2.BackGrndImage = ImageResources.OpenInExplorer;
            yamuiImageButton3.BackGrndImage = ImageResources.OpenInExplorer;
            yamuiImageButton5.BackGrndImage = ImageResources.OpenInExplorer;
            yamuiImageButton7.BackGrndImage = ImageResources.OpenInExplorer;
            yamuiImageButton9.BackGrndImage = ImageResources.OpenInExplorer;
        }
        #endregion

        private void yamuiComboBox1_SelectedIndexChanged(object sender, System.EventArgs e) {

        }

    }
}
