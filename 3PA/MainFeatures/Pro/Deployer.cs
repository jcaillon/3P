using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3PA.Lib;

namespace _3PA.MainFeatures.Pro {

    public static class Deployer {



        #region public static event

        /// <summary>
        /// Called when the list of DeployTransfers is updated
        /// </summary>
        public static event Action OnDeployConfigurationUpdate;

        #endregion

        #region public static fields

        /// <summary>
        /// Get the compilation path list
        /// </summary>
        public static List<DeployRule> GetDeployRulesList {
            get {
                if (_deployRulesList == null)
                    Import();
                return _deployRulesList;
            }
        }

        #endregion
        
        #region private static fields

        private static List<DeployRule> _deployRulesList;

        #endregion

        #region public static methods

        /// <summary>
        /// Read the list of compilation Path Items,
        /// if the file is present in the Config dir, use it
        /// </summary>
        public static void Import() {
            var i = 0;
            _deployRulesList = new List<DeployRule>();
            Utils.ForEachLine(Config.FileDeployement, new byte[0], s => {
                var items = s.Split('\t');
                if (items.Length == 5) {
                    // find the TransferType from items[3]
                    DeployType type;
                    if (!Enum.TryParse(items[3].ToTitleCase(), true, out type))
                        type = DeployType.Move;

                    var obj = new DeployRule {
                        ApplicationFilter = items[0].Trim(),
                        EnvLetterFilter = items[1].Trim(),
                        SourcePattern = items[2].Trim().Replace('/', '\\'),
                        Type = type,
                        DeployTarget = items[4].Trim().Replace('/', '\\'),
                        Line = i++
                    };
                    if (!string.IsNullOrEmpty(obj.SourcePattern) && !string.IsNullOrEmpty(obj.DeployTarget)) {
                        if (obj.ApplicationFilter.Equals("*"))
                            obj.ApplicationFilter = "";
                        if (obj.EnvLetterFilter.Equals("*"))
                            obj.EnvLetterFilter = "";
                        _deployRulesList.Add(obj);
                    }
                }
            },
            Encoding.Default);

            if (OnDeployConfigurationUpdate != null)
                OnDeployConfigurationUpdate();
        }

        #endregion
        
        #region Deploy
        
        /// <summary>
        /// Deploy a given list of files (can reduce the list if there are duplicated items so it returns it)
        /// </summary>
        public static List<FileToDeploy> DeployFiles(List<FileToDeploy> deployToDo, string prolibPath, Action<int> onOneFileDone = null) {

            // make sure to transfer a given file only once at the same place (happens with .cls file since a source
            // can have several .r files generated if it is used in another classes)
            deployToDo = deployToDo
                .GroupBy(deploy => deploy.To)
                .Select(group => group.FirstOrDefault(move => Path.GetFileNameWithoutExtension(move.From ?? "").Equals(Path.GetFileNameWithoutExtension(move.Origin))) ?? group.First())
                .ToList();

            // check that every target dir exist (for copy/move deployements)
            deployToDo
                .Where(deploy => deploy.DeployType == DeployType.Copy || deploy.DeployType == DeployType.Move)
                .GroupBy(deploy => Path.GetDirectoryName(deploy.To))
                .Select(group => group.First())
                .ToNonNullList()
                .ForEach(deploy => Utils.CreateDirectory(Path.GetDirectoryName(deploy.To)));


            #region for .pl deployements, we treat them before anything else

            // for PL, we need to MOVE each file into a temporary folder with the internal structure of the .pl file,
            // then move it back where it was for further deploys...

            var plDeployements = deployToDo
                .Where(deploy => deploy.DeployType == DeployType.Pl)
                .ToNonNullList();

            // first, determine the .pl path for each deployement
            plDeployements
                .ForEach(deploy => {
                    var pos = deploy.To.LastIndexOf(".pl", StringComparison.CurrentCultureIgnoreCase);
                    if (pos >= 0)
                        deploy.PlPath = deploy.To.Substring(0, pos + 3);
                });

            // then we create a unique temporary folder for each .pl
            var dicPlToTempFolder = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var fileToDeploy in plDeployements
                .GroupBy(deploy => deploy.PlPath)
                .Select(deploys => deploys.First())
                .ToNonNullList()) {
                if (string.IsNullOrEmpty(fileToDeploy.PlPath))
                    continue;
                // ensure that the folder to the .pl file exists
                Utils.CreateDirectory(Path.GetDirectoryName(fileToDeploy.PlPath));

                // create a unique temp folder for this .pl
                if (!dicPlToTempFolder.ContainsKey(fileToDeploy.PlPath)) {
                    var uniqueTempFolder = Path.Combine(Path.GetDirectoryName(fileToDeploy.PlPath), Path.GetFileName(fileToDeploy.PlPath) + "~" + Path.GetRandomFileName());
                    dicPlToTempFolder.Add(fileToDeploy.PlPath, uniqueTempFolder);
                    Utils.CreateDirectory(uniqueTempFolder, FileAttributes.Hidden);
                }
            }

