#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using YamuiFramework.HtmlRenderer.Core.Core.Entities;
using _3PA.Html;
using _3PA.MainFeatures;

namespace _3PA.Lib {

    /*        
       #region Events

        private static event EventHandler<EventArgs> OnUpdatedUi;

        public static event EventHandler<EventArgs> UpdatedUi {
            add { OnUpdatedUi += value; }
            remove { OnUpdatedUi -= value; }
        }

        // execute events
        if (OnUpdatedUi != null) {
            OnUpdatedUi(null, new EventArgs());
        }

        #endregion
     */

    /// <summary>
    /// in an enumeration, above the item:
    /// [DisplayAttr(Name = "my stuff")]
    /// how to use it:
    /// ((DisplayAttr)myenumValue.GetAttributes()).Name)
    /// </summary>
    public class DisplayAttr : Extensions.EnumAttr {
        public string Name { get; set; }
        public string ActionText { get; set; }
    }

    /// <summary>
    /// Class that exposes utility methods
    /// </summary>
    public static class Utils {

        #region Prevent spam! That's for CCL

        private static Dictionary<string, DateTime> _registeredEvents = new Dictionary<string, DateTime>();

        /// <summary>
        /// register a feature's last execution datetime and prevent the user from using it too often 
        /// by setting a minimum amount of time to wait between two calls
        /// </summary>
        /// <param name="featureName"></param>
        /// <param name="minIntervalInMilliseconds"></param>
        /// <returns></returns>
        public static bool IsSpamming(string featureName, int minIntervalInMilliseconds) {
            // first use, no problem
            if (!_registeredEvents.ContainsKey(featureName)) {
                _registeredEvents.Add(featureName, DateTime.Now);
                return false;
            }
            // minimum interval not respected
            if (DateTime.Now.Subtract(_registeredEvents[featureName]).TotalMilliseconds < minIntervalInMilliseconds) {
                _registeredEvents[featureName] = DateTime.Now;
                return true;
            }
            _registeredEvents[featureName] = DateTime.Now;
            return false;
        }

        #endregion


