using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using YamuiFramework.Forms;
using YamuiFramework.Resources;

namespace _3PA.Lib {
    /// <summary>
    /// </summary>
    public static class Utils {

        /// <summary>
        /// Shows a dialog that allows the user to pick a file
        /// </summary>
        /// <param name="initialFile"></param>
        /// <param name="filter">txt files (*.txt)|*.txt|All files (*.*)|*.*</param>
        /// <returns></returns>
        public static string ShowFileSelection(string initialFile, string filter) {
            OpenFileDialog dialog = new OpenFileDialog {
                Multiselect = false,
                Filter = string.IsNullOrEmpty(filter) ? "All files (*.*)|*.*" : filter
            };
            var initialFolder = Path.GetDirectoryName(initialFile);
            if (initialFolder != null && Directory.Exists(initialFolder))
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
            string argument = "/select, \"" + folderPath + "\"";
            Process.Start("explorer.exe", argument);
            return true;
        }

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
        /// Gets the image from the resource folder and resize it
        /// </summary>
        /// <param name="imgToResize"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static Image ResizeImage(Image imgToResize, Size size) {

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
            var ser = new XmlSerializer(typeof (T));
            ser.Serialize(fileStream, obj);
            fileStream.Close();
        }

        public static T DeserializeFromXml<T>(string fileName) {
            var deserializer = new XmlSerializer(typeof (T));
            TextReader reader = new StreamReader(fileName);
            var obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T) obj;
        }
    }
}