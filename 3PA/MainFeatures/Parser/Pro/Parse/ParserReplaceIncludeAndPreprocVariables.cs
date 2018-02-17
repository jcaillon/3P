using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Lib;
using _3PA.MainFeatures.Parser.Pro.Tokenize;

namespace _3PA.MainFeatures.Parser.Pro.Parse {
    internal partial class Parser {
        /// <summary>
        /// If it is an {&amp;var} or {include.i}, replaces the token at "current position + posAhead" by a list of tokens.
        /// In case of {&amp;var}, the list of tokens has been extracted when we found the GLOBAL/SCOPE-DEFINE.
        /// In case of {include.i}, we will read the file and tokenize it, then we will have the list of token we need to replace.
        /// </summary>
        private bool ReplaceIncludeAndPreprocVariablesAhead(int posAhead) {
            /*
            { include-file
                [ argument ... | {&argument-name = "argument-value" } ... ] }

             This method should handle those cases :
             {   file.i &name=val &2="value"   } -> {&name} and {&2}
             {file.i val "value"} -> {1} {2}

            { &preprocessor-name } 
            { { n | &argument-name } }
            */

            bool weReplacedSomething = false;

            // we check if the token + posAhead will be an include that needs to be replaced
            var toReplaceToken = PeekAt(posAhead);

            HashSet<string> replacedName = null; // keep track of replacement here

            while (toReplaceToken is TokenInclude || toReplaceToken is TokenPreProcVariable) {
                // replace the {include} present within this {include}
                var count = 1;
                while (true) {
                    var curToken = PeekAt(posAhead + count);
                    if (curToken is TokenEof) return false; // we didn't match the end of the include, better not do anything
                    if (curToken is TokenSymbol && curToken.Value == "}") break;
                    if (!ReplaceIncludeAndPreprocVariablesAhead(posAhead + count))
                        count++;
                }

                count++; // number of tokens composing this include

                // get the caracteristics of this include
                string replaceName = null;
                ParsedIncludeFile parsedInclude = null;
                string preprocValue = null;
                if (count >= 3) {
                    if (toReplaceToken is TokenInclude) {
                        parsedInclude = CreateParsedIncludeFile(toReplaceToken, posAhead, posAhead + count - 1);
                        if (parsedInclude != null)
                            replaceName = !string.IsNullOrEmpty(parsedInclude.FullFilePath) ? parsedInclude.FullFilePath : parsedInclude.Name;
                    } else {
                        replaceName = (toReplaceToken.Value == "{" ? "" : "&") + PeekAt(posAhead + 1).Value;
                        preprocValue = CreateUsedPreprocVariable(toReplaceToken, replaceName);
                    }
                }

                // remove the tokens composing this include
                RemoveTokens(posAhead, count);

                weReplacedSomething = true;

                // name not found, leave
                if (string.IsNullOrEmpty(replaceName))
                    break;

                // make sure to not replace the same include in the same replacement loop, if we do that
                // this means we will go into an infinite loop case of a {&{&one}} with one=two and two=one
                if (replacedName == null)
                    replacedName = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) {_parsedIncludes[0].FullFilePath};
                if (replacedName.Contains(replaceName))
                    break;
                replacedName.Add(replaceName);

                var prevToken = PeekAt(posAhead - 1);

                // get the list of tokens that will replace this include
                List<Token> valueTokens;
                if (toReplaceToken is TokenInclude) {
                    valueTokens = GetIncludeFileTokens(prevToken as TokenString, parsedInclude);
                } else {
                    valueTokens = GetPreProcVariableTokens(prevToken as TokenString, toReplaceToken, preprocValue);
                }

                // do we have a definition for the var/include?
                if (valueTokens != null) {
                    // we have to "merge" the TokenWord at the beginning and end of what we are inserting, this allows to take care of
                    // cases like : DEF VAR lc_{&val}_end AS CHAR NO-UNDO.
                    if (MergeTokenAtPosition(posAhead - 1, valueTokens.FirstOrDefault() as TokenWord, true)) {
                        valueTokens.RemoveAt(0);
                    }

                    if (MergeTokenAtPosition(posAhead, valueTokens.LastOrDefault() as TokenWord, false)) {
                        valueTokens.RemoveAt(valueTokens.Count - 1);
                    }

                    // if we are in this case : MESSAGE "begin{include.i}end". we must try to merge this into one single string
                    prevToken = PeekAt(posAhead - 1);
                    var nextToken = PeekAt(posAhead);
                    if (prevToken is TokenString && nextToken is TokenString) {
                        while (MergeTokenAtPosition(posAhead - 1, valueTokens.FirstOrDefault(), true)) {
                            var weMergedAString = valueTokens.FirstOrDefault() is TokenString;
                            valueTokens.RemoveAt(0);
                            if (weMergedAString) 
                                // to handle the particular case where include.i contains something like :
                                // word". MESSAGE "anothre mess
                                break;
                        }

                        while (MergeTokenAtPosition(posAhead, valueTokens.LastOrDefault(), false)) {
                            var weMergedAString = valueTokens.LastOrDefault() is TokenString;
                            valueTokens.RemoveAt(valueTokens.Count - 1);
                            if (weMergedAString)
                                break;
                        }
                    }
                }

                // if we have tokens insert, do it
                if (valueTokens != null && valueTokens.Count > 0) {
                    InsertTokens(posAhead, valueTokens);
                } else {
                    // make sure we don't have two TokenWord following each other, or we must merge them
                    if (MergeTokenAtPosition(posAhead - 1, PeekAt(posAhead) as TokenWord, true) ||
                        MergeTokenAtPosition(posAhead - 1, PeekAt(posAhead) as TokenString, true)) {
                        RemoveTokens(posAhead, 1);
                    }
                }

                toReplaceToken = PeekAt(posAhead);
            }

