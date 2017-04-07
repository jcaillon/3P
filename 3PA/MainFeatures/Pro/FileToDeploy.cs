using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WixToolset.Dtf.Compression;
using WixToolset.Dtf.Compression.Cab;
using WixToolset.Dtf.Compression.Zip;
using _3PA.Lib;
using _3PA.Lib.Compression.Pl;
using _3PA.Lib.Ftp;

namespace _3PA.MainFeatures.Pro {

    #region FileToDeploy

    /// <summary>
    /// Represents a file that needs to be deployed
    /// </summary>
    public class FileToDeploy {

        #region Properties

        /// <summary>
        /// The path of input file that was originally compiled to trigger this move (can be equal to From)
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Need to deploy this file FROM this path
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Need to deploy this file TO this path
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// true if the transfer went fine
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// target path computed from the deployment rules
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public virtual DeployType DeployType { get { return DeployType.Copy; } }

        /// <summary>
        /// Null if no errors, otherwise it contains the description of the error that occurred for this file
        /// </summary>
        public string DeployError { get; set; }

        /// <summary>
        /// Return true if the deployment type can be parallelized
        /// </summary>
        public virtual bool CanParallelizeDeploy { get { return false; } }

        /// <summary>
        /// Directory name of To
        /// </summary>
        public virtual string ToBasePath { get { return Path.GetDirectoryName(To); } }

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetPath"></param>
        public FileToDeploy(string targetPath) {
            TargetPath = targetPath;
        }

        #endregion

        #region Methods

        public virtual FileToDeploy Set(string origin, string from, string to) {
            Origin = origin;
            From = from;
            To = to;
            return this;
        }

        /// <summary>
        /// Returns a "copy" (only target path and those inputs are copied) if this object, setting properties in the meantime
        /// </summary>
        public virtual FileToDeploy Copy(string origin, string from, string to) {
            return New(DeployType, TargetPath).Set(origin, from, to);
        }

        /// <summary>
        /// Deploy this file
        /// </summary>
        public virtual bool DeploySelf() {
            if (IsOk)
                return false;
            IsOk = TryDeploy();
            return IsOk;
        }

        /// <summary>
        /// A representation of this file to deploy
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("<div>");
            if (IsOk) {
                sb.Append("<img height='15px' src='" + Utils.GetExtensionImage((Path.GetExtension(To) ?? "").Replace(".", "")) + "'>");
            } else {
                sb.Append("<img height='15px' src='Error30x30'>Transfer failed for ");
            }
            sb.Append("(" + DeployType + ") " + To.ToHtmlLink());
            if (!IsOk) {
                sb.Append("<br>" + DeployError);
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Representation of a group of this type
        /// </summary>
        /// <returns></returns>
        public virtual string GroupHeaderToString() {
            return "<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage("Folder", true) + "' height='15px'><b>" + ToBasePath.ToHtmlLink(null, true) + "</b></div>";
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Deploy this file
        /// </summary>
        protected virtual bool TryDeploy() {
            return true;
        }

        /// <summary>
        /// Creates the directory, can apply attributes
        /// </summary>
        protected bool CreateDirectory(string path, FileAttributes attributes = FileAttributes.Directory) {
            try {
                if (Directory.Exists(path)) {
                    return true;
                }
                var dirInfo = Directory.CreateDirectory(path);
                dirInfo.Attributes |= attributes;
            } catch (Exception e) {
                DeployError = "Couldn't create directory " + path.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }

        #endregion

        #region Factory

        public static FileToDeploy New(DeployType deployType, string targetPath) {
            switch (deployType) {
                case DeployType.Prolib:
                    return new FileToDeployProlib(targetPath);
                case DeployType.Zip:
                    return new FileToDeployZip(targetPath);
                case DeployType.DeleteInProlib:
                    return new FileToDeployDeleteInProlib(targetPath);
                case DeployType.Ftp:
                    return new FileToDeployFtp(targetPath);
                case DeployType.Delete:
                    return new FileToDeployDelete(targetPath);
                case DeployType.Copy:
                    return new FileToDeployCopy(targetPath);
                case DeployType.Move:
                    return new FileToDeployMove(targetPath);
                case DeployType.Cab:
                    return new FileToDeployCab(targetPath);
                default:
                    throw new ArgumentOutOfRangeException("deployType", deployType, null);
            }
        }

        #endregion
    }

    #region FileToDeployArchive

    internal abstract class FileToDeployArchive : FileToDeploy {

        #region Properties

        /// <summary>
        /// Path to the .pl or .zip file in which we need to include this file
        /// </summary>
        public string ArchivePath { get; set; }

        /// <summary>
        /// The relative path of the file within the archive
        /// </summary>
        public string RelativePathInArchive { get; set; }

        /// <summary>
        /// Path to the archive file
        /// </summary>
        public override string ToBasePath { get { return ArchivePath ?? To; } }

        /// <summary>
        /// Extension of the archive file
        /// </summary>
        public virtual string ArchiveExt { get { return ".arc"; } }

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        protected FileToDeployArchive(string targetPath) : base(targetPath) { }

        #endregion

        #region Methods

        public override FileToDeploy Set(string origin, string @from, string to) {
            var pos = to.LastIndexOf(ArchiveExt, StringComparison.CurrentCultureIgnoreCase);
            if (pos >= 0) {
                pos += ArchiveExt.Length;
                ArchivePath = to.Substring(0, pos);
                RelativePathInArchive = to.Substring(pos + 1);
            }
            return base.Set(origin, @from, to);
        }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public virtual IArchiveInfo NewArchive(Deployer deployer) {
            return null;
        }

        /// <summary>
        /// Saves an exception in the deploy error 
        /// </summary>
        public void RegisterArchiveException(Exception e) {
            IsOk = false;
            DeployError = "Problem with the target archive " + ArchivePath.ProQuoter() + " : \"" + e.Message + "\"";
        }

        public bool IfFromFileExists() {
            if (!File.Exists(From)) {
                DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Representation of a group of this type
        /// </summary>
        /// <returns></returns>
        public override string GroupHeaderToString() {
            return "<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage(ArchiveExt.Replace(".", "")) + "' height='15px'><b>" + ToBasePath.ToHtmlLink(null, true) + "</b></div>";
        }

        #endregion
    }

    #region FileToDeployCab

    internal class FileToDeployCab : FileToDeployArchive {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Cab; } }

        public override string ArchiveExt { get { return ".cab"; } }

        public FileToDeployCab(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IArchiveInfo NewArchive(Deployer deployer) {
            return new CabInfo(ArchivePath);
        }
    }

    #endregion

    #region FileToDeployProlib

    internal class FileToDeployProlib : FileToDeployArchive {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Prolib; } }

        public override string ArchiveExt { get { return ".pl"; } }

        public FileToDeployProlib(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IArchiveInfo NewArchive(Deployer deployer) {
            return new PlInfo(ArchivePath, deployer.ProlibPath);
        }

    }

    #endregion

    #region FileToDeployZip

    internal class FileToDeployZip : FileToDeployArchive {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Zip; } }

        public override string ArchiveExt { get { return ".zip"; } }

        public FileToDeployZip(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IArchiveInfo NewArchive(Deployer deployer) {
            return new ZipInfo(ArchivePath);
        }
    }

    #endregion

    #region FileToDeployDeleteInProlib

    internal class FileToDeployDeleteInProlib : FileToDeployProlib {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.DeleteInProlib; } }

