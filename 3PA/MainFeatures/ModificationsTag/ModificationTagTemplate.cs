using System.IO;
using System.Text;
using _3PA.Lib;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.ModificationsTag {

    internal class ModificationTagTemplate {
        
        #region public Properties

        public string TagOpener { get; set; }
        public string TagCloser { get; set; }
        public string TitleBlockHeader { get; set; }
        public string TitleBlockLine { get; set; }
        public string TitleBlockFooter { get; set; }

        /// <summary>
        /// Get the compilation path list
        /// </summary>
        public static ModificationTagTemplate Instance {
            get {
                if (_template == null)
                    Import();
                return _template;
            }
        }

        #endregion

        #region private fields

        private static ModificationTagTemplate _template;

        #endregion

        #region Import/export

        public static void EditTemplate() {
            Export();
            Npp.OpenFile(Config.FileModificationTags);
        }

        public static void Export() {
            if (!File.Exists(Config.FileModificationTags))
                Utils.FileWriteAllBytes(Config.FileModificationTags, DataResources.ModificationTags);
        }

        /// <summary>
        /// Read file containing the modification tags template
        /// </summary>
        public static void Import() {
            _template = new ModificationTagTemplate();
            string currentProperty = null;
            var sb = new StringBuilder();
            Utils.ForEachLine(Config.FileModificationTags, DataResources.ModificationTags, (i, line) => {
                    if (line.EndsWith("=>")) {
                        if (!string.IsNullOrEmpty(currentProperty))
                            _template.SetValueOf(currentProperty, sb.TrimEnd(2).ToString());
                        sb.Clear();
                        currentProperty = line.Replace("=>", "").Trim();
                    } else {
                        sb.AppendLine(line);
                    }
                },
                Encoding.Default);
            if (!string.IsNullOrEmpty(currentProperty))
                _template.SetValueOf(currentProperty, sb.TrimEnd(2).ToString());
        }

        #endregion

    }
}