            return weReplacedSomething;
        }

        /// <summary>
        /// Creates the preproc variable reference to be seen in the explorer
        /// </summary>
        private string CreateUsedPreprocVariable(Token bracketToken, string varName) {

            // do we have a definition for the var?
            string value = GetPreProcVariableValue(bracketToken.OwnerNumber, varName);
            
            AddParsedItem(new ParsedUsedPreProcVariable(varName, bracketToken, value == null), bracketToken.OwnerNumber);

            return value;
        }

        /// <summary>
        /// Create the include reference to be seen in the explorer
        /// </summary>
        private ParsedIncludeFile CreateParsedIncludeFile(Token bracketToken, int startPos, int endPos) {
            // info we will extract from the current statement :
            ParsedIncludeFile newInclude = null;
            string fileName = "";
            bool usesNamedArg = false; // true if the arguments used are with the format : &name=""
            bool expectingFirstArg = true;
            string argName = null;
            int argNumber = 1;
            var parameters = new Dictionary<string, string>(_parsedIncludes[bracketToken.OwnerNumber].ScopedPreProcVariables, StringComparer.CurrentCultureIgnoreCase); // the scoped variable of this procedure will be available in the include file

            var state = 0;
            for (int i = startPos + 1; i <= endPos - 1; i++) {
                var token = PeekAt(i);
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // read the file name
                        if (token is TokenWord || token is TokenString) {
                            fileName += GetTokenStrippedValue(token);
                            state++;
                        }

                        break;

                    case 1:
                        if (token is TokenSymbol && (token.Value.Equals("/") || token.Value.Equals("\\"))) {
                            // it's a path, append it to the name of the run
                            fileName += token.Value;
                            state = 0;
                            break;
                        }

                        // read the arguments
                        if (expectingFirstArg) {
                            // case of a {file.i &x="arg1" &x=arg2}
                            if (token is TokenPreProcDirective) {
                                argName = token.Value;
                                usesNamedArg = true;
                                expectingFirstArg = false;
                                // case of a {file.i "arg1" arg2}
                            } else if (!(token is TokenEol || token is TokenWhiteSpace)) {
                                if (!parameters.ContainsKey(argNumber.ToString()))
                                    parameters.Add(argNumber.ToString(), GetTokenStrippedValue(token));
                                argNumber++;
                                expectingFirstArg = false;
                            }
                        } else {
                            if (usesNamedArg) {
                                // still waiting to read the argument name
                                if (argName == null) {
                                    if (token is TokenPreProcDirective)
                                        argName = token.Value;
                                } else if (!(token is TokenEol || token is TokenWhiteSpace || token.Value == "=")) {
                                    if (!parameters.ContainsKey(argName))
                                        parameters.Add(argName, GetTokenStrippedValue(token));
                                    argName = null;
                                }
                            } else if (!(token is TokenEol || token is TokenWhiteSpace)) {
                                if (!parameters.ContainsKey(argNumber.ToString()))
                                    parameters.Add(argNumber.ToString(), GetTokenStrippedValue(token));
                                argNumber++;
                            }
                        }

                        break;
                }
            }

            // we matched the include file name
            if (!string.IsNullOrEmpty(fileName)) {
                // try to find the file in the propath
                var fullFilePath = FindIncludeFullPath(fileName);

                // always add the parameter "0" which it the filename
                if (parameters.ContainsKey("0"))
                    parameters["0"] = Path.GetFileName(fullFilePath ?? fileName);
                else
                    parameters.Add("0", Path.GetFileName(fullFilePath ?? fileName));

                newInclude = new ParsedIncludeFile(fileName, bracketToken, parameters, fullFilePath, _parsedIncludes[bracketToken.OwnerNumber]);

                AddParsedItem(newInclude, bracketToken.OwnerNumber);
            }

