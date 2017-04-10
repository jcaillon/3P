using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using _3PA.Lib;
using _3PA.NppCore;
using _3PA._Resource;

namespace _3PA.MainFeatures.Pro {

    internal static class DeploymentRules {

        #region public event

        /// <summary>
        /// Called when the list of DeployTransfers is updated
        /// </summary>
        public static event Action OnDeployConfigurationUpdate;

        #endregion

        #region public Properties

        /// <summary>
        /// Get the compilation path list
        /// </summary>
        public static List<DeployRule> GetFullDeployRulesList {
            get {
                if (_fullDeployRulesList == null)
                    Import();
                return _fullDeployRulesList;
            }
        }

        #endregion

        #region private fields

        private static List<DeployRule> _fullDeployRulesList;

        #endregion

        #region Import/export

        public static void EditRules() {
            Export();
            Npp.OpenFile(Config.FileDeploymentRules);
        }

        public static void Export() {
            if (!File.Exists(Config.FileDeploymentRules))
                Utils.FileWriteAllBytes(Config.FileDeploymentRules, DataResources.DeploymentRules);
        }

        /// <summary>
        /// Read the list of compilation Path Items,
        /// if the file is present in the Config dir, use it
        /// </summary>
        public static void Import() {
            string outputMessage;
            _fullDeployRulesList = ReadConfigurationFile(Config.FileDeploymentRules, out outputMessage);

            if (outputMessage.Length > 0)
                UserCommunication.NotifyUnique("deployRulesErrors", "The following rules are incorrect :<br><br>" + outputMessage + "<br><br>They have been ignored, please correct them " + Config.FileDeploymentRules.ToHtmlLink("here"), MessageImg.MsgHighImportance, "Error(s) reading rules file", "Rules incorrect", args => {
                    EditRules();
                    args.Handled = true;
                });
            else
                UserCommunication.CloseUniqueNotif("deployRulesErrors");

            if (OnDeployConfigurationUpdate != null)
                OnDeployConfigurationUpdate();
        }

        #endregion

        #region BuildHtmlTableForRules

