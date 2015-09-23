using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA {
    class Abl {

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
    }
}
