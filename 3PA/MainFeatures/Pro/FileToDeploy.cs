using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using _3PA.Lib;
using _3PA.Lib.Compression.Cab;
using _3PA.Lib.Compression.Prolib;
using _3PA.Lib.Compression.Zip;
using _3PA.Lib.Ftp;

namespace _3PA.MainFeatures.Pro {

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
        public virtual string GroupBasePath { get { return Path.GetDirectoryName(To); } }

        /// <summary>
        /// A directory that must exist or be created for this deployment
        /// </summary>
        public virtual string DirectoryThatMustExist { get { return Path.GetDirectoryName(To); } }

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
            sb.Append("<div style='padding-left: 10px'>");
            if (IsOk) {
                sb.Append("<img height='15px' src='" + Utils.GetExtensionImage((Path.GetExtension(To) ?? "").Replace(".", "")) + "'>");
            } else {
                sb.Append("<img height='15px' src='Error30x30'>Transfer failed for ");
            }
            sb.Append("(" + DeployType + ") " + To.ToHtmlLink(To.Replace(GroupBasePath, "").TrimStart('\\')));
            sb.Append(" <span style='padding-left: 8px; padding-right: 8px;'>from</span> ");
            sb.Append(Origin.ToHtmlLink(Path.GetFileName(Origin), true));
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
            return "<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage("Folder", true) + "' height='15px'><b>" + GroupBasePath.ToHtmlLink(null, true) + "</b></div>";
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Deploy this file
        /// </summary>
        protected virtual bool TryDeploy() {
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

    #region FileToDeployInPack

    /// <summary>
    /// A class for files that need to be deploy in "packs" (i.e. .zip, FTP)
    /// </summary>
    internal abstract class FileToDeployInPack : FileToDeploy {

        #region Properties

        /// <summary>
        /// Path to the pack in which we need to include this file
        /// </summary>
        public string PackPath { get; protected set; }

        /// <summary>
        /// The relative path of the file within the pack
        /// </summary>
        public string RelativePathInPack { get; set; }

        /// <summary>
        /// A directory that must exist or be created for this deployment
        /// </summary>
        public override string DirectoryThatMustExist { get { return Path.GetDirectoryName(PackPath); } }

        /// <summary>
        /// Path to the pack file
        /// </summary>
        public override string GroupBasePath { get { return PackPath ?? To; } }

        /// <summary>
        /// Extension of the archive file
        /// </summary>
        public virtual string PackExt { get { return ".arc"; } }

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        protected FileToDeployInPack(string targetPath) : base(targetPath) { }

        #endregion

        #region Methods

        public override FileToDeploy Set(string origin, string @from, string to) {
            var pos = to.LastIndexOf(PackExt, StringComparison.CurrentCultureIgnoreCase);
            if (pos >= 0) {
                pos += PackExt.Length;
                PackPath = to.Substring(0, pos);
                RelativePathInPack = to.Substring(pos + 1);
            }
            return base.Set(origin, @from, to);
        }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public virtual IPackager NewArchive(Deployer deployer) {
            return null;
        }

        /// <summary>
        /// Saves an exception in the deploy error 
        /// </summary>
        public virtual void RegisterArchiveException(Exception e) {
            IsOk = false;
            DeployError = "Problem with the target pack " + PackPath.ProQuoter() + " : \"" + e.Message + "\"";
        }

        /// <summary>
        /// Allows to check the source file before putting this fileToDeploy in a pack
        /// </summary>
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
            return "<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage(PackExt.Replace(".", "")) + "' height='15px'><b>" + GroupBasePath.ToHtmlLink(null, true) + "</b></div>";
        }

        #endregion
    }

    #region FileToDeployCab