        /// <summary>
        /// returns a string containing an html representation of the compilation path table
        /// </summary>
        public static string BuildHtmlTableForRules(List<DeployRule> rules) {
            var strBuilder = new StringBuilder();

            if (rules.Any()) {
                if (rules.Exists(rule => rule is DeployVariableRule)) {
                    strBuilder.Append("<h2 style='padding-top: 0px; margin-top: 0px;'>Path variables</h2>");
                    strBuilder.Append("<table width='100%;'>");
                    strBuilder.Append("<tr class='CompPathHead'><td align='center' width='9%'>Application<br>Name</td><td align='center' width='9%'>Application<br>Suffix</td><td align='center' width='13%'>Var<br>Name</td><td width='69%' align='right'>Path</td></tr>");

                    var alt = false;
                    foreach (var rule in rules.OfType<DeployVariableRule>()) {
                        strBuilder.Append("<tr><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.NameFilter) ? "*" : rule.NameFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.SuffixFilter) ? "*" : rule.SuffixFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + WebUtility.HtmlEncode(rule.VariableName) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='right'>" + (rule.Path.Length > 45 ? "..." + rule.Path.Substring(rule.Path.Length - 45) : rule.Path) + "</td></tr>");
                        alt = !alt;
                    }

                    strBuilder.Append("</table>");
                }

                if (rules.Exists(rule => rule is DeployFilterRule)) {
                    strBuilder.Append("<h2 style='padding-top: 0px; margin-top: 0px;'>Filter rules</h2>");
                    strBuilder.Append("<table width='100%;'>");
                    strBuilder.Append("<tr class='CompPathHead'><td align='center' width='5%'>Step</td><td align='center' width='9%'>Application<br>Name</td><td align='center' width='9%'>Application<br>Suffix</td><td align='center' width='8%'>Rule<br>Type</td><td width='69%' align='right'>Source path pattern</td></tr>");

                    var alt = false;
                    foreach (var rule in rules.OfType<DeployFilterRule>()) {
                        strBuilder.Append("<tr><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + rule.Step + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.NameFilter) ? "*" : rule.NameFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.SuffixFilter) ? "*" : rule.SuffixFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (rule.Include ? "Include" : "Exclude") + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='right'>" + (rule.SourcePattern.Length > 45 ? "..." + rule.SourcePattern.Substring(rule.SourcePattern.Length - 45) : rule.SourcePattern) + "</td></tr>");
                        alt = !alt;
                    }

                    strBuilder.Append("</table>");
                }

                if (rules.Exists(rule => rule is DeployTransferRule)) {
                    strBuilder.Append("<h2>Transfer rules</h2>");
                    strBuilder.Append("<table width='100%;'>");
                    strBuilder.Append("<tr class='CompPathHead'><td align='center' width='5%'>Step</td><td align='center' width='9%'>Application<br>Name</td><td align='center' width='9%'>Application<br>Suffix</td><td align='center' width='6%'>Rule<br>Type</td><td align='center' width='5%'>Next?</td><td width='33%'>Source path pattern</td><td width='33%' align='right'>Deployment target</td></tr>");

                    var alt = false;
                    foreach (var rule in rules.OfType<DeployTransferRule>()) {
                        strBuilder.Append("<tr><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + rule.Step + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.NameFilter) ? "*" : rule.NameFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (string.IsNullOrEmpty(rule.SuffixFilter) ? "*" : rule.SuffixFilter) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + rule.Type + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='center'>" + (rule.ContinueAfterThisRule ? "Yes" : "No") + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + ">" + WebUtility.HtmlEncode(rule.SourcePattern.Length > 50 ? "..." + rule.SourcePattern.Substring(rule.SourcePattern.Length - 50) : rule.SourcePattern) + "</td><td" + (alt ? " class='AlternatBackColor'" : "") + " align='right'>" + WebUtility.HtmlEncode(rule.DeployTarget.Length > 50 ? "..." + rule.DeployTarget.Substring(rule.DeployTarget.Length - 50) : rule.DeployTarget) + "</td></tr>");
                        alt = !alt;
                    }

                    strBuilder.Append("</table>");
                }
            } else {
                strBuilder.Append("<b>No rules defined yet!</b><br>Modify the rules file to get started");
            }

            return strBuilder.ToString();
        }

        #endregion

        #region GetRules

        public static List<DeployRule> GetRules(string confFilePath, string envName, string envSuffix) {

            string outputMessage;

            // Need to match the application name / suffix filter with the current env
            var rulesList = ReadConfigurationFile(confFilePath, out outputMessage)
                .Where(item => envName.RegexMatch(item.NameFilter.WildCardToRegex()) && envSuffix.RegexMatch(item.SuffixFilter.WildCardToRegex()))
                .ToNonNullList();

            // sort the rules
            rulesList.Sort((item1, item2) => {
                // exact name match first
                int compare = item2.NameFilter.EqualsCi(envName).CompareTo(item1.NameFilter.EqualsCi(envName));
                if (compare != 0)
                    return compare;

                // longer name filter first
                compare = item2.NameFilter.Length.CompareTo(item1.NameFilter.Length);
                if (compare != 0)
                    return compare;

                // exact suffix match first
                compare = item2.SuffixFilter.EqualsCi(envSuffix).CompareTo(item1.SuffixFilter.EqualsCi(envSuffix));
                if (compare != 0)
                    return compare;

                // longer suffix filter first
                compare = item2.SuffixFilter.Length.CompareTo(item1.SuffixFilter.Length);
                if (compare != 0)
                    return compare;


                // lower step first
                compare = item1.Step.CompareTo(item2.Step);
                if (compare != 0)
                    return compare;

                var itemTransfer1 = item1 as DeployTransferRule;
                var itemTransfer2 = item2 as DeployTransferRule;

                if (itemTransfer1 != null && itemTransfer2 != null) {
                    // continue first
                    compare = itemTransfer2.ContinueAfterThisRule.CompareTo(itemTransfer1.ContinueAfterThisRule);
                    if (compare != 0)
                        return compare;

                    // copy last
                    compare = itemTransfer1.Type.CompareTo(itemTransfer2.Type);
                    if (compare != 0)
                        return compare;

                    // first line in first in
                    return itemTransfer1.Line.CompareTo(itemTransfer2.Line);
                }

                // filter before transfer
                return itemTransfer1 == null ? 1 : -1;
            });

            return rulesList;
        }

