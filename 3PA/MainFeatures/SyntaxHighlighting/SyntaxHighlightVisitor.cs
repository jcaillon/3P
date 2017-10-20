#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (SyntaxHighlightVisitor.cs) is part of 3P.
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
using System.Linq;
using _3PA.MainFeatures.AutoCompletionFeature;
using _3PA.MainFeatures.Parser;
using _3PA.NppCore;
using Lexer = _3PA.MainFeatures.Parser.Lexer;

namespace _3PA.MainFeatures.SyntaxHighlighting {

    internal class SyntaxHighlightVisitor : ILexerVisitor {

        private int _includeDepth;

        private char[] _operatorChars = { '=', '+', '-', '/', '*', '^', '<', '>' };

        public void PreVisit(Lexer lexer) {
            var proLexer = lexer as ProLexer;
            if (proLexer != null) {
                _includeDepth = proLexer.IncludeDepth;
                Sci.StartStyling(proLexer.Offset);
            }
        }

        public void Visit(TokenComment tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Comment);
        }

        public void Visit(TokenEol tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.WhiteSpace);
        }

        public void Visit(TokenEos tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Default);
        }

        public void Visit(TokenInclude tok) {
            _includeDepth++;
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Include);
        }

        public void Visit(TokenPreProcVariable tok) {
            _includeDepth++;
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Include);
        }

        public void Visit(TokenNumber tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Number);
        }

        public void Visit(TokenString tok) {
            if (tok.Value != null && tok.Value[0] == '\'') {
                SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.SimpleQuote);
            } else {
                SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.DoubleQuote);
            }
        }

        public void Visit(TokenStringDescriptor tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Default);
        }

        public void Visit(TokenSymbol tok) {
            SciStyleId style = SciStyleId.Default;
            if (_includeDepth > 0 && tok.Value == "}") {
                _includeDepth--;
                style = SciStyleId.Include;
            } else if (tok.EndPosition - tok.StartPosition == 1 && _operatorChars.Contains(tok.Value[0])) {
                style = SciStyleId.Operator;
            }
            SetStyling(tok.EndPosition - tok.StartPosition, style);
        }

        public void Visit(TokenEof tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.WhiteSpace);
        }

        public void Visit(TokenWord tok) {
            SciStyleId style = SciStyleId.Default;
            if (_includeDepth > 0)
                style = SciStyleId.Include;
            else {
                var existingKeywords = Keywords.Instance.GetKeywordsByName(tok.Value);
                if (existingKeywords != null && existingKeywords.Count > 0) {
                    style = existingKeywords.First().KeywordSyntaxStyle;
                }
            }

            //NormedVariables, // variables prefix (gc_, li_...)
            /*"gc_", "gch_", "gda_", "gdt_", "gdz_", "gd_", "gh_", "gi_", "gl_", "glg_", "gm_", "grw_", "gr_", "gr_", "gwh_", "sc_", "gch_", "gda_", "gdt_", "gdz_", "gd_", "gh_", "gi_", "gl_", "glg_", "gm_", "grw_", "gr_", "gr_", "gwh_", "lc_", "lch_", "lda_", "ldt_", "ldz_", "ld_", "lh_", "li_", "ll_", "llg_", "lm_", "lrw_", "lr_", "lr_", "lwh_", "ipc_", "ipch_", "ipda_", "ipdt_", "ipdz_", "ipd_", "iph_", "ipi_", "ipl_", "iplg_", "ipm_", "iprw_", "ipr_", "ipr_", "ipwh_", "opc_", "opch_", "opda_", "opdt_", "opdz_", "opd_", "oph_", "opi_", "opl_", "oplg_", "opm_", "oprw_", "opr_", "opr_", "opwh_", "iop_", "iopc_", "iopch_", "iopda_", "iopdt_", "iopdz_", "iopd_", "ioph_", "iopi_", "iopl_", "ioplg_", "iopm_", "ioprw_", "iopr_", "iopr_", "iopwh_",  
             */

            SetStyling(tok.EndPosition - tok.StartPosition, style);
        }

        public void Visit(TokenWhiteSpace tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.WhiteSpace);
        }

        public void Visit(TokenUnknown tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Default);
        }

        public void Visit(TokenPreProcDirective tok) {
            SetStyling(tok.EndPosition - tok.StartPosition, SciStyleId.Preprocessor);
        }

        public void PostVisit() {
        }

        private void SetStyling(int length, SciStyleId style) {
            Sci.SetStyling(length, (int)style);
        }
    }
}