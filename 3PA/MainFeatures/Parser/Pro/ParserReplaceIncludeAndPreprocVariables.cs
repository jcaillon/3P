using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {

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
                if (count >= 3) {
                    if (toReplaceToken is TokenInclude) {
                        parsedInclude = CreateParsedIncludeFile(toReplaceToken, posAhead, posAhead + count - 1);
                        if (parsedInclude != null)
                            replaceName = parsedInclude.FullFilePath;
                    } else {
                        replaceName = (toReplaceToken.Value == "{" ? "" : "&") + PeekAt(posAhead + 1).Value;
                    }
                }
                
                // remove the tokens composing this include
                RemoveTokens(posAhead, count);

                weReplacedSomething = true;

                // name not found, leave
                if (string.IsNullOrEmpty(replaceName))
                    break;

                // make sure to not replace the same include in the same replacement loop, if we do that
                // this means we will go into an infinite loop
                if (replacedName == null)
                    replacedName = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) { _parsedIncludes[0].FullFilePath };
                if (replacedName.Contains(replaceName))
                    break;
                replacedName.Add(replaceName);
                
                // get the list of tokens that will replace this include
                List<Token> valueTokens;
                if (toReplaceToken is TokenInclude) {
                    valueTokens = GetIncludeFileTokens(toReplaceToken, parsedInclude);
                } else {
                    valueTokens = GetPreProcVariableTokens(toReplaceToken, replaceName);
                }

                // do we have a definition for the var/include?
                if (valueTokens == null) {
                    // otherwise, make sure we don't have two TokenWord following each other
                    var prevToken = PeekAt(posAhead - 1);
                    var nextToken = PeekAt(posAhead);
                    if (prevToken is TokenWord && PeekAt(posAhead) is TokenWord) {
                        ReplaceToken(posAhead - 1, new TokenWord(prevToken.Value + nextToken.Value, prevToken.Line, prevToken.Column, prevToken.StartPosition, nextToken.EndPosition));
                        RemoveTokens(posAhead, 1);
                    } else {
                        // if we don't have the definition for the variable, it must be replaced by an empty whitespace
                        valueTokens = new List<Token> {
                            new TokenWhiteSpace("", toReplaceToken.Line, toReplaceToken.Column, toReplaceToken.StartPosition, toReplaceToken.EndPosition)
                        };
                    }

                } else {
                    // we have to "merge" the TokenWord at the beginning and end of what we are inserting, this allows to take care of
                    // cases like : DEF VAR lc_truc{&extension} AS CHAR NO-UNDO.
                    var prevToken = PeekAt(posAhead - 1);
                    if (valueTokens.FirstOrDefault() is TokenWord && prevToken is TokenWord) {
                        // append previous word with the first word of the value tokens
                        ReplaceToken(posAhead - 1, new TokenWord(prevToken.Value + valueTokens.First().Value, prevToken.Line, prevToken.Column, prevToken.StartPosition, prevToken.EndPosition));
                        valueTokens.RemoveAt(0);
                    }
                    var nextToken = PeekAt(posAhead);
                    if (valueTokens.LastOrDefault() is TokenWord && nextToken is TokenWord) {
                        ReplaceToken(posAhead, new TokenWord(valueTokens.Last().Value + nextToken.Value, nextToken.Line, nextToken.Column, nextToken.StartPosition, nextToken.EndPosition));
                        valueTokens.RemoveAt(valueTokens.Count - 1);
                    }
                }

                // if we have tokens insert, do it
                if (valueTokens != null && valueTokens.Count > 0) {
                    InsertTokens(posAhead, valueTokens);
                }

                toReplaceToken = PeekAt(posAhead);
            }

            return weReplacedSomething;
        }

        private ParsedIncludeFile CreateParsedIncludeFile(Token bracketToken, int startPos, int endPos) {

            // info we will extract from the current statement :
            ParsedIncludeFile newInclude = null;
            string fileName = "";
            bool usesNamedArg = false; // true if the arguments used are with the format : &name=""
            bool expectingFirstArg = true;
            string argName = null;
            int argNumber = 1;
            var parameters = new Dictionary<string, List<Token>>(_parsedIncludes[bracketToken.OwnerNumber].ScopedPreProcVariables, StringComparer.CurrentCultureIgnoreCase); // the scoped variable of this procedure will be available in the include file

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
                                    parameters.Add(argNumber.ToString(), TokenizeString(GetTokenStrippedValue(token)));
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
                                        parameters.Add(argName, TokenizeString(GetTokenStrippedValue(token)));
                                    argName = null;
                                }
                            } else if (!(token is TokenEol || token is TokenWhiteSpace)) {
                                if (!parameters.ContainsKey(argNumber.ToString()))
                                    parameters.Add(argNumber.ToString(), TokenizeString(GetTokenStrippedValue(token)));
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
                    parameters["0"] = new List<Token> {new TokenWord(Path.GetFileName(fullFilePath ?? fileName), 0, 0, 0, 0)};
                else
                    parameters.Add("0", new List<Token> {new TokenWord(Path.GetFileName(fullFilePath ?? fileName), 0, 0, 0, 0)});

                newInclude = new ParsedIncludeFile(fileName, bracketToken, parameters, fullFilePath, _parsedIncludes[bracketToken.OwnerNumber]);

                AddParsedItem(newInclude, bracketToken.OwnerNumber);
            }

            return newInclude;
        }

        /// <summary>
        /// Returns the list of tokens corresponding to the given include
        /// </summary>
        /// <param name="bracketToken"></param>
        /// <param name="parsedInclude"></param>
        /// <returns></returns>
        private List<Token> GetIncludeFileTokens(Token bracketToken, ParsedIncludeFile parsedInclude) {

            // Parse the include file ?
            if (!string.IsNullOrEmpty(parsedInclude.FullFilePath)) {

                ProLexer proLexer;

                // did we already parsed this file in a previous parse session?
                if (SavedLexerInclude.ContainsKey(parsedInclude.FullFilePath)) {
                    proLexer = SavedLexerInclude[parsedInclude.FullFilePath];
                } else {
                    // Parse it
                    proLexer = new ProLexer(Utils.ReadAllText(parsedInclude.FullFilePath));
                    if (!SavedLexerInclude.ContainsKey(parsedInclude.FullFilePath))
                        SavedLexerInclude.Add(parsedInclude.FullFilePath, proLexer);
                }

                // add this include to the references and modify each token
                _parsedIncludes.Add(parsedInclude);
                var includeNumber = (ushort) (_parsedIncludes.Count - 1);
                var tokens = proLexer.GetTokensList.ToList().GetRange(0, proLexer.GetTokensList.Count - 1).Select(token => token.Copy(token.Line, token.Column, token.StartPosition, token.EndPosition)).ToList();
                tokens.ForEach(token => token.OwnerNumber = includeNumber);

                return tokens;

            }

            parsedInclude.Flags |= ParseFlag.NotFound;
            return null;
        }

        /// <summary>
        /// Returns the list of tokens corresponding to the {&amp;variable} to replace
        /// </summary>
        /// <param name="bracketToken"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        private List<Token> GetPreProcVariableTokens(Token bracketToken, string varName) {
            List<Token> valueTokens;

            // do we have a definition for the var?
            if (_parsedIncludes[bracketToken.OwnerNumber].ScopedPreProcVariables.ContainsKey(varName))
                valueTokens = _parsedIncludes[bracketToken.OwnerNumber].ScopedPreProcVariables[varName].ToList();
            else if (_globalPreProcVariables.ContainsKey(varName))
                valueTokens = _globalPreProcVariables[varName].ToList();
            else
                return null;

            List<Token> copiedTokens = new List<Token>();
            foreach (var token in valueTokens) {
                var copiedToken = token.Copy(bracketToken.Line, bracketToken.Column, bracketToken.StartPosition, bracketToken.EndPosition);
                copiedToken.OwnerNumber = bracketToken.OwnerNumber;
                copiedTokens.Add(copiedToken);
            }

            return copiedTokens;
        }

    }
}