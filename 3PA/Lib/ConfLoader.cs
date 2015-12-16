using System;
using System.IO;
using System.Text;
using _3PA.MainFeatures;

namespace _3PA.Lib {
    /// <summary>
    /// This class allows to easily import/export into .conf files
    /// </summary>
    public class ConfLoader {

        /// <summary>
        /// Reads all the line of either the filePath (if the file exists) or from byte array dataResources,
        /// Apply the action toApplyOnEachLine to each line
        /// Uses encoding as the Encoding to read the file or convert the byte array to a string
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataResources"></param>
        /// <param name="toApplyOnEachLine"></param>
        /// <param name="encoding"></param>
        public static void ForEachLine(string filePath, byte[] dataResources, Encoding encoding, Action<string> toApplyOnEachLine) {
            try {
                using (StringReader reader = new StringReader((!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) ? File.ReadAllText(filePath, encoding) : encoding.GetString(dataResources))) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        toApplyOnEachLine(line);
                    }
                }
            } catch (Exception e) {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    ErrorHandler.ShowErrors(e, "Error reading file", filePath);
                else
                    ErrorHandler.ShowErrors(e, "Error data resource for " + filePath);
            }
        }
    }
}