            var prolibMessage = new StringBuilder();

            // for each .pl that needs to be created...
            foreach (var pl in dicPlToTempFolder) {

                var onePlDeployements = plDeployements
                    .Where(deploy => !string.IsNullOrEmpty(deploy.PlPath) && deploy.PlPath.Equals(pl.Key))
                    .ToNonNullList();
                if (onePlDeployements.Count == 0)
                    continue;

                //  we set the temporary folder on which each file will be copied..
                // Tuple : <(base) temp directory, relative path in pl, path to .pl>
                var dicTempFolderToPl = new Dictionary<string, Tuple<string, string, string>>(StringComparer.CurrentCultureIgnoreCase);
                foreach (var fileToDeploy in onePlDeployements) {
                    if (string.IsNullOrEmpty(fileToDeploy.PlPath))
                        continue;

                    if (dicPlToTempFolder.ContainsKey(fileToDeploy.PlPath)) {
                        fileToDeploy.ToTemp = Path.Combine(
                            dicPlToTempFolder[fileToDeploy.PlPath],
                            fileToDeploy.To.Replace(fileToDeploy.PlPath, "").TrimStart('\\')
                            );

                        // If not already done, remember that the *.r code in this temp folder must be integrated to this .pl file
                        var tempSubFolder = Path.GetDirectoryName(fileToDeploy.ToTemp);
                        if (!string.IsNullOrEmpty(tempSubFolder) && !dicTempFolderToPl.ContainsKey(tempSubFolder)) {
                            dicTempFolderToPl.Add(tempSubFolder, new Tuple<string, string, string>(
                                dicPlToTempFolder[fileToDeploy.PlPath], // path of the temp dir
                                Path.GetDirectoryName(fileToDeploy.To.Replace(fileToDeploy.PlPath, "").TrimStart('\\')), // relative path in .pl
                                fileToDeploy.PlPath)); // path to the .pl file

                            // also, create the folder
                            Utils.CreateDirectory(tempSubFolder);
                        }
                    }
                }

                var prolibExe = new ProcessIo(prolibPath);

                // for each subfolder in the .pl
                foreach (var plSubFolder in dicTempFolderToPl) {

                    var onePlSubFolderDeployements = onePlDeployements
                        .Where(deploy => plSubFolder.Key.Equals(Path.GetDirectoryName(deploy.ToTemp)))
                        .ToNonNullList();
                    if (onePlSubFolderDeployements.Count == 0)
                        continue;

                    //// move the files into the temp .pl folder
                    //foreach (var deploy in onePlSubFolderDeployements) {
                    //    if (File.Exists(deploy.From))
                    //        deploy.IsOk = !string.IsNullOrEmpty(deploy.ToTemp) && Utils.MoveFile(deploy.From, deploy.ToTemp);
                    //}
                    Parallel.ForEach(onePlSubFolderDeployements, deploy => {
                        if (File.Exists(deploy.From))
                            deploy.IsOk = !string.IsNullOrEmpty(deploy.ToTemp) && Utils.MoveFile(deploy.From, deploy.ToTemp);
                    });

                    // now we just need to add the content of temp folders into the .pl
                    foreach (var kpv in dicTempFolderToPl) {
                        prolibExe.StartInfo.WorkingDirectory = kpv.Value.Item1; // base temp dir
                        prolibExe.Arguments = kpv.Value.Item3.ProQuoter() + " -create -nowarn -add " + Path.Combine(kpv.Value.Item2, "*.r").ProQuoter();
                        if (!prolibExe.TryDoWait(true))
                            prolibMessage.Append(prolibExe.ErrorOutput);
                    }

                    // move the files from the temp .pl folder back to their origin so they can be used normally
                    //foreach (var deploy in onePlSubFolderDeployements) {
                    //    deploy.IsOk = deploy.IsOk && Utils.MoveFile(deploy.ToTemp, deploy.From);
                    //}
                    Parallel.ForEach(onePlSubFolderDeployements, deploy => {
                        deploy.IsOk = deploy.IsOk && Utils.MoveFile(deploy.ToTemp, deploy.From);
                    });
                    
                }

                // delete temp folders, compress .pl
                foreach (var kpv in dicPlToTempFolder) {
                    prolibExe.StartInfo.WorkingDirectory = Path.GetDirectoryName(kpv.Key);
                    prolibExe.Arguments = kpv.Key.ProQuoter() + " -compress -nowarn";
                    if (!prolibExe.TryDoWait(true))
                        prolibMessage.Append(prolibExe.ErrorOutput);

                    Utils.DeleteDirectory(kpv.Value, true);
                }
            }

