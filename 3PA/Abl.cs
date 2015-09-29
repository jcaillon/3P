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
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string 
        /// stopAtPoint or not, if not, output the nbPoints found
        /// </summary>
        /// <param name="input"></param>
        /// <param name="stopAtPoint"></param>
        /// <param name="nbPoints"></param>
        /// <param name="readRightToLeft"></param>
        /// <returns></returns>
        public static string ReadAblWord(string input, bool stopAtPoint, out int nbPoints, bool readRightToLeft = true) {
            nbPoints = 0;
            var max = input.Length - 1;
            int count = 0;
            while (count <= max) {
                var pos = readRightToLeft ? max - count : count;
                var ch = input[pos];
                // normal word
                if (IsCharAllowedInVariables(ch))
                    count++;
                else if (ch == '.' && !stopAtPoint) {
                    count++;
                    nbPoints++;
                } else break;
            }
            return count == 0 ? string.Empty : input.Substring(readRightToLeft ? input.Length - count : 0, count);
        }

        /// <summary>
        /// Overload,
        /// Reads a a word, either starting from the end (readRightToLeft = true) of the start of the input string 
        /// stopAtPoint or not
        /// </summary>
        /// <param name="input"></param>
        /// <param name="stopAtPoint"></param>
        /// <param name="readRightToLeft"></param>
        /// <returns></returns>
        public static string ReadAblWord(string input, bool stopAtPoint, bool readRightToLeft = true) {
            int nb;
            return ReadAblWord(input, stopAtPoint, out nb, readRightToLeft);
        }
    }
}
