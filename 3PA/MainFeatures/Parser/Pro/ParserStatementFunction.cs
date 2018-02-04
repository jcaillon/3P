using System;
using System.Collections.Generic;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

        /// <summary>
        /// Matches a function definition (not the FORWARD prototype)
        /// </summary>
        private bool CreateParsedFunction(Token functionToken) {
            // info we will extract from the current statement :
            string name = null;
            string parsedReturnType = null;
            string extend = null;
            ParseFlag flags = 0;
            StringBuilder parameters = new StringBuilder();
            List<ParsedDefine> parametersList = null;

            _lastTokenWasSpace = true;

            Token token;
            int state = 0;
            do {
                token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                switch (state) {
                    case 0:
                        // matching name
                        if (!(token is TokenWord)) break;
                        name = token.Value;
                        state++;
                        break;
                    case 1:
                        // matching return type
                        if (!(token is TokenWord)) break;
                        if (token.Value.EqualsCi("returns") || token.Value.EqualsCi("class"))
                            continue;

                        parsedReturnType = token.Value;

                        state++;
                        break;
                    case 2:
                        // matching parameters (start)
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("private"))
                                flags |= ParseFlag.Private;
                            if (token.Value.EqualsCi("extent"))
                                flags |= ParseFlag.Extent;

                            // we didn't match any opening (, but we found a forward
                            if (token.Value.EqualsCi("forward"))
                                state = 99;
                            else if (token.Value.EqualsCi("in"))
                                state = 100;
                        } else if (token is TokenSymbol && token.Value.Equals("("))
                            state = 3;
                        else if (flags.HasFlag(ParseFlag.Extent) && token is TokenNumber)
                            extend = token.Value;
                        break;
                    case 3:
                        // read parameters, define a ParsedDefineItem for each
                        parametersList = GetParsedParameters(functionToken, parameters);
                        state = 10;
                        break;
                    case 10:
                        // matching prototype, we dont want to create a ParsedItem for prototype
                        if (token is TokenWord) {
                            if (token.Value.EqualsCi("forward"))
                                state = 99;
                            else if (token.Value.EqualsCi("in"))
                                state = 100;
                        }
                        break;
                }
            } while (MoveNext());
            if (name == null || parsedReturnType == null)
                return false;

            // otherwise it needs to ends with : or .
            if (!(token is TokenEos))
                return false;

            var returnType = ConvertStringToParsedPrimitiveType(parsedReturnType, false);

            // New prototype, we matched a forward or a IN
            if (state >= 99) {
                ParsedPrototype createdProto = new ParsedPrototype(name, functionToken, returnType) {
                    Scope = _context.Scope,
                    FilePath = FilePathBeingParsed,
                    SimpleForward = state == 99, // allows us to know if we expect an implementation in this .p or not
                    EndPosition = token.EndPosition,
                    EndBlockLine = token.Line,
                    EndBlockPosition = token.EndPosition,
                    Flags = flags,
                    Extend = extend ?? String.Empty,
                    ParametersString = parameters.ToString()
                };
                if (!_functionPrototype.ContainsKey(name))
                    _functionPrototype.Add(name, createdProto);

                AddParsedItem(createdProto, functionToken.OwnerNumber);

                // case of a IN
                if (!createdProto.SimpleForward) {
                    // modify context
                    _context.Scope = createdProto;

                    // add the parameters to the list
                    if (parametersList != null) {
                        createdProto.Parameters = new List<ParsedDefine>();
                        foreach (var parsedItem in parametersList) {
                            AddParsedItem(parsedItem, functionToken.OwnerNumber);
                            createdProto.Parameters.Add(parsedItem);
                        }
                    }

                    // reset context
                    _context.Scope = _rootScope;
                }

                return false;
            }

            // New function
            ParsedImplementation createdImp = new ParsedImplementation(name, functionToken, returnType) {
                EndPosition = token.EndPosition,
                Flags = flags,
                Extend = extend ?? String.Empty,
                ParametersString = parameters.ToString()
            };

            // it has a prototype?
            if (_functionPrototype.ContainsKey(name)) {
                // make sure it was a prototype!
                var proto = _functionPrototype[name] as ParsedPrototype;
                if (proto != null && proto.SimpleForward) {
                    createdImp.HasPrototype = true;
                    createdImp.PrototypeLine = proto.Line;
                    createdImp.PrototypeColumn = proto.Column;
                    createdImp.PrototypePosition = proto.Position;
                    createdImp.PrototypeEndPosition = proto.EndPosition;

                    // boolean to know if the implementation matches the prototype
                    createdImp.PrototypeUpdated = (
                        createdImp.Flags == proto.Flags &&
                        createdImp.Extend.Equals(proto.Extend) &&
                        createdImp.ReturnType.Equals(proto.ReturnType) &&
                        createdImp.ParametersString.Equals(proto.ParametersString));
                }
            } else {
                _functionPrototype.Add(name, createdImp);
            }

            AddParsedItem(createdImp, functionToken.OwnerNumber);

            // modify context
            _context.Scope = createdImp;

            // add the parameters to the list
            if (parametersList != null) {
                createdImp.Parameters = new List<ParsedDefine>();
                foreach (var parsedItem in parametersList) {
                    AddParsedItem(parsedItem, functionToken.OwnerNumber);
                    createdImp.Parameters.Add(parsedItem);
                }
            }

            return true;
        }

    }
}