            if (prolibMessage.Length > 0)
                UserCommunication.Notify("Errors occured when trying to create/add files to the .pl file :<br>" + prolibMessage, MessageImg.MsgError, "Prolib output", "Errors");

            #endregion


            // do a deployement action for each file
            int[] nbFilesDone = { 0 };
            Parallel.ForEach(deployToDo, file => {
                DeploySingleFile(file);
                nbFilesDone[0]++;
                if (onOneFileDone != null)
                    onOneFileDone(nbFilesDone[0]);
            });

            return deployToDo;
        }

        /// <summary>
        /// Transfer a single file
        /// </summary>
        private static void DeploySingleFile(FileToDeploy file) {
            if (!file.IsOk) {
                if (File.Exists(file.From)) {
                    switch (file.DeployType) {

                        case DeployType.Copy:
                            file.IsOk = Utils.CopyFile(file.From, file.To);
                            break;

                        case DeployType.Ftp:
                            break;

                        case DeployType.Move:
                            file.IsOk = Utils.MoveFile(file.From, file.To, true);
                            break;
                    }
                }
            }
        }

        #endregion

    }

    #region DeployRule

    public class DeployRule {

        /// <summary>
        /// This compilation path applies to a given application (can be empty)
        /// </summary>
        public string ApplicationFilter { get; set; }

        /// <summary>
        /// This compilation path applies to a given Env letter (can be empty)
        /// </summary>
        public string EnvLetterFilter { get; set; }

        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string SourcePattern { get; set; }

        /// <summary>
        /// String to append to the compilation directory if the match is true
        /// </summary>
        public string DeployTarget { get; set; }

        /// <summary>
        /// The type of transfer that should occur for this compilation path
        /// </summary>
        public DeployType Type { get; set; }

        /// <summary>
        /// The line from which we read this info, allows to sort by line
        /// </summary>
        public int Line { get; set; }

    }

    #endregion

    #region DeployType

    public enum DeployType {
        Pl,
        Copy,
        Ftp,
        Move
    }

    #endregion

    #region FileToDeploy

    public class FileToDeploy {

        /// <summary>
        /// The path of input file that was originally compiled to trigger this move
        /// </summary>
        public string Origin { get; set; }

        public string From { get; set; }
        public string To { get; set; }

        /// <summary>
        /// true if the transfer went fine
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// Type de transfer
        /// </summary>
        public DeployType DeployType { get; set; }

        #region for .pl deploy type

        /// <summary>
        /// Temporary folder in which to copy the file before including it into a .pl
        /// </summary>
        public string ToTemp { get; set; }

        /// <summary>
        /// Path to the .pl file in which we need to include this file
        /// </summary>
        public string PlPath { get; set; }

        #endregion

        public FileToDeploy(string origin, string @from, string to, DeployType deployType) {
            Origin = origin;
            From = @from;
            To = to;
            DeployType = deployType;

        }
    }

    #endregion

}