        #endregion

        #region ReadConfigurationFile

        /// <summary>
        /// Reads the given rule file
        /// </summary>
        public static List<DeployRule> ReadConfigurationFile(string path, out string readingErrorsHtml) {

            var outputMessage = new StringBuilder();

            // get all the rules
            var list = new List<DeployRule>();
            Utils.ForEachLine(path, new byte[0], (lineNb, lineString) => {
                var items = lineString.Split('\t');

                int step = 0;
                if (items.Length > 1 && !int.TryParse(items[0].Trim(), out step))
                    return;

                // new transfer rule
                if (items.Length == 7) {
                    DeployType type;
                    if (!Enum.TryParse(items[3].Trim(), true, out type))
                        return;

                    var obj = DeployTransferRule.New(type);
                    obj.Source = path;
                    obj.Line = lineNb + 1;
                    obj.Step = step;
                    obj.NameFilter = items[1].Trim();
                    obj.SuffixFilter = items[2].Trim();
                    obj.ContinueAfterThisRule = items[4].Trim().EqualsCi("yes") || items[4].Trim().EqualsCi("true");
                    obj.SourcePattern = items[5].Trim();
                    obj.DeployTarget = items[6].Trim().Replace('/', '\\');

                    obj.ShouldDeployTargetReplaceDollar = obj.DeployTarget.StartsWith(":");
                    if (obj.ShouldDeployTargetReplaceDollar)
                        obj.DeployTarget = obj.DeployTarget.Remove(0, 1);

                    string errorMsg;
                    if (obj.IsValid(out errorMsg))
                        list.Add(obj);
                    if (!string.IsNullOrEmpty(errorMsg)) {
                        outputMessage.Append(errorMsg);
                        outputMessage.Append("<br>");
                    }

                } else if (items.Length == 5) {
                    // new filter rule

                    var obj = new DeployFilterRule {
                        Source = path,
                        Line = lineNb + 1,
                        Step = step,
                        NameFilter = items[1].Trim(),
                        SuffixFilter = items[2].Trim(),
                        Include = items[3].Trim().EqualsCi("+") || items[3].Trim().EqualsCi("Include"),
                        SourcePattern = items[4].Trim()
                    };
                    obj.RegexSourcePattern = obj.SourcePattern.StartsWith(":") ? obj.SourcePattern.Remove(0, 1) : obj.SourcePattern.Replace('/', '\\').WildCardToRegex();

                    if (!string.IsNullOrEmpty(obj.SourcePattern))
                        list.Add(obj);

                } else if (items.Length == 4) {
                    // new variable

                    var obj = new DeployVariableRule {
                        Source = path,
                        Line = lineNb + 1,
                        NameFilter = items[0].Trim(),
                        SuffixFilter = items[1].Trim(),
                        VariableName = items[2].Trim(),
                        Path = items[3].Trim()
                    };

                    if (!obj.VariableName.StartsWith("<") || !obj.VariableName.EndsWith(">")) {
                        outputMessage.Append("- The variable rule line n°" + (lineNb + 1) + " is incorrect, the variable should have the format <b>&lt;XXX&gt;</b><br>");
                        return;
                    }

                    if (!string.IsNullOrEmpty(obj.Path))
                        list.Add(obj);
                }
            }, Encoding.Default);

            readingErrorsHtml = outputMessage.ToString();

            return list;
        }
        
        #endregion

    }
    
    #region DeployRule

    public abstract class DeployRule {
        /// <summary>
        /// Step to which the rule applies : 0 = compilation, 1 = deployment of all files, 2+ = extra
        /// </summary>
        public int Step { get; set; }

        /// <summary>
        /// This compilation path applies to a given application (can be empty)
        /// </summary>
        public string NameFilter { get; set; }

        /// <summary>
        /// This compilation path applies to a given Env letter (can be empty)
        /// </summary>
        public string SuffixFilter { get; set; }
        
