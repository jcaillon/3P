using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace _3PA.Lib {
    /// <summary>
    /// </summary>
    public static class Utils {

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

        /// <summary>
        ///     Converts 16x16 bitmap into icon compatible with the Notepad++ toolbar buttons.
        ///     <para><c>Color.Fuchsia</c> is used as a 'transparency' color. </para>
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        public static Icon NppBitmapToIcon(Bitmap bitmap) {
            using (var newBmp = new Bitmap(16, 16)) {
                var g = Graphics.FromImage(newBmp);
                var colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = Color.Fuchsia;
                colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                var attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);
                //g.DrawImage(new Bitmap(@"E:\Dev\Notepad++.Plugins\NppScripts\css_logo_16x16.png"), new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                g.DrawImage(bitmap, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                return Icon.FromHandle(newBmp.GetHicon());
            }
        }
    }
}