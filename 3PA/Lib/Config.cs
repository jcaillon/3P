using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace _3PA.Lib {

    public class ConfigObject {
        public Dictionary<string, string> ShortCuts = new Dictionary<string, string>();

        // https://msdn.microsoft.com/en-us/library/dd901590(VS.95).aspx
        [Display(Name = "User trigram", Description = "This is your user trigram duh")]
        public string UserTrigram = LocalEnv.Instance.GetTrigramFromPa();
        public bool UseDefaultValuesInsteadOfLastValuesInEditTags = false;
        public int AutoCompleteStartShowingListAfterXChar = 2;
        public bool AutoCompleteUseTabToAccept = true;
        public bool AutoCompleteUseEnterToAccept = false;
        public bool AutoCompleteShowFieldSuggestionsOnPointInput = true;
        public bool AutoCompleteShowCompleteListOnKeyInput = true;
        public bool AutoCompleteUseSpaceToInsertSnippet = true;
        public bool AutoCompleteReplaceSemicolon = true;
        public bool AutoCompleteInsertEndAfterDo = true;
        public bool AutoCompleteShowInCommentsAndStrings = true;
        public bool AutoCompleteAnimate = false;
        public int AutoCompleteIndentNbSpaces = 4;
        public int AutoCompleteShowListOfXSuggestions = 8;
        public bool AutoCompleteFixedHeight = false;
        public bool AutoCompleteAlwaysShowEmptyList = true;
        public int AutoCompleteNumberOfItemsToShowOnKeyInput = 5;
        public int AutoCompleteChangeCaseMode = 1; // 0 = inactive, 1 = upper, 2 = lower, 3 = camel
        public string ProgressProwin32ExePath = @"C:\Progress\client\v1110_dv\dlc\bin\prowin32.exe";
    }


    public static class Config {
        private static ConfigObject _instance;
        private static string _filePath;
        private static string _location = Npp.GetConfigDir();
        private static string _fileName = "settings.xml";

        public static ConfigObject Instance {
            get { return _instance ?? (_instance = Init()); }
        }

        private static ConfigObject Init() {
            _instance = new ConfigObject();

            _filePath = Path.Combine(_location, _fileName);

            if (!Directory.Exists(_location))
                Directory.CreateDirectory(_location);

            if (File.Exists(_filePath)) {
                try {
                    Class2Xml.Load(_instance, _filePath);
                } catch (Exception e) {
                    Plug.ShowErrors(e, "Error when loading settings", _filePath);
                }
            }

            SetupFileWatcher();

            return _instance;
        }

        public static void Save() {
            try  {
                if (!string.IsNullOrWhiteSpace(_filePath))
                    Class2Xml.Save(_instance, _filePath);
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error when saving settings");
            }
        }

        static FileSystemWatcher _configWatcher;
        
        private static void SetupFileWatcher() {
            var dir = Path.GetDirectoryName(_location);
            var filen = Path.GetFileName(_fileName);
            if (dir != null && filen != null) {
                _configWatcher = new FileSystemWatcher(dir, filen);
                _configWatcher.NotifyFilter = NotifyFilters.LastWrite;
                _configWatcher.Changed += configWatcher_Changed;
                _configWatcher.EnableRaisingEvents = true;
            }
        }

        private static void configWatcher_Changed(object sender, FileSystemEventArgs e) {
            Init();
        }
    }
}