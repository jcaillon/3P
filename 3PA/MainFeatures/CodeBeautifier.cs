#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (CodeBeautifier.cs) is part of 3P.
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