        /// <summary>
        /// Checks if a directory is writable as is
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static bool IsDirectoryWritable(string dirPath) {
            try {
                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Delete a dir, recursively
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        public static void DeleteDirectory(string path, bool recursive) {
            try {
                if (!Directory.Exists(path))
                    return;

                // Delete all files and sub-folders?
                if (recursive) {
                    // Yep... Let's do this
                    var subfolders = Directory.GetDirectories(path);
                    foreach (var s in subfolders) {
                        DeleteDirectory(s, true);
                    }
                }

                // Get all files of the folder
                var files = Directory.GetFiles(path);
                foreach (var f in files) {
                    // Get the attributes of the file
                    var attr = File.GetAttributes(f);

                    // Is this file marked as 'read-only'?
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                        // Yes... Remove the 'read-only' attribute, then
                        File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                    }

                    // Delete the file
                    File.Delete(f);
                }

                // When we get here, all the files of the folder were
                // already deleted, so we just delete the empty folder
                Directory.Delete(path);
            } catch (Exception e) {
                ErrorHandler.DirtyLog(e);
                UserCommunication.Notify("Failed to delete the following directory :<br>" + path, MessageImg.MsgHighImportance, "Delete folder", "Can't delete a folder");
            }
        }

        /// <summary>
        /// Shows a dialog that allows the user to pick a file
        /// </summary>
        /// <param name="initialFile"></param>
        /// <param name="filter">txt files (*.txt)|*.txt|All files (*.*)|*.*</param>
        /// <returns></returns>
        public static string ShowFileSelection(string initialFile, string filter) {
            OpenFileDialog dialog = new OpenFileDialog {
                Multiselect = false,
                Filter = string.IsNullOrEmpty(filter) ? "All files (*.*)|*.*" : filter,
                Title = "Select a file"
            };
            var initialFolder = (!File.Exists(initialFile)) ? null : Path.GetDirectoryName(initialFile);
            if (!string.IsNullOrEmpty(initialFolder) && Directory.Exists(initialFolder))
                dialog.InitialDirectory = initialFolder;
            if (File.Exists(initialFile))
                dialog.FileName = initialFile;
            //dialog.Title = "Select a file";
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// Show a dialog that allows the user to pick a folder
        /// </summary>
        /// <param name="initialFolder"></param>
        /// <returns></returns>
        public static string ShowFolderSelection(string initialFolder) {
            // Prepare a dummy string, thos would appear in the dialog
            string dummyFileName = "Select a folder";
            SaveFileDialog sf = new SaveFileDialog {
                FileName = dummyFileName,
                Title = "Select a folder"
            };
            if (!string.IsNullOrEmpty(initialFolder) && Directory.Exists(initialFolder))
                sf.InitialDirectory = initialFolder;
            return sf.ShowDialog() == DialogResult.OK ? Path.GetDirectoryName(sf.FileName) : string.Empty;
            var fbd = new FolderBrowserDialog();
            if (Directory.Exists(initialFolder))
                fbd.SelectedPath = initialFolder;
            return fbd.ShowDialog() == DialogResult.OK ? fbd.SelectedPath : string.Empty;
        }

        /// <summary>
        /// Opens a file's folder and select the file in it
        /// </summary>
        /// <param name="filePath"></param>
        public static bool OpenFileInFolder(string filePath) {
            if (!File.Exists(filePath))
                return false;
            string argument = "/select, \"" + filePath + "\"";
            Process.Start("explorer.exe", argument);
            return true;
        }

        /// <summary>
        /// Opens a folder in the explorer
        /// </summary>
        /// <param name="folderPath"></param>
        public static bool OpenFolder(string folderPath) {
            if (!Directory.Exists(folderPath))
                return false;
            string argument = "\"" + folderPath + "\"";
            Process.Start("explorer.exe", argument);
            return true;
        }

        /// <summary>
        /// Returns the given image... but in grayscale
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Bitmap MakeGrayscale3(Bitmap original) {
            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[] {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            var attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

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

            var nPercentW = (size.Width / (float)sourceWidth);
            var nPercentH = (size.Height / (float)sourceHeight);

            var nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        /// <summary>
        /// Simply read the content of a .log file and format it to html...
        /// </summary>
        /// <param name="logFullPath"></param>
        /// <returns></returns>
        public static string ReadAndFormatLogToHtml(string logFullPath) {
            string output = "";
            if (!string.IsNullOrEmpty(logFullPath) && File.Exists(logFullPath)) {
                output = File.ReadAllText(logFullPath, TextEncodingDetect.GetFileEncoding(logFullPath)).Replace("\n", "<br>");
                output = "<div class='ToolTipcodeSnippet'>" + output + "</div>";
            }
            return output;
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

            // open the file if it has a progress extension or Known extension
            string ext = null;
            try {
                ext = Path.GetExtension(link);
            } catch (Exception e) {
                if (!(e is ArgumentException))
                    ErrorHandler.DirtyLog(e);
            }
            if (!string.IsNullOrEmpty(ext) && (Config.Instance.GlobalNppOpenableExtension.Contains(ext) || Config.Instance.GlobalProgressExtension.Contains(ext)) && File.Exists(link)) {
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
                OpenFolder(link);
            }

            var process = new ProcessStartInfo(link) {
                UseShellExecute = true
            };
            Process.Start(process);
            return true;
        }

        /// <summary>
        /// Simple click handler that opens any link as a file (either in notepad++ if the extension is known,
        /// or with the default program, or as a folder in the explorer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="htmlLinkClickedEventArgs"></param>
        public static void OpenPathClickHandler(object sender, HtmlLinkClickedEventArgs htmlLinkClickedEventArgs) {
            htmlLinkClickedEventArgs.Handled = OpenAnyLink(htmlLinkClickedEventArgs.Link);
        }


        private static DateTime Str2Date(string str) {
            DateTime MyDateTime;
            MyDateTime = new DateTime();
            MyDateTime = DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", null);
            return MyDateTime;
        }

        private static string Date2Str(DateTime mdate) {
            return mdate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static void SerializeToXml<T>(T obj, string fileName) {
            var fileStream = new FileStream(fileName, FileMode.Create);
            var ser = new XmlSerializer(typeof(T));
            ser.Serialize(fileStream, obj);
            fileStream.Close();
        }

        public static T DeserializeFromXml<T>(string fileName) {
            var deserializer = new XmlSerializer(typeof(T));
            TextReader reader = new StreamReader(fileName);
            var obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T)obj;
        }
    }
}