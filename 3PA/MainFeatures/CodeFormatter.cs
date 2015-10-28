#region Header
// // ========================================================================
// // Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// // This file (CodeFormatter.cs) is part of 3P.
// 
// // 3P is a free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// 
// // 3P is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// // GNU General Public License for more details.
// 
// // You should have received a copy of the GNU General Public License
// // along with 3P. If not, see <http://www.gnu.org/licenses/>.
// // ========================================================================
#endregion
using System;
using System.Linq;
using System.Threading.Tasks;
using _3PA.Interop;
using _3PA.Lib;

namespace _3PA.MainFeatures
{
    class CodeFormatter
    {
        //public static void OnCharTyped(char c)
        //{
        //    if (Config.Instance.SmartIndenting)
        //    {
        //        switch (c)
        //        {
        //            //case '\n': OnNewLine(); break; //it conflicts with N++ auto-indent
        //            case '{': OnOpenBracket(); break;
        //            case '}': OnCloseBracket(); break;
        //        }
        //    }
        //    //it conflicts with N++ auto-indent
        //    //else if (Config.Instance.FormatAsYouType)
        //    //{
        //    //    switch (c)
        //    //    {
        //    //        case '\n': OnNewLine(); break; 
        //    //    }
        //    //}
        //}

        //static void OnNewLine()
        //{
        //    if (Config.Instance.FormatAsYouType)
        //    {
        //        int currentLineNum = Npp.GetCaretLineNumber();
        //        string prevLineText = Npp.GetLine(currentLineNum - 1).TrimEnd();

        //        if (prevLineText != "")
        //            SourceCodeFormatter.FormatDocumentPrevLines();
        //    }
        //    if (Config.Instance.SmartIndenting)
        //        FormatCurrentLine();
        //}

        static void FormatCurrentLine()
        {
            int currentLineNum = Npp.GetCaretLineNumber();
            string prevLineText = Npp.GetLineText(currentLineNum - 1).TrimEnd();

            if (prevLineText.EndsWith("{") || prevLineText.IsControlStatement())
                Perform(InsertIndent);
        }

        static void OnOpenBracket()
        {
            int currentPos = Npp.GetCaretPosition();
            int currentLineNum = Npp.GetLineFromPosition(currentPos);
            string currLineText = Npp.GetLineText(currentLineNum);
            string prevLineText = Npp.GetLineText(currentLineNum - 1);

            if (currLineText.Trim() == "{" && prevLineText.IsControlStatement())
                Perform(RemoveIndent);
        }

        static void OnCloseBracket()
        {
            int currentPos = Npp.GetCaretPosition();
            int currentLineNum = Npp.GetLineFromPosition(currentPos);
            string currLineText = Npp.GetLineText(currentLineNum);
            //string prevText = Npp.TextBeforeCursor(500); //do not load all all "top" document but its substantial part

            //if (currLineText.Trim() == "}" && IsBracketOpened(prevText))
                Perform(RemoveIndent);
        }

        static bool IsBracketOpened(string text)
        {
            //TODO: it is better to use unsafe here.
            int openedBracketsCount = 0;

            if (!string.IsNullOrWhiteSpace(text))
                for (int i = text.Length - 1; i >= 0; i--)
                {
                    if (text[i] == '{')
                        openedBracketsCount++;
                    else if (text[i] == '}')
                        openedBracketsCount--;

                    if (openedBracketsCount > 0)
                        return true;
                }
            return false;
        }

        static void Perform(Action action)
        {
            Task.Factory.StartNew(action); //needs to be asynchronous to not to interfere with the SCI processing the typed chars
        }

        static void InsertIndent()
        {
            int currentPos = Npp.GetCaretPosition();
            IntPtr sci = Npp.HandleScintilla;
            Win32.SendMessage(sci, SciMsg.SCI_ADDTEXT, IndentText);
        }

        static string indentText = null;

        static public string IndentText
        {
            get
            {
                if (indentText == null)
                {
                    if (UseTabs)
                    {
                        indentText = "\t";
                    }
                    else
                    {
                        int widthInChars = (int)Win32.SendMessage(Npp.HandleScintilla, SciMsg.SCI_GETTABWIDTH, 0, 0);
                        indentText = new string(' ', widthInChars);
                    }
                }
                return indentText;
            }

            set
            {
                indentText = value;
            }
        }

        static bool? useTabs;

        static public bool UseTabs
        {
            get
            {
                if (!useTabs.HasValue)
                {
                    int retval = (int)Win32.SendMessage(Npp.HandleScintilla, SciMsg.SCI_GETUSETABS, 0, 0);
                    useTabs = (retval == 1);
                }
                return useTabs.Value;
            }
            set { useTabs = value; }
        }

        static void RemoveIndent()
        {
            int currentPos = Npp.GetCaretPosition();
            IntPtr sci = Npp.HandleScintilla;
            int startPos = currentPos - 1 - IndentText.GetByteCount();
            int endPos = currentPos - 1;
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, startPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, endPos, 0);
            //Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, 0, "");
            Win32.SendMessage(sci, SciMsg.SCI_REPLACESEL, "");
            currentPos = startPos + 1;
            Win32.SendMessage(sci, SciMsg.SCI_SETCURRENTPOS, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONSTART, currentPos, 0);
            Win32.SendMessage(sci, SciMsg.SCI_SETSELECTIONEND, currentPos, 0);
        }

        static public char[] operatorChars = new[] { '+', '=', '-', '*', '/', '%', '&', '|', '^', '<', '>', '!' };
        static public char[] wordDelimiters = new[] { '\t', '\n', '\r', '\'', ' ', '.', ';', ',', '[', '{', '(', ')', '}', ']' };
        static public char[] AllWordDelimiters
        {
            get
            {
                return wordDelimiters.Concat(operatorChars).ToArray();
            }
        }
    }
}