        public FileToDeployDeleteInProlib(string targetPath) : base(targetPath) { }
    }

    #endregion

    #endregion

    #region FileToDeployDelete

    internal class FileToDeployDelete : FileToDeploy {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Delete; } }

        public FileToDeployDelete(string targetPath) : base(targetPath) { }

        protected override bool TryDeploy() {
            try {
                if (string.IsNullOrEmpty(To) || !File.Exists(To))
                    return true;
                File.Delete(To);
            } catch (Exception e) {
                DeployError = "Couldn't delete " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }

    }

    #endregion

    #region FileToDeployCopy

    internal class FileToDeployCopy : FileToDeploy {

        /// <summary>
        /// Return true if the deployment type can be parallelized
        /// </summary>
        public override bool CanParallelizeDeploy { get { return true; } }

        /// <summary>
        /// This can be set to true for a file deployed during step 0 (compilation), if the last 
        /// deployment is a Copy, we make it a Move because this allows us to directly compile were
        /// we need to finally move it instead of compiling then copying...
        /// </summary>
        public bool FinalDeploy { get; set; }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return FinalDeploy ? DeployType.Move : DeployType.Copy; } }

        public FileToDeployCopy(string targetPath) : base(targetPath) { }

        protected override bool TryDeploy() {
            try {
                if (From.Equals(To))
                    return true;
                if (!File.Exists(From)) {
                    DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                    return false;
                }
                if (!CreateDirectory(ToBasePath)) {
                    return false;
                }
                File.Delete(To);
                File.Copy(From, To);
            } catch (Exception e) {
                DeployError = "Couldn't copy " + From.ProQuoter() + " to  " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }
    }

    #endregion

    #region FileToDeployMove

    internal class FileToDeployMove : FileToDeploy {

        /// <summary>
        /// Return true if the deployment type can be parallelized
        /// </summary>
        public override bool CanParallelizeDeploy { get { return true; } }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Move; } }

        public FileToDeployMove(string targetPath) : base(targetPath) { }

        protected override bool TryDeploy() {
            try {
                if (From.Equals(To))
                    return true;
                if (!File.Exists(From)) {
                    DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                    return false;
                }
                if (!CreateDirectory(ToBasePath)) {
                    return false;
                }
                File.Delete(To);
                File.Move(From, To);
            } catch (Exception e) {
                DeployError = "Couldn't move " + From.ProQuoter() + " to  " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }
            return true;
        }
    }

    #endregion

    #region FileToDeployFtp

    internal class FileToDeployFtp : FileToDeploy {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Ftp; } }

        /// <summary>
        /// Path to the archive file
        /// </summary>
        public override string ToBasePath { get { return _serverUri ?? To; } }

        private string _serverUri;

        public FileToDeployFtp(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Representation of a group of this type
        /// </summary>
        /// <returns></returns>
        public override string GroupHeaderToString() {
            return "<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage("Ftp", true) + "' height='15px'><b>" + ToBasePath.ToHtmlLink(null, true) + "</b></div>";
        }

        /// <summary>
        /// Sends a file to a ftp(s) server : EASY MODE, connects, create the directories...
        /// Utils.SendFileToFtp(@"D:\Profiles\jcaillon\Downloads\function_forward_sample.p", "ftp://cnaf049:sopra100@rs28.lyon.fr.sopra/cnaf/users/cnaf049/vm/jca/derp/yolo/test.p");
        /// </summary>
        protected override bool TryDeploy() {
            if (string.IsNullOrEmpty(From) || !File.Exists(From)) {
                DeployError = "The source file " + From.ProQuoter() + " doesn't exist";
                return false;
            }
            try {
                // parse our uri
                var regex = new Regex(@"^(ftps?:\/\/([^:\/@]*)?(:[^:\/@]*)?(@[^:\/@]*)?(:[^:\/@]*)?)(\/.*)$");
                var match = regex.Match(To.Replace("\\", "/"));
                if (!match.Success) {
                    DeployError = "Invalid URI for the targeted FTP : " + To.ProQuoter();
                    return false;
                }

                _serverUri = match.Groups[1].Value;
                var distantPath = match.Groups[6].Value;
                string userName = null;
                string passWord = null;
                string server;
                int port;
                if (!string.IsNullOrEmpty(match.Groups[4].Value)) {
                    userName = match.Groups[2].Value;
                    passWord = match.Groups[3].Value.Trim(':');
                    server = match.Groups[4].Value.Trim('@');
                    if (!int.TryParse(match.Groups[5].Value.Trim(':'), out port))
                        port = -1;
                } else {
                    server = match.Groups[2].Value;
                    if (!int.TryParse(match.Groups[3].Value.Trim(':'), out port))
                        port = -1;
                }

                FtpsClient ftp;
                if (!_ftpClients.ContainsKey(_serverUri))
                    _ftpClients.Add(_serverUri, new FtpsClient());
                ftp = _ftpClients[_serverUri];

                // try to connect!
                if (!ftp.Connected) {
                    if (!ConnectFtp(ftp, userName, passWord, server, port))
                        return false;
                }

                // dispose of the ftp on shutdown
                Plug.OnShutDown += DisconnectFtp;

                try {
                    ftp.PutFile(From, distantPath);
                } catch (Exception) {

                    // might be disconnected??
                    try {
                        ftp.GetCurrentDirectory();
                    } catch (Exception) {
                        if (!ConnectFtp(ftp, userName, passWord, server, port))
                            return false;
                    }

                    // try to create the directory and then push the file again
                    ftp.MakeDir((Path.GetDirectoryName(distantPath) ?? "").Replace('\\', '/'), true);
                    ftp.PutFile(From, distantPath);
                }
            } catch (Exception e) {
                DeployError = "Error sending " + From.ProQuoter() + " to FTP " + To.ProQuoter() + " : \"" + e.Message + "\"";
                return false;
            }

            return true;
        }

        private bool ConnectFtp(FtpsClient ftp, string userName, string passWord, string server, int port) {
            NetworkCredential credential = null;
            if (!string.IsNullOrEmpty(userName))
                credential = new NetworkCredential(userName, passWord);

            var modes = new List<EsslSupportMode>();
            typeof(EsslSupportMode).ForEach<EsslSupportMode>((s, l) => { modes.Add((EsslSupportMode)l); });

            ftp.DataConnectionMode = EDataConnectionMode.Passive;
            while (!ftp.Connected && ftp.DataConnectionMode == EDataConnectionMode.Passive) {
                foreach (var mode in modes.OrderByDescending(mode => mode)) {
                    try {
                        var curPort = port > -1 ? port : ((mode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21);
                        ftp.Connect(server, curPort, credential, mode, 1800);
                        ftp.Connected = true;
                        break;
                    } catch (Exception) {
                        //ignored
                    }
                }
                ftp.DataConnectionMode = EDataConnectionMode.Active;
            }

            // failed?
            if (!ftp.Connected) {
                DeployError = "Failed to connect to a FTP server with : " + string.Format(@"Username : {0}, Password : {1}, Host : {2}, Port : {3}", userName ?? "none", passWord ?? "none", server, port == -1 ? 21 : port);
                return false;
            }

            return true;
        }

        #region Static for FTP

        private static Dictionary<string, FtpsClient> _ftpClients = new Dictionary<string, FtpsClient>();

        private static void DisconnectFtp() {
            foreach (var ftpsClient in _ftpClients) {
                ftpsClient.Value.Close();
            }
            _ftpClients.Clear();
        }

        #endregion

    }

    #endregion

    #endregion

}
