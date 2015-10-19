using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using _3PA.Interop;

namespace _3PA.Lib {
    /// <summary>
    /// </summary>
    public static class Utils {

        /// <summary>
        /// Shows a dialog that allows the user to pick a file
        /// </summary>
        /// <param name="initialFile"></param>
        /// <returns></returns>
        public static string ShowFileSelection(string initialFile) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            var initialFolder = Path.GetDirectoryName(initialFile);
            if (Directory.Exists(initialFolder))
                dialog.InitialDirectory = initialFolder;
            if (File.Exists(initialFile))
                dialog.FileName = initialFile;
            dialog.Title = "Select a text file";
            if (dialog.ShowDialog() == DialogResult.OK) {
                return dialog.FileName;
            }
            return string.Empty;
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
            if (fbd.ShowDialog() == DialogResult.OK) {
                return fbd.SelectedPath;
            }
            return string.Empty;
        }

        /// <summary>
        /// Opens a file's folder and select the file in it
        /// </summary>
        /// <param name="filePath"></param>
        public static void OpenFileInFolder(string filePath) {
            if (!File.Exists(filePath))
                return;
            string argument = "/select, \"" + filePath + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
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