        /// <summary>
        /// The line from which we read this info, allows to sort by line
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// the full file path in which this rule can be found
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Provides a representation for this rule
        /// </summary>
        public string ToStringDescription() {
            return (Source + "|" + Line).ToHtmlLink("(rule n°" + Line + ")");
        }
    }

    public class DeployVariableRule : DeployRule {
        /// <summary>
        /// the name of the variable, format &lt;XXX&gt;
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// The path that should replace the variable &lt;XXX&gt;
        /// </summary>
        public string Path { get; set; }
    }

    public class DeployFilterRule : DeployRule {
        /// <summary>
        /// true if the rule is about including a file (+) false if about excluding (-)
        /// </summary>
        public bool Include { get; set; }

        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string SourcePattern { get; set; }

        /// <summary>
        /// Pattern to match in the source (as a regular expression)
        /// </summary>
        public string RegexSourcePattern { get; set; }
    }

    #endregion

    #region DeployTransferRule

    /// <summary>
    /// Base class for transfer rules
    /// </summary>
    public abstract class DeployTransferRule : DeployRule {

        #region Properties

        /// <summary>
        /// The type of transfer that should occur for this compilation path
        /// </summary>
        public virtual DeployType Type { get { return DeployType.Copy; } }

        /// <summary>
        /// A transfer can either apply to a file or to a folder
        /// </summary>
        public virtual DeployTransferRuleTarget TargetType { get { return DeployTransferRuleTarget.File; } }

        /// <summary>
        /// if true, this should be the last rule applied to this file
        /// </summary>
        public bool ContinueAfterThisRule { get; set; }

        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string SourcePattern { get; set; }

        /// <summary>
        /// deploy target depending on the deploytype of this rule
        /// </summary>
        public string DeployTarget { get; set; }