    internal class FileToDeployCab : FileToDeployInPack {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Cab; } }

        public override string PackExt { get { return ".cab"; } }

        public FileToDeployCab(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IPackager NewArchive(Deployer deployer) {
            return new CabPackager(PackPath);
        }
    }

    #endregion

    #region FileToDeployProlib

    internal class FileToDeployProlib : FileToDeployInPack {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Prolib; } }

        public override string PackExt { get { return ".pl"; } }

        public FileToDeployProlib(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IPackager NewArchive(Deployer deployer) {
            return new ProlibPackager(PackPath, deployer.ProlibPath);
        }

    }

    #endregion

    #region FileToDeployZip

    internal class FileToDeployZip : FileToDeployInPack {

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Zip; } }

        public override string PackExt { get { return ".zip"; } }

        public FileToDeployZip(string targetPath) : base(targetPath) { }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IPackager NewArchive(Deployer deployer) {
            return new ZipPackager(PackPath);
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

    #region FileToDeployFtp

    internal class FileToDeployFtp : FileToDeployInPack {

        #region Properties

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Ftp; } }

        /// <summary>
        /// Path to the pack file
        /// </summary>
        public override string GroupBasePath { get { return PackPath ?? To; } }

        /// <summary>
        /// A directory that must exist or be created for this deployment
        /// </summary>
        public override string DirectoryThatMustExist { get { return null; } }

        #endregion

        #region Life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public FileToDeployFtp(string targetPath) : base(targetPath) { }

        #endregion

        private string _host;
        private int _port;
        private string _userName;
        private string _passWord;

        #region Methods

        public override FileToDeploy Set(string origin, string @from, string to) {
            // parse our uri
            var regex = new Regex(@"^(ftps?:\/\/([^:\/@]*)?(:[^:\/@]*)?(@[^:\/@]*)?(:[^:\/@]*)?)(\/.*)$");
            var match = regex.Match(to.Replace("\\", "/"));
            if (match.Success) {
                PackPath = match.Groups[1].Value;
                RelativePathInPack = match.Groups[6].Value;
                if (!string.IsNullOrEmpty(match.Groups[4].Value)) {
                    _userName = match.Groups[2].Value;
                    _passWord = match.Groups[3].Value.Trim(':');
                    _host = match.Groups[4].Value.Trim('@');
                    if (!int.TryParse(match.Groups[5].Value.Trim(':'), out _port))
                        _port = -1;
                } else {
                    _host = match.Groups[2].Value;
                    if (!int.TryParse(match.Groups[3].Value.Trim(':'), out _port))
                        _port = -1;
                }
            }
            return base.Set(origin, @from, to);
        }

        /// <summary>
        /// Returns a new archive info
        /// </summary>
        public override IPackager NewArchive(Deployer deployer) {
            return new FtpPackager(_host, _port, _userName, _passWord, PackPath);
        }

        /// <summary>
        /// Saves an exception in the deploy error 
        /// </summary>
        public override void RegisterArchiveException(Exception e) {
            IsOk = false;
            DeployError = "Problem with the FTP " + PackPath.ProQuoter() + " : \"" + e.Message + "\"";
        }

        /// <summary>
        /// Representation of a group of this type
        /// </summary>
        /// <returns></returns>
        public override string GroupHeaderToString() {
            return "<div style='padding-bottom: 5px;'><img src='" + Utils.GetExtensionImage("Ftp", true) + "' height='15px'><b>" + GroupBasePath.ToHtmlLink(null, true) + "</b></div>";
        }

        #endregion
        
    }

    #endregion

    #endregion

    #region FileToDeployDelete

    internal class FileToDeployDelete : FileToDeploy {

        #region Properties

        /// <summary>
        /// A directory that must exist or be created for this deployment
        /// </summary>
        public override string DirectoryThatMustExist { get { return null; } }

        /// <summary>
        /// Type of transfer
        /// </summary>
        public override DeployType DeployType { get { return DeployType.Delete; } }

        #endregion

        public FileToDeployDelete(string targetPath) : base(targetPath) { }

        #region Methods

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

        #endregion

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

}
