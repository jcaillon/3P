using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Lib;

namespace _3PA.MainFeatures {
    public struct FileTag {
        public string Nb { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
        public string NomAppli { get; set; }
        public string Version { get; set; }
        public string Chantier { get; set; }
        public string Jira { get; set; }
    }

    public class FileTags {
        private const string FileName = "filetags.data";
        public static SpecialDictionary<List<FileTag>> Map = new SpecialDictionary<List<FileTag>>();

        private static string ConfigFile {
            get { return Path.Combine(Npp.GetConfigDir(), FileName); }
        }

        public static List<string> Keys {
            get { return Map.Keys.ToList(); }
        }

        //public static ShowFileTags GetForm { get; private set; }

        //public static bool IsShowingFileTags {
        //    get { return GetForm != null && GetForm.Visible; }
        //}

        public static bool Contains(string filename) {
            lock (Map) {
                return (!string.IsNullOrWhiteSpace(filename)) && Map.ContainsKey(filename);
            }
        }

        //public static void ShowFileTagsForm() {
        //    if (GetForm != null)
        //        GetForm.Close();

        //    GetForm = new ShowFileTags();
        //    var ans = GetForm.ShowDialog(Npp.Win32WindowNpp);
        //    if (ans == DialogResult.OK)
        //        MessageBox.Show("ok");
        //    else {
        //        MessageBox.Show("nop");
        //    }

        //    var FormTitle = string.Format("{0} {1}",
        //        AssemblyInfo.Product,
        //        AssemblyInfo.ProductTitle);

        //    Npp.SetStatusbarLabel(FormTitle);
        //}

        public static void Init() {
            lock (Map) {
                if (File.Exists(ConfigFile)) {
                    Map.Clear();
                    try {
                        Map.Load(ConfigFile);
                    } catch (Exception e) {
                        Plug.ShowErrors(e, "Error while loading file info!");
                    }
                }
                try {
                    if (!Contains("default_tag"))
                        SetFileTags("default_tag", "", "", "", "AFC", "", "", "");
                    if (!Contains("last_tag"))
                        SetFileTags("last_tag", "", "", "", "AFC", "", "", "");
                } catch (Exception e) {
                    Plug.ShowErrors(e, "Error while setting default file info!");
                }
            }
        }

        public static List<FileTag> GetFileTagsList(string filename) {
            lock (Map) {
                return Contains(filename) ? Map[filename].OrderByDescending(o => o.Nb).ToList() : new List<FileTag>();
            }
        }

        public static FileTag GetLastFileTag(string filename) {
            lock (Map) {
                return Contains(filename) ? Map[filename].Last() : new FileTag();
            }
        }

        public static FileTag GetFileTags(string filename, string nb) {
            return (filename == "last_tag" || filename == "default_tag")
                ? GetFileTagsList(filename).First()
                : GetFileTagsList(filename).Find(x => (x.Nb.Equals(nb)));
        }

        public static void SetFileTags(string filename, string nb, string date, string text, string nomAppli,
            string version, string chantier, string jira) {
            if (string.IsNullOrWhiteSpace(filename)) return;

            try {
                // filename exists
                lock (Map) {
                    if (Contains(filename)) {
                        if (filename == "last_tag" || filename == "default_tag")
                            Map[filename].Clear();

                        // modif number exists
                        var found = false;
                        var idk = 0;
                        foreach (var item in Map[filename]) {
                            if (item.Nb == nb) {
                                found = true;
                                break;
                            }
                            idk++;
                        }
                        if (found)
                            Map[filename].RemoveAt(idk);

                        Map[filename].Add(new FileTag {
                            Nb = nb,
                            Date = date,
                            Text = text,
                            NomAppli = nomAppli,
                            Version = version,
                            Chantier = chantier,
                            Jira = jira
                        });
                    } else {
                        var newlist = new List<FileTag> {
                            new FileTag {
                                Nb = nb,
                                Date = date,
                                Text = text,
                                NomAppli = nomAppli,
                                Version = version,
                                Chantier = chantier,
                                Jira = jira
                            }
                        };
                        Map.Add(filename, newlist);
                    }
                }
                Save();
            } catch (Exception e) {
                Plug.ShowErrors(e, "Error while setting a file info!");
            }
        }

        public static void Save() {
            lock (Map) {
                try {
                    Map.Save(ConfigFile);
                } catch (Exception e) {
                    Plug.ShowErrors(e, "Error while saving file info!", ConfigFile);
                }
            }
        }
    }
}