        /// <summary>
        /// True if the rule is directly written as a regex and we want to replace matches in the source directory in the deploy target (in that case it must start with ":")
        /// </summary>
        public bool ShouldDeployTargetReplaceDollar { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Should return true if the rule is valid
        /// </summary>
        /// <param name="error"></param>
        public virtual bool IsValid(out string error) {
            error = null;
            if (!string.IsNullOrEmpty(SourcePattern) && !string.IsNullOrEmpty(DeployTarget)) {
                return true;
            }
            error = "Line " + Line + " : Source pattern or deploy target path null";
            return false;
        }

        #endregion

        #region Factory

        public static DeployTransferRule New(DeployType type) {
            switch (type) {
                case DeployType.Prolib:
                    return new DeployTransferRuleProlib();
                case DeployType.Cab:
                    return new DeployTransferRuleCab();
                case DeployType.Zip:
                    return new DeployTransferRuleZip();
                case DeployType.DeleteInProlib:
                    return new DeployTransferRuleProlib();
                case DeployType.Ftp:
                    return new DeployTransferRuleFtp();
                case DeployType.Delete:
                    return new DeployTransferRuleDelete();
                case DeployType.Copy:
                    return new DeployTransferRuleCopy();
                case DeployType.Move:
                    return new DeployTransferRuleMove();
                case DeployType.CopyFolder:
                    return new DeployTransferRuleCopyFolder();
                case DeployType.DeleteFolder:
                    return new DeployTransferRuleDeleteFolder();
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
        }

        #endregion
    }

    #region DeployTransferRuleArchive

    /// <summary>
    /// Abstract class for PACK rules
    /// </summary>
    public abstract class DeployTransferRulePack : DeployTransferRule {

        public virtual string ArchiveExt { get { return ".arc"; } }

        public override bool IsValid(out string error) {
            if (!DeployTarget.ContainsFast(ArchiveExt)) {
                error = "Line " + Line + " : The rule has an incorrect deployment target, a " + ArchiveExt + " should be found";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #region DeployTransferRuleProlib

    /// <summary>
    /// Transfer file(s) to a .pl file
    /// </summary>
    public class DeployTransferRuleProlib : DeployTransferRulePack {

        public override DeployType Type { get { return DeployType.Prolib; } }

        public override string ArchiveExt { get { return ".pl"; } }
    }

    #endregion

    #region DeployTransferRuleZip

    /// <summary>
    /// Transfer file(s) to a .zip file
    /// </summary>
    public class DeployTransferRuleZip : DeployTransferRulePack {

        public override DeployType Type { get { return DeployType.Zip; } }

        public override string ArchiveExt { get { return ".zip"; } }
    }

    #endregion

    #region DeployTransferRuleCab

    /// <summary>
    /// Transfer file(s) to a .cab file
    /// </summary>
    public class DeployTransferRuleCab : DeployTransferRulePack {

        public override DeployType Type { get { return DeployType.Cab; } }

        public override string ArchiveExt { get { return ".cab"; } }
    }

    #endregion

    #region DeployTransferRuleDeleteInProlib

    /// <summary>
    /// Delete file(s) in a prolib file
    /// </summary>
    public class DeployTransferRuleDeleteInProlib : DeployTransferRulePack {

        public override DeployType Type { get { return DeployType.DeleteInProlib; } }

        public override string ArchiveExt { get { return ".pl"; } }
    }

    #endregion

    #region DeployTransferRuleFtp

    /// <summary>
    /// Send file(s) over FTP
    /// </summary>
    public class DeployTransferRuleFtp : DeployTransferRulePack {

        public override DeployType Type { get { return DeployType.Ftp; } }

        public override bool IsValid(out string error) {
            if (!DeployTarget.IsValidFtpAdress()) {
                error = "Line " + Line + " : The FTP rule has an incorrect deployment target, it should follow the pattern ftp://user:pass@server:port/distantpath/ (with user/pass/port being optional)";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #endregion

    #endregion

    #region DeployTransferRuleDelete

    /// <summary>
    /// Delete file(s) 
    /// </summary>
    public class DeployTransferRuleDelete : DeployTransferRule {

        public override DeployType Type { get { return DeployType.Delete; } }

        public override bool IsValid(out string error) {
            if (Step < 1) {
                error = "Line " + Line + " : The Delete rule can only applied to steps >= 1 for safety reasons";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #endregion

    #region DeployTransferRuleCopyFolder

    /// <summary>
    /// Copy folder(s) recursively
    /// </summary>
    public class DeployTransferRuleCopyFolder : DeployTransferRule {
        public override DeployType Type { get { return DeployType.CopyFolder; } }
    }

    #endregion

    #region DeployTransferRuleDeleteFolder

    /// <summary>
    /// Delete folder(s) recursively
    /// </summary>
    public class DeployTransferRuleDeleteFolder : DeployTransferRule {

        public override DeployType Type { get { return DeployType.DeleteFolder; } }

        public override bool IsValid(out string error) {
            if (Step < 1) {
                error = "Line " + Line + " : The DeleteFolder rule can only applied to steps >= 1 for safety reasons";
                return false;
            }
            return base.IsValid(out error);
        }
    }

    #endregion

    #region DeployTransferRuleCopy

    /// <summary>
    /// Copy file(s) 
    /// </summary>
    public class DeployTransferRuleCopy : DeployTransferRule {
        public override DeployType Type { get { return DeployType.Copy; } }
    }

    #endregion

    #region DeployTransferRuleMove

    /// <summary>
    /// Move file(s) 
    /// </summary>
    public class DeployTransferRuleMove : DeployTransferRule {
        public override DeployType Type { get { return DeployType.Move; } }
    }

    #endregion

    #endregion

    #region DeployTransferRuleTarget

    /// <summary>
    /// Types of deploy, used during rules sorting
    /// </summary>
    public enum DeployTransferRuleTarget : byte {
        File = 1,
        Folder = 2,
    }

    #endregion

    #region DeployType

    /// <summary>
    /// Types of deploy, used during rules sorting
    /// </summary>
    public enum DeployType : byte {
        Prolib = 1,
        Zip = 2,
        Cab = 3,
        DeleteInProlib = 4,
        Ftp = 5,
        // every item above are treated in "packs"

        Delete = 20,
        CopyFolder = 21,
        DeleteFolder = 22,

        // Copy / move should always be last
        Copy = 30,
        Move = 31
    }

    #endregion

}
