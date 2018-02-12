using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {

    internal partial class Parser {

        /// <summary>
        /// Analyse a preprocessed directive (analyses the whole statement)
        /// This method does not use the MoveNext because we don't want to analyse the words in a preprocess directive
        /// (in case of a scope-define we will analyse the words in the variable value when we actually use the variable,
        /// and in the other directive the words are garbage)
        /// </summary>
        private ParsedScopePreProcBlock CreateParsedPreProcDirective(Token directiveToken) {

            ParsedScopePreProcBlock newBlock = null;

            // info we will extract from the current statement :
            string variableName = null;
            StringBuilder definition = new StringBuilder();

            var count = 0;
            while (true) {
                count++;
                // need to replace in case we use for instance a {&var} in a scope-define value
                ReplaceIncludeAndPreprocVariablesAhead(count);
                var token = PeekAt(count);
                if (token is TokenEof) break;
                if (token is TokenComment) continue;
                // a ~ allows for a eol but we don't control if it's an eol because if it's something else we probably parsed it wrong anyway (in the lexer)
                if (token is TokenSymbol && token.Value == "~") {
                    if (PeekAt(count + 1) is TokenEol)
                        count++;
                    continue;
                }
                if (token is TokenEol) break;

                // read the first word after the directive
                if (string.IsNullOrEmpty(variableName) && token is TokenWord) {
                    variableName = token.Value;
                    continue;
                }

                definition.Append(token.Value);
            }
            
            ParseFlag flags = 0;

            // match first word of the statement
            switch (directiveToken.Value.ToUpper()) {
                case "&GLOBAL-DEFINE":
                case "&GLOBAL":
                case "&GLOB":
                    flags |= ParseFlag.Global;
                    break;

                case "&SCOPED-DEFINE":
                case "&SCOPED":
                    flags |= ParseFlag.FileScope;
                    break;

                case "&ANALYZE-SUSPEND":
                    // we don't care about the blocks of include files
                    if (directiveToken.OwnerNumber > 0)
                        break;

                    // it marks the beginning of an appbuilder block, it can only be at a root/File level, otherwise flag error
                    if (!( GetCurrentBlock<ParsedScopeBlock>() is ParsedFile)) {
                        _parserErrors.Add(new ParserError(ParserErrorType.NotAllowedUibBlockStart, directiveToken, _context.BlockStack.Count, _parsedIncludes));
                    }

                    // we match a new block start but we didn't match the previous block end, flag error
                    if (GetCurrentBlock<ParsedScopePreProcBlock>() != null) {
                        _parserErrors.Add(new ParserError(ParserErrorType.UnexpectedUibBlockStart, directiveToken, _context.BlockStack.Count, _parsedIncludes));
                    }

                    // matching different intersting blocks
                    var textAfterDirective = variableName + " " + definition.ToString().Trim();
                    ParsedPreProcBlockType type = ParsedPreProcBlockType.Block;
                    string blockName = "Appbuilder block";
                    if (textAfterDirective.ContainsFast("_FUNCTION-FORWARD")) {
                        type = ParsedPreProcBlockType.Prototype;
                        blockName = "Function prototype";
                    } else if (textAfterDirective.ContainsFast("_MAIN-BLOCK")) {
                        type = ParsedPreProcBlockType.MainBlock;
                        blockName = "Main block";
                    } else if (textAfterDirective.ContainsFast("_DEFINITIONS")) {
                        type = ParsedPreProcBlockType.DefinitionBlock;
                        blockName = "Definitions";
                    } else if (textAfterDirective.ContainsFast("_UIB-PREPROCESSOR-BLOCK")) {
                        type = ParsedPreProcBlockType.UibPreprocessorBlock;
                        blockName = "Pre-processor definitions";
                    } else if (textAfterDirective.ContainsFast("_XFTR")) {
                        type = ParsedPreProcBlockType.XtfrBlock;
                        blockName = "Xtfr";
                    } else if (textAfterDirective.ContainsFast("_PROCEDURE-SETTINGS")) {
                        type = ParsedPreProcBlockType.SettingsBlock;
                        blockName = "Procedure settings";
                    } else if (textAfterDirective.ContainsFast("_CREATE-WINDOW")) {
                        type = ParsedPreProcBlockType.CreateWindowBlock;
                        blockName = "Window settings";
                    } else if (textAfterDirective.ContainsFast("_RUN-TIME-ATTRIBUTES")) {
                        type = ParsedPreProcBlockType.RuntimeBlock;
                        blockName = "Runtime attributes";
                    }

                    newBlock = new ParsedScopePreProcBlock(blockName, directiveToken) {
                        Type = type,
                        BlockDescription = textAfterDirective
                    };

                    // save the block description
                    AddParsedItem(newBlock, directiveToken.OwnerNumber);
                    break;
                    
                case "&UNDEFINE":
                    if (variableName != null) {
                        var found = (ParsedPreProcVariable) _parsedItemList.FindLast(item => (item is ParsedPreProcVariable && item.Name.Equals(variableName)));
                        if (found != null)
                            found.UndefinedLine = _context.StatementFirstToken.Line;
                    }
                    break;
            }

            // We matched a new preprocessed variable?
            if (flags > 0 && !string.IsNullOrEmpty(variableName)) {
                var newVar = new ParsedPreProcVariable(variableName, directiveToken, 0, definition.ToString().Trim()) {
                    Flags = flags
                };
                AddParsedItem(newVar, directiveToken.OwnerNumber);

                // add it to the know variables (either to the global scope or to the local scope)
                SetPreProcVariableValue(flags.HasFlag(ParseFlag.Global) ? 0 : directiveToken.OwnerNumber, variableName, newVar.Value);
            }

            // we directly set the new token position there (it will be the EOL after this directive)
            _tokenPos += count;

            // since we didn't use MoveNext we also manually replace the includes ahead
            ReplaceIncludeAndPreprocVariablesAhead(1);
            ReplaceIncludeAndPreprocVariablesAhead(2);

            return newBlock;
        }
        
        
    }
}