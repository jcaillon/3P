#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NppLexerVisitor.cs) is part of 3P.
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

using System.Collections.Generic;
using _3PA.MainFeatures.AutoCompletionFeature;

namespace _3PA.MainFeatures.Parser {

    internal class TextLexerVisitor : ILexerVisitor {

        #region Properties

        /// <summary>
        /// False to add the number parsed to the output list
        /// </summary>
        public bool IgnoreNumbers { get; set; }

        /// <summary>
        /// Allows to have a unique list of words and not add twice the same
        /// </summary>
        public HashSet<string> KnownWords { get; set; }

        /// <summary>
        /// Words with a length strictly inferior to this will not appear in the autocompletion
        /// </summary>
        public int MinWordLengthRequired { get; set; }

        public List<CompletionItem> ParsedCompletionItemsList { get; private set; }

        #endregion

        #region Visits

        public void PreVisit(Lexer lexer) {
            if (ParsedCompletionItemsList == null)
                ParsedCompletionItemsList = new List<CompletionItem>();
            if (KnownWords == null)
                KnownWords = new HashSet<string>();
        }

        public void PostVisit() {
        }

        /// <summary>
        /// Numbers
        /// </summary>
        /// <param name="tok"></param>
        public void Visit(TokenNumber tok) {
            if (!IgnoreNumbers) {
                if (!KnownWords.Contains(tok.Value) && tok.EndPosition - tok.StartPosition >= MinWordLengthRequired) {
                    KnownWords.Add(tok.Value);
                    PushToAutoCompletion(new NumberCompletionItem (), tok);
                }
            }
        }

        /// <summary>
        /// Words
        /// </summary>
        /// <param name="tok"></param>
        public void Visit(TokenWord tok) {
            if (!KnownWords.Contains(tok.Value) && tok.EndPosition - tok.StartPosition >= MinWordLengthRequired) {
                KnownWords.Add(tok.Value);
                PushToAutoCompletion(new WordCompletionItem(), tok);
            }
        }

        private void PushToAutoCompletion(TextCompletionItem item, Token origin) {
            item.DisplayText = origin.Value;
            item.OriginToken = origin;
            item.Ranking = AutoCompletion.FindRankingOfParsedItem(item.DisplayText);
            ParsedCompletionItemsList.Add(item);
        }

        #endregion

        #region unused

        public void Visit(TokenComment tok) {
        }

        public void Visit(TokenEol tok) {
        }

        public void Visit(TokenEos tok) {
        }

        public void Visit(TokenInclude tok) {
        }

        public void Visit(TokenPreProcVariable tok) {
        }

        public void Visit(TokenString tok) {
        }

        public void Visit(TokenStringDescriptor tok) {
        }

        public void Visit(TokenSymbol tok) {
        }

        public void Visit(TokenWhiteSpace tok) {
        }
        
        public void Visit(TokenEof tok) {
        }

        public void Visit(TokenUnknown tok) {
        }

        public void Visit(TokenPreProcDirective tok) {
        }

        #endregion

    }
}
