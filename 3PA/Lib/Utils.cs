#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Utils.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WixToolset.Dtf.Compression.Zip;
using YamuiFramework.Helper;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.MainFeatures;
using _3PA.MainFeatures.Appli;
using _3PA.MainFeatures.FileExplorer;
using _3PA.NppCore;
using _3PA.WindowsCore;
using _3PA._Resource;

namespace _3PA.Lib {
    /*        
       #region Events

        public delegate void NotificationEvent(SCNotification notification);
        public static event NotificationEvent OnNppNotification;

        // execute events
        if (OnUpdatedUi != null) {
            OnUpdatedUi(null, new EventArgs());
        }

        #endregion
     */

    /// <summary>
    /// Class that exposes utility methods
    /// </summary>
    internal static class Utils {

        #region File manipulation wrappers

        /// <summary>
        /// File write all bytes
        /// </summary>
        public static bool FileWriteAllBytes(string path, byte[] bytes) {
            try {
                File.WriteAllBytes(path, bytes);
                return true;
            } catch (Exception e) {
                UserCommunication.Notify("Unable to write the following file :<br>" + path + "<br>Please check that you have the appropriate rights on this folder" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Write file", "Failed");
            }
            return false;
        }

        /// <summary>
        /// File write all text
        /// </summary>
        public static bool FileWriteAllText(string path, string text, Encoding encoding = null) {
            try {
                if (encoding == null)
                    encoding = Encoding.Default;
                File.WriteAllText(path, text, encoding);
                return true;
            } catch (Exception e) {
                UserCommunication.Notify("Unable to write the following file :<br>" + path + "<br>Please check that you have the appropriate rights on this folder" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Write file", "Failed");
            }
            return false;
        }

        /// <summary>
        /// File write all text
        /// </summary>
        public static bool FileAppendAllText(string path, string text, Encoding encoding = null) {
            try {
                if (encoding == null)
                    encoding = Encoding.Default;
                File.AppendAllText(path, text, encoding);
                return true;
            } catch (Exception e) {
                UserCommunication.Notify("Unable to write the following file :<br>" + path + "<br>Please check that you have the appropriate rights on this folder" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Write file", "Failed");
            }
            return false;
        }

        /// <summary>
        /// Reads all the line of either the filePath (if the file exists) or from byte array dataResources,
        /// Apply the action toApplyOnEachLine(int lineNumber, string lineString) to each line
        /// Uses encoding as the Encoding to read the file or convert the byte array to a string
        /// Uses the char # as a comment in the file (must be the first char of a line)
        /// </summary>
        public static void ForEachLine(string filePath, byte[] dataResources, Action<int, string> toApplyOnEachLine, Encoding encoding = null) {
            try {
                Exception ex = new Exception("Undetermined");
                if (!Utilities.ForEachLine(filePath, dataResources, toApplyOnEachLine, encoding ?? TextEncodingDetect.GetFileEncoding(filePath), exception => ex = exception)) {
                    ErrorHandler.ShowErrors(ex, "Error reading file", filePath);
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while reading an internal ressource!");
            }
        }

        /// <summary>
        /// Read all the text of a file in one go, same as File.ReadAllText expect it's truly a read only function
        /// </summary>
        public static string ReadAllText(string path, Encoding encoding = null) {
            try {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using (var textReader = new StreamReader(fileStream, encoding ?? TextEncodingDetect.GetFileEncoding(path))) {
                        return textReader.ReadToEnd();
                    }
                }
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Error while reading the following file : " + path);
            }
            return null;
        }

        /// <summary>
        /// Allows to hide a directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HideDirectory(string path) {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            var dirInfo = new DirectoryInfo(path);

            // See if directory has hidden flag, if not, make hidden
            if ((dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
                // Add Hidden flag    
                dirInfo.Attributes |= FileAttributes.Hidden;
            }

            return true;
        }

        /// <summary>
        /// Checks if a directory is writable as is
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static bool IsDirectoryWritable(string dirPath) {
            try {
                File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Delete a dir, recursively
        /// </summary>
        public static bool DeleteDirectory(string path, bool recursive) {
            try {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    return true;
                Directory.Delete(path, recursive);
            } catch (Exception e) {
                UserCommunication.Notify("Failed to delete the following directory :<br>" + path.ToHtmlLink() + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgHighImportance, "Delete folder", "Can't delete a folder!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        public static bool DeleteFile(string path) {
            try {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return true;
                File.Delete(path);
            } catch (Exception e) {
                UserCommunication.Notify("Failed to delete the following file :<br>" + path.ToHtmlLink() + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgHighImportance, "Delete file", "Can't delete a file!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates the directory, can apply attributes
        /// </summary>
        public static bool CreateDirectory(string path, FileAttributes attributes = FileAttributes.Directory) {
            try {
                if (Directory.Exists(path))
                    return true;
                var dirInfo = Directory.CreateDirectory(path);
                dirInfo.Attributes |= attributes;
            } catch (Exception e) {
                UserCommunication.Notify("There was a problem when i tried to create the directory:<br>" + path + "<br><br><i>Please make sure that you have the privileges to create this directory</i>" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Create directory", "Couldn't create the directory");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Move a file, ensures the user gets a feedback is something goes wrong
        /// return true if ok, false otherwise
        /// </summary>
        public static bool MoveFile(string sourceFile, string targetFile, bool silent = false) {
            try {
                if (!File.Exists(sourceFile)) {
                    if (!silent)
                        UserCommunication.Notify("There was a problem when trying to move a file, the source doesn't exist :<br>" + sourceFile, MessageImg.MsgError, "Move file", "Couldn't find source file");
                    return false;
                }
                if (sourceFile.Equals(targetFile))
                    return true;
                File.Delete(targetFile);
                File.Move(sourceFile, targetFile);
            } catch (Exception e) {
                if (!silent)
                    UserCommunication.Notify("There was a problem when i tried to write the following file:<br>" + targetFile.ToHtmlLink() + "<br><br><i>Please make sure that you have the privileges to write in the targeted directory / file</i>" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Move file", "Couldn't write target file");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Copy a file (erase existing target), ensures the user gets a feedback is something goes wrong
        /// return true if ok, false otherwise
        /// </summary>
        public static bool CopyFile(string sourceFile, string targetFile) {
            try {
                if (sourceFile.Equals(targetFile))
                    return true;
                if (!File.Exists(sourceFile)) {
                    UserCommunication.Notify("There was a problem when trying to copy a file, the source doesn't exist :<br>" + sourceFile, MessageImg.MsgError, "Copy file", "Couldn't find source file");
                    return false;
                }
                File.Copy(sourceFile, targetFile, true);
            } catch (Exception e) {
                UserCommunication.Notify("There was a problem when i tried to write the following file:<br>" + targetFile.ToHtmlLink() + "<br><br><i>Please make sure that you have the privileges to write in the targeted directory / file</i>" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Copy file", "Couldn't write target file");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Copy a directory, ensures the user gets a feedback is something goes wrong
        /// returns true if ok, false otherwise
        /// </summary>
        public static bool CopyDirectory(string sourceFolder, string targetFolder, bool deleteExistingTarget = false) {
            try {
                sourceFolder = Path.GetFullPath(sourceFolder);
                targetFolder = Path.GetFullPath(targetFolder);
                if (!Directory.Exists(sourceFolder)) {
                    UserCommunication.Notify("There was a problem when trying to copy a folder, the source doesn't exist :<br>" + sourceFolder, MessageImg.MsgError, "Copy folder", "Couldn't find source folder");
                    return false;
                }
                if (deleteExistingTarget)
                    DeleteDirectory(targetFolder, true);

                Directory.CreateDirectory(targetFolder);

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories).ToList()) {
                    var target = newPath.Replace(sourceFolder, targetFolder);
                    CreateDirectory(Path.GetDirectoryName(target));
                    File.Copy(newPath, target, true);
                }
            } catch (Exception e) {
                UserCommunication.Notify("There was a problem when i tried to copy the following folder:<br>" + targetFolder.ToHtmlLink() + "<br><br><i>Please make sure that you have the privileges to write in the targeted directory</i>" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Copy folder", "Couldn't write target folder");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Move a directory, ensures the user gets a feedback is something goes wrong
        /// returns true if ok, false otherwise
        /// </summary>
        public static bool MoveDirectory(string sourceFolder, string targetFolder, bool deleteExistingTarget = false) {
            try {
                sourceFolder = Path.GetFullPath(sourceFolder);
                targetFolder = Path.GetFullPath(targetFolder);
                if (!Directory.Exists(sourceFolder)) {
                    UserCommunication.Notify("There was a problem when trying to move a folder, the source doesn't exist :<br>" + sourceFolder, MessageImg.MsgError, "Copy folder", "Couldn't find source folder");
                    return false;
                }
                if (deleteExistingTarget)
                    DeleteDirectory(targetFolder, true);

                Directory.CreateDirectory(targetFolder);

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories).ToList()) {
                    var target = newPath.Replace(sourceFolder, targetFolder);
                    CreateDirectory(Path.GetDirectoryName(target));
                    File.Move(newPath, target);
                }
            } catch (Exception e) {
                UserCommunication.Notify("There was a problem when i tried to move the following folder:<br>" + targetFolder.ToHtmlLink() + "<br><br><i>Please make sure that you have the privileges to write in the targeted directory</i>" + "<div class='AlternatBackColor' style='padding: 5px; margin: 5px;'>\"" + e.Message + "\"</div>", MessageImg.MsgError, "Move folder", "Couldn't write target folder");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Same as Directory.EnumerateDirectories but doesn't list hidden folders
        /// </summary>
        public static IEnumerable<string> EnumerateFolders(string folderPath, string pattern, SearchOption options) {
            var hiddenDirList = new List<string>();
            foreach (var dir in Directory.EnumerateDirectories(folderPath, pattern, options)) {
                if (new DirectoryInfo(dir).Attributes.HasFlag(FileAttributes.Hidden)) {
                    hiddenDirList.Add(dir);
                }
                bool hidden = false;
                foreach (var hiddenDir in hiddenDirList) {
                    if (dir.StartsWith(hiddenDir)) {
                        hidden = true;
                    }
                }
                if (!hidden)
                    yield return dir;
            }
        }

        /// <summary>
        /// Same as Directory.EnumerateFiles but doesn't list files in hidden folders
        /// </summary>
        public static IEnumerable<string> EnumerateFiles(string folderPath, string pattern, SearchOption options) {
            foreach (var file in Directory.EnumerateFiles(folderPath, pattern, SearchOption.TopDirectoryOnly)) {
                yield return file;
            }
            if (options == SearchOption.AllDirectories) {
                foreach (var folder in EnumerateFolders(folderPath, "*", SearchOption.AllDirectories)) {
                    foreach (var file in Directory.EnumerateFiles(folder, pattern, SearchOption.TopDirectoryOnly)) {
                        yield return file;
                    }
                }
            }
        }

        #endregion

        #region File/directory selection

        /// <summary>
        /// Shows a dialog that allows the user to pick a file
        /// </summary>
        /// <param name="initialFile"></param>
        /// <param name="filter">txt files (*.txt)|*.txt|All files (*.*)|*.*</param>
        /// <returns></returns>
        public static string ShowFileSelection(string initialFile, string filter) {
            using (OpenFileDialog dialog = new OpenFileDialog {
                Multiselect = false,
                Filter = string.IsNullOrEmpty(filter) ? "All files (*.*)|*.*" : filter,
                Title = @"Select a file"
            }) {
                var initialFolder = (!File.Exists(initialFile)) ? null : Path.GetDirectoryName(initialFile);
                if (!string.IsNullOrEmpty(initialFolder) && Directory.Exists(initialFolder))
                    dialog.InitialDirectory = initialFolder;
                if (File.Exists(initialFile))
                    dialog.FileName = initialFile;
                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : string.Empty;
            }
        }

        /// <summary>
        /// Show a dialog that allows the user to pick a folder
        /// </summary>
        /// <param name="initialFolder"></param>
        /// <returns></returns>
        public static string ShowFolderSelection(string initialFolder) {
            var fsd = new FolderSelectDialog();
            if (!string.IsNullOrEmpty(initialFolder) && Directory.Exists(initialFolder))
                fsd.InitialDirectory = initialFolder;
            if (Appli.IsVisible) {
                try {
                    Win32Api.EnableWindow(Npp.Handle, false);
                    fsd.ShowDialog(Appli.Form.Handle);
                } finally {
                    Win32Api.EnableWindow(Npp.Handle, true);
                }
            } else
                fsd.ShowDialog(Npp.Handle);
            return fsd.FileName ?? string.Empty;
        }

        #endregion

        #region Link/file/directories opening

        /// <summary>
        /// Opens a file's folder and select the file in it
        /// </summary>
        /// <param name="filePath"></param>
        public static bool OpenFileInFolder(string filePath) {
            if (!File.Exists(filePath))
                return false;
            if (!Win32Api.OpenFolderAndSelectItem(Path.GetDirectoryName(filePath), filePath)) {
                string argument = "/select, \"" + filePath + "\"";
                Process.Start("explorer.exe", argument);
            }
            return true;
        }

        /// <summary>
        /// Opens a folder in the explorer
        /// </summary>
        /// <param name="folderPath"></param>
        public static bool OpenFolder(string folderPath) {
            if (!Directory.Exists(folderPath))
                return false;
            Process.Start(folderPath);
            return true;
        }

        /// <summary>
        /// Open the given link either in notepad++ (if the file extension is know)
        /// or with window (opens a folder if it is a folder, or open a file with correct program
        /// using shell extension)
        /// also works for urls
        /// </summary>
        /// <param name="link"></param>
        public static bool OpenAnyLink(string link) {
            if (string.IsNullOrEmpty(link)) return false;
            try {
                // open the file if it has a progress extension or Known extension
                if ((link.TestAgainstListOfPatterns(Config.Instance.FilesPatternNppOpenable) || link.TestAgainstListOfPatterns(Config.Instance.FilesPatternProgress)) && File.Exists(link)) {
                    Npp.Goto(link);
                    return true;
                }

                // url?
                if (new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$").Match(link).Success) {
                    Process.Start(link);
                    return true;
                }

                // open with default shell action
                if (!File.Exists(link)) {
                    if (!Directory.Exists(link))
                        return false;
                    if (OpenFolder(link))
                        return true;
                }

                var process = new ProcessStartInfo(link) {
                    UseShellExecute = true
                };
                Process.Start(process);
            } catch (Exception e) {
                if (!(e is Win32Exception))
                    ErrorHandler.LogError(e);
            }
            return true;
        }

        /// <summary>
        /// Simple click handler that opens any link as a file (either in notepad++ if the extension is known,
        /// or with the default program, or as a folder in the explorer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="htmlLinkClickedEventArgs"></param>
        public static void OpenPathClickHandler(object sender, HtmlLinkClickedEventArgs htmlLinkClickedEventArgs) {
            if (htmlLinkClickedEventArgs.Link.Contains("|")) {
                var splitted = htmlLinkClickedEventArgs.Link.Split('|');
                if (splitted.Length == 2)
                    Npp.Goto(splitted[0], int.Parse(splitted[1]));
                else
                    Npp.Goto(splitted[0], int.Parse(splitted[1]), int.Parse(splitted[2]));
                htmlLinkClickedEventArgs.Handled = true;
            } else {
                htmlLinkClickedEventArgs.Handled = OpenAnyLink(htmlLinkClickedEventArgs.Link);
            }
        }

        #endregion

        #region Image manipulation

        /// <summary>
        /// Returns a 16x16 pixels icon to use in the dockable panel
        /// </summary>
        /// <returns></returns>
        public static Icon GetIconFromImage(Image image) {
            Icon dockableIcon;
            using (Bitmap newBmp = new Bitmap(16, 16)) {
                Graphics g = Graphics.FromImage(newBmp);
                var colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap {
                    OldColor = Color.Transparent,
                    NewColor = Color.FromKnownColor(KnownColor.ButtonFace)
                };
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                g.DrawImage(image, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                dockableIcon = Icon.FromHandle(newBmp.GetHicon());
            }
            return dockableIcon;
        }

        /// <summary>
        /// Gets the image from the resource folder and resize it
        /// </summary>
        /// <param name="imgToResize"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Image ResizeImage(Image imgToResize, Size size) {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            var nPercentW = (size.Width / (float) sourceWidth);
            var nPercentH = (size.Height / (float) sourceHeight);

            var nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

            int destWidth = (int) (sourceWidth * nPercent);
            int destHeight = (int) (sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Allows to know how many files of each file type there is
        /// </summary>
        public static Dictionary<FileExt, int> GetNbFilesPerType(List<string> files) {
            Dictionary<FileExt, int> output = new Dictionary<FileExt, int>();

            foreach (var file in files) {
                FileExt fileExt;
                if (!Enum.TryParse((Path.GetExtension(file) ?? "").Replace(".", ""), true, out fileExt))
                    fileExt = FileExt.Unknow;
                if (output.ContainsKey(fileExt))
                    output[fileExt]++;
                else
                    output.Add(fileExt, 1);
            }

            return output;
        }

        /// <summary>
        /// Get the time elapsed in a human readable format
        /// </summary>
        public static string ConvertToHumanTime(TimeSpan t) {
            if (t.Hours > 0)
                return string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
            if (t.Minutes > 0)
                return string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);
            if (t.Seconds > 0)
                return string.Format("{0:D2}s", t.Seconds);
            return string.Format("{0:D3}ms", t.Milliseconds);
        }

        /// <summary>
        /// Allows to download the given file asynchronously
        /// </summary>
        public static void DownloadFile(string url, string downloadPath, AsyncCompletedEventHandler handler, Action<WebClient> setWebClient = null) {
            using (WebClient wc = new WebClient()) {
                wc.Proxy = Config.Instance.GetWebClientProxy();
                wc.Headers.Add("user-agent", Config.WebclientUserAgent);
                if (setWebClient != null)
                    setWebClient(wc);
                if (handler != null)
                    wc.DownloadFileCompleted += handler;
                wc.DownloadFileAsync(new Uri(url), downloadPath);
            }
        }

        /// <summary>
        /// Computes the MD5 hash of the given string
        /// </summary>
        public static string CalculateMd5Hash(string input) {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// register a feature's last execution datetime and prevent the user from using it too often 
        /// by setting a minimum amount of time to wait between two calls
        /// </summary>
        public static bool IsLastCallFromMoreThanXMinAgo(string name, int waitTimeInMinute, bool resetOnSpam = false) {
            // first use, no problem
            if (!Config.Instance.LastCallDateTime.ContainsKey(name)) {
                Config.Instance.LastCallDateTime.Add(name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return true;
            }
            DateTime lastCheck;
            if (!DateTime.TryParseExact(Config.Instance.LastCallDateTime[name], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastCheck)) {
                lastCheck = DateTime.MinValue;
            }

            // minimum interval not respected
            if (DateTime.Now.Subtract(lastCheck).TotalMinutes < waitTimeInMinute) {
                if (resetOnSpam) {
                    Config.Instance.LastCallDateTime[name] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return false;
            }
            Config.Instance.LastCallDateTime[name] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return true;
        }

        private static Dictionary<string, DateTime> _registeredEvents = new Dictionary<string, DateTime>();

        /// <summary>
        /// register a feature's last execution datetime and prevent the user from using it too often 
        /// by setting a minimum amount of time to wait between two calls
        /// </summary>
        public static bool IsSpamming(string featureName, int minIntervalInMilliseconds, bool resetOnSpam = false) {
            // first use, no problem
            if (!_registeredEvents.ContainsKey(featureName)) {
                _registeredEvents.Add(featureName, DateTime.Now);
                return false;
            }
            // minimum interval not respected
            if (DateTime.Now.Subtract(_registeredEvents[featureName]).TotalMilliseconds < minIntervalInMilliseconds) {
                if (resetOnSpam) {
                    _registeredEvents[featureName] = DateTime.Now;
                }
                return true;
            }
            _registeredEvents[featureName] = DateTime.Now;
            return false;
        }

        private static Dictionary<string, DateTime> _watchedFiles = new Dictionary<string, DateTime>();

        /// <summary>
        /// Allows to quickly check if a file has been modified since the last we called this method
        /// It returns false the first time we call it, then it compares the LastWriteTime for the following
        /// calls
        /// </summary>
        public static bool HasFileChanged(string filePath) {
            // first watch, no problem
            if (!_watchedFiles.ContainsKey(filePath)) {
                if (File.Exists(filePath))
                    _watchedFiles.Add(filePath, File.GetLastWriteTime(filePath));
                return false;
            }
            // then check the last write date didn't change
            if (File.Exists(filePath)) {
                if (_watchedFiles[filePath].Equals(File.GetLastWriteTime(filePath)))
                    return false;
                _watchedFiles[filePath] = File.GetLastWriteTime(filePath);
            } else {
                _watchedFiles.Remove(filePath);
            }
            return true;
        }

        /// <summary>
        /// Returns the list of control of given typ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetControlsOfType<T>(Control root) where T : Control {
            var t = root as T;
            if (t != null)
                yield return t;
            var container = root as ContainerControl;
            if (container != null)
                foreach (Control c in container.Controls)
                foreach (var i in GetControlsOfType<T>(c))
                    yield return i;
        }

        /// <summary>
        /// Simply read the content of a .log file and format it to html...
        /// </summary>
        /// <param name="logFullPath"></param>
        /// <returns></returns>
        public static string ReadAndFormatLogToHtml(string logFullPath) {
            string output = "";
            if (!string.IsNullOrEmpty(logFullPath) && File.Exists(logFullPath)) {
                output = ReadAllText(logFullPath).Replace("\n", "<br>");
                output = "<div class='ToolTipcodeSnippet'>" + output + "</div>";
            }
            return output;
        }

        /// <summary>
        /// Returns the version of a given dll
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDllVersion(string path) {
            try {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                return myFileVersionInfo.FileVersion;
            } catch (Exception) {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the name of the image to use for a particular extension
        /// </summary>
        public static string GetExtensionImage(string ext, bool exist = false) {
            if (exist)
                return "Ext" + ext;
            FileExt fileExt;
            if (!Enum.TryParse(ext, true, out fileExt))
                fileExt = FileExt.Unknow;
            return "Ext" + fileExt;
        }

        /// <summary>
        /// Returns the image from the resources
        /// </summary>
        public static Image GetImageFromStr(string typeStr) {
            Image tryImg = (Image) ImageResources.ResourceManager.GetObject(typeStr);
            return tryImg ?? ImageResources.Error;
        }

        public static string GetImageNameOf(string name) {
            return ImageResources.ResourceManager.GetObject(name) != null ? name : "Error";
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Usage : var s = GetNameOf(() => Properties.Resources.Lily);
        /// </summary>
        public static string GetNameOf<T>(Expression<Func<T>> property) {
            var me = property.Body as MemberExpression;
            return me == null ? string.Empty : me.Member.Name;
        }

        #endregion

        #region Zip Wrapper

        /// <summary>
        /// This methods extract a zip file in the given directory
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="targetDir"></param>
        public static bool ExtractAll(string filePath, string targetDir) {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;
            if (!CreateDirectory(targetDir))
                return false;
            try {
                ZipInfo zip = new ZipInfo(filePath);
                zip.Unpack(targetDir);
            } catch (Exception e) {
                ErrorHandler.ShowErrors(e, "Unzipping " + Path.GetFileName(filePath));
                return false;
            }
            return true;
        }

        #endregion

    }
}