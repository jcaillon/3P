using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Html;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures {
    class CodeBeautifier {

        /// <summary>
        /// Tries to re-indent the code of the whole document
        /// </summary>
        public static void CorrectCodeIndentation() {
            // Can we indent? We can't if we didn't parse the code correctly or if there are grammar errors
            if (ParserHandler.CanIndent()) {
                
            } else {
                UserCommunication.Notify("This action can't be executed right now because it seems that your document contains grammatical errors.<br><br><i>If the code compiles sucessfully then i failed to parse your document correctly, please make sure to create an issue on the project's github and (if possible) include the incriminating code so i can fix this problem : <br><a href='#about'>Open the about window to get the github url</a>", MessageImg.MsgRip, "Format document", "Incorrect grammar", args => { Appli.Appli.GoToAboutPage(); }, 20);
            }
        }
    }
}
