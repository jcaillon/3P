using System;
using System.Linq;
using System.Windows.Forms;
using YamuiFramework.Controls;
using _3PA.Images;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures.Appli.Pages.Set {
    public partial class SetEnvironment : YamuiPage {

        #region fields

        #endregion

        #region constructor
        public SetEnvironment() {
            InitializeComponent();

            // buttons images
            btleft1.BackGrndImage = ImageResources.SelectFile;
            btleft2.BackGrndImage = ImageResources.SelectFile;
            btleft3.BackGrndImage = ImageResources.SelectFile;
            btleft4.BackGrndImage = ImageResources.SelectFile;
            btleft5.BackGrndImage = ImageResources.SelectFile;
            btleft6.BackGrndImage = ImageResources.SelectFile;

            btright1.BackGrndImage = ImageResources.OpenInExplorer;
            btright2.BackGrndImage = ImageResources.OpenInExplorer;
            btright3.BackGrndImage = ImageResources.OpenInExplorer;
            btright4.BackGrndImage = ImageResources.OpenInExplorer;
            btright5.BackGrndImage = ImageResources.OpenInExplorer;
            btright6.BackGrndImage = ImageResources.OpenInExplorer;

            UpdateView();
        }
        #endregion

        private void UpdateView() {
            var envList = ProgressEnv.GetList();

            if (envList.Count == 0) {
                // the user needs to add a new one

                return;
            }

            try {
                // Combo box appli
                var appliList = envList.Select(environnement => environnement.Appli).Distinct().ToList();
                if (appliList.Count > 0) {
                    cbAppli.DataSource = appliList;
                    var selectedIdx = appliList.FindIndex(str => str.EqualsCi(Config.Instance.EnvCurrentAppli));
                    cbAppli.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                    // Combo box env letter
                    var envLetterList = envList.Where(environnement => environnement.Appli.EqualsCi(cbAppli.SelectedItem.ToString())).Select(environnement => environnement.EnvLetter).ToList();
                    if (envLetterList.Count > 0) {
                        cbEnvLetter.DataSource = envLetterList;
                        selectedIdx = envLetterList.FindIndex(str => str.EqualsCi(Config.Instance.EnvCurrentEnvLetter));
                        cbEnvLetter.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                        // Combo box database
                        var databaseList = envList.First(environnement => environnement.Appli.EqualsCi(cbAppli.SelectedItem.ToString()) && environnement.EnvLetter.EqualsCi(cbEnvLetter.SelectedItem.ToString())).PfPath.Keys.ToList();
                        if (databaseList.Count > 0) {
                            cbDatabase.DataSource = databaseList;
                            selectedIdx = databaseList.FindIndex(str => str.EqualsCi(Config.Instance.EnvCurrentDatabase));
                            cbDatabase.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                        }
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error when filling comboboxes");
            }

            // determines the current item selected in the envList
            var selItem = envList.First(environnement => 
                environnement.Appli.EqualsCi(cbAppli.SelectedItem.ToString()) &&
                environnement.EnvLetter.EqualsCi(cbEnvLetter.SelectedItem.ToString()));
            if (selItem != null)
                ProgressEnv.Current = selItem;

            // fill details
            multitextbox1.Text = ProgressEnv.Current.DataBaseConnection;
            multibox2.Text = ProgressEnv.Current.ProPath;

            textbox1.Text = ProgressEnv.GetCurrentPfPath();
            textbox2.Text = ProgressEnv.Current.IniPath;
            textbox3.Text = ProgressEnv.Current.BaseLocalPath;
            textbox4.Text = ProgressEnv.Current.BaseCompilationPath;
            textbox5.Text = ProgressEnv.Current.ProwinPath;
            textbox6.Text = ProgressEnv.Current.LogFilePath;

            envLabel.Text = ProgressEnv.Current.Label;
        }

        private void cbAppli_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvCurrentAppli.Equals(cbAppli.SelectedItem.ToString()))
                return;
            Config.Instance.EnvCurrentAppli = cbAppli.SelectedItem.ToString();
            UpdateView();
        }

        private void cbEnvLetter_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvCurrentEnvLetter.Equals(cbEnvLetter.SelectedItem.ToString()))
                return;
            Config.Instance.EnvCurrentEnvLetter = cbEnvLetter.SelectedItem.ToString();
            UpdateView();
        }

        private void cbDatabase_SelectedIndexChanged(object sender, EventArgs e) {
            if (Config.Instance.EnvCurrentDatabase.Equals(cbDatabase.SelectedItem.ToString()))
                return;
            Config.Instance.EnvCurrentDatabase = cbDatabase.SelectedItem.ToString();
            UpdateView();
        }
    }
}