            return newInclude;
        }

        /// <summary>
        /// Returns the list of tokens corresponding to the given include
        /// </summary>
        private List<Token> GetIncludeFileTokens(TokenString previousTokenString, ParsedIncludeFile parsedInclude) {
            // Parse the include file ?
            if (!string.IsNullOrEmpty(parsedInclude.FullFilePath)) {
                ProTokenizer proTokenizer;

                // did we already parsed this file in a previous parse session?
                if (SavedLexerInclude.ContainsKey(parsedInclude.FullFilePath)) {
                    proTokenizer = SavedLexerInclude[parsedInclude.FullFilePath];
                } else {
                    // Parse it
                    if (previousTokenString != null && !string.IsNullOrEmpty(previousTokenString.Value)) {
                        proTokenizer = new ProTokenizer(Utils.ReadAllText(parsedInclude.FullFilePath), previousTokenString.Value[0] == '"', previousTokenString.Value[0] == '\'');
                    } else {
                        proTokenizer = new ProTokenizer(Utils.ReadAllText(parsedInclude.FullFilePath));
                    }
                    if (!SavedLexerInclude.ContainsKey(parsedInclude.FullFilePath))
                        SavedLexerInclude.Add(parsedInclude.FullFilePath, proTokenizer);
                }

                _parsedIncludes.Add(parsedInclude);
                var includeNumber = (ushort) (_parsedIncludes.Count - 1);
                
                // add this include to the references and modify each token
                // Remove EOF
                List<Token> copiedTokens = new List<Token>();
                for (int i = 0; i < proTokenizer.GetTokensList.Count - 1; i++) {
                    var token = proTokenizer.GetTokensList[i];
                    var copiedToken = token.Copy(token.Line, token.Column, token.StartPosition, token.EndPosition);
                    copiedToken.OwnerNumber = includeNumber;
                    copiedTokens.Add(copiedToken);
                }
                return copiedTokens;
            }

            parsedInclude.Flags |= ParseFlag.NotFound;
            return null;
        }

        /// <summary>
        /// Returns the list of tokens corresponding to the {&amp;variable} to replace
        /// </summary>
        private List<Token> GetPreProcVariableTokens(TokenString previousTokenString, Token bracketToken, string value) {
            // do we have a definition for the var?
            if (value == null)
                return null;

            List<Token> valueTokens;
            
            // Parse it
            if (previousTokenString != null && !string.IsNullOrEmpty(previousTokenString.Value)) {
                valueTokens = new ProTokenizer(value, previousTokenString.Value[0] == '"', previousTokenString.Value[0] == '\'').GetTokensList.ToList();
            } else {
                valueTokens = new ProTokenizer(value).GetTokensList.ToList();
            }

            // Remove EOF
            List<Token> copiedTokens = new List<Token>();
            for (int i = 0; i < valueTokens.Count - 1; i++) {
                var copiedToken = valueTokens[i].Copy(bracketToken.Line, bracketToken.Column, bracketToken.StartPosition, bracketToken.EndPosition);
                copiedToken.OwnerNumber = bracketToken.OwnerNumber;
                copiedTokens.Add(copiedToken);
            }
            return copiedTokens;
        }

        /// <summary>
        /// Merge the values of two tokens of the same type (return true if it does)
        /// The merge is done at the givent @position and using the given @token value
        /// </summary>
        private bool MergeTokenAtPosition<T>(int position, T token, bool appendTokenValue) where T : Token {
            var tokenAtPos = PeekAt(position);
            if (token != null && tokenAtPos is T) {
                // append previous word with the first word of the value tokens
                Token newToken;
                if (typeof(T) == typeof(TokenWord))
                    newToken = new TokenWord(appendTokenValue ? tokenAtPos.Value + token.Value : token.Value + tokenAtPos.Value, tokenAtPos.Line, tokenAtPos.Column, tokenAtPos.StartPosition, tokenAtPos.EndPosition);
                else
                    newToken = new TokenString(appendTokenValue ? tokenAtPos.Value + token.Value : token.Value + tokenAtPos.Value, tokenAtPos.Line, tokenAtPos.Column, tokenAtPos.StartPosition, tokenAtPos.EndPosition);
                ReplaceToken(position, newToken);
                return true;
            }
            return false;
        }

        /// <summary>
        /// set a preprocessed variable to its owner. Input ownerNumber to 0 for a global variable
        /// </summary>
        private void SetPreProcVariableValue(int ownerNumber, string variableName, string variableValue) {
            if (_parsedIncludes[ownerNumber].ScopedPreProcVariables.ContainsKey("&" + variableName))
                _parsedIncludes[ownerNumber].ScopedPreProcVariables["&" + variableName] = variableValue;
            else
                _parsedIncludes[ownerNumber].ScopedPreProcVariables.Add("&" + variableName, variableValue);
        }

        /// <summary>
        /// Get a preprocessed variable value
        /// </summary>
        private string GetPreProcVariableValue(int ownerNumber, string variableName) {
            if (_parsedIncludes[ownerNumber].ScopedPreProcVariables.ContainsKey(variableName))
                return _parsedIncludes[ownerNumber].ScopedPreProcVariables[variableName];
            if (_parsedIncludes[0].ScopedPreProcVariables.ContainsKey(variableName))
                return _parsedIncludes[0].ScopedPreProcVariables[variableName];
            return null;
        }
    }
}