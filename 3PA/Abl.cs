using _3PA.Lib;

namespace _3PA {
    public class Abl {

        /// <summary>
        /// autocase the keyword in input according to the user config
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static string AutoCaseToUserLiking(string keyword) {
            string output;
            switch (Config.Instance.AutoCompleteChangeCaseMode) {
                case 1:
                    output = keyword.ToUpper();
                    break;
                case 2:
                    output = keyword.ToLower();
                    break;
                default:
                    output = keyword.ToTitleCase();
                    break;
            }
            return output;
        }

        /// <summary>
        /// is the char allowed in a variable's name?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCharAllowedInVariables(char c) {
            return char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '&';
        }

        /// <summary>
        /// Is the current file (in npp) a progress file? (allowed extensions defined in Config)
        /// </summary>
        /// <returns></returns>
        public static bool IsCurrentProgressFile() {
            var ext = Npp.GetCurrentFileExtension();
            return !string.IsNullOrEmpty(ext) && Config.Instance.GlobalProgressExtension.Contains(ext);
        }

        /// <summary>
        /// reads a word with this format : [a-Z_&]+[\w_-]*((\.[\w_-]*)?){1,}
        /// </summary>
        public static string ReadAblWord(string input, bool stopAtPoint, out int nbPoints, bool readRightToLeft = true) {
            nbPoints = 0;
            var max = input.Length - 1;
            int count = 0;
            while (count <= max) {
                var pos = readRightToLeft ? max - count : count;
                var ch = input[pos];
                // normal word
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch =='&')
                    count++;
                else if (ch == '.' && !stopAtPoint) {
                    count++;
                    nbPoints++;
                } else break;
            }
            return count == 0 ? string.Empty : input.Substring(readRightToLeft ? input.Length - count : 0, count);
        }
    }
}
