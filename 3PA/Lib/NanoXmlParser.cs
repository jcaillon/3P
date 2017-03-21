#region header

// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (NanoXmlParser.cs) is part of 3P.
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

using System;
using System.Collections.Generic;

namespace _3PA.Lib {
    /// <summary>
    /// Credits go to https://www.codeproject.com/Tips/682245/NanoXML-Simple-and-fast-XML-parser
    /// Base class containing useful features for all XML classes
    /// </summary>
    internal class NanoXmlBase {
        protected static bool IsSpace(char c) {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        protected static void SkipSpaces(string str, ref int i) {
            while (i < str.Length) {
                if (!IsSpace(str[i])) {
                    if (str[i] == '<' && i + 4 < str.Length && str[i + 1] == '!' && str[i + 2] == '-' && str[i + 3] == '-') {
                        i += 4; // skip <!--

                        while (i + 2 < str.Length && !(str[i] == '-' && str[i + 1] == '-'))
                            i++;

                        i += 2; // skip --
                    } else
                        break;
                }

                i++;
            }
        }

        protected static string GetValue(string str, ref int i, char endChar, char endChar2, bool stopOnSpace) {
            int start = i;
            while ((!stopOnSpace || !IsSpace(str[i])) && str[i] != endChar && str[i] != endChar2) i++;

            return str.Substring(start, i - start);
        }

        protected static bool IsQuote(char c) {
            return c == '"' || c == '\'';
        }

        // returns name
        protected static string ParseAttributes(string str, ref int i, List<NanoXmlAttribute> attributes, char endChar, char endChar2) {
            SkipSpaces(str, ref i);
            string name = GetValue(str, ref i, endChar, endChar2, true);

            SkipSpaces(str, ref i);

            while (str[i] != endChar && str[i] != endChar2) {
                string attrName = GetValue(str, ref i, '=', '\0', true);

                SkipSpaces(str, ref i);
                i++; // skip '='
                SkipSpaces(str, ref i);

                char quote = str[i];
                if (!IsQuote(quote))
                    throw new XmlParsingException("Unexpected token after " + attrName);

                i++; // skip quote
                string attrValue = GetValue(str, ref i, quote, '\0', false);
                i++; // skip quote

                attributes.Add(new NanoXmlAttribute(attrName, attrValue));

                SkipSpaces(str, ref i);
            }

            return name;
        }
    }

    /// <summary>
    /// Class representing whole DOM XML document
    /// </summary>
    internal class NanoXmlDocument : NanoXmlBase {
        private NanoXmlNode _rootNode;

        private List<NanoXmlAttribute> _declarations = new List<NanoXmlAttribute>();

        /// <summary>
        /// Public constructor. Loads xml document from raw string
        /// </summary>
        /// <param name="xmlString">String with xml</param>
        public NanoXmlDocument(string xmlString) {
            int i = 0;

            while (true) {
                SkipSpaces(xmlString, ref i);

                if (xmlString[i] != '<')
                    throw new XmlParsingException("Unexpected token");

                i++; // skip <

                if (xmlString[i] == '?') {
                    i++; // skip ?
                    ParseAttributes(xmlString, ref i, _declarations, '?', '>');
                    i++; // skip ending ?
                    i++; // skip ending >

                    continue;
                }

                if (xmlString[i] == '!') {
                    // doctype
                    while (xmlString[i] != '>') // skip doctype
                        i++;

                    i++; // skip >

                    continue;
                }

                _rootNode = new NanoXmlNode(xmlString, ref i);
                break;
            }
        }

        /// <summary>
        /// Root document element
        /// </summary>
        public NanoXmlNode RootNode {
            get { return _rootNode; }
        }

        /// <summary>
        /// List of XML Declarations as <see cref="NanoXmlAttribute"/>
        /// </summary>
        public IEnumerable<NanoXmlAttribute> Declarations {
            get { return _declarations; }
        }
    }

    /// <summary>
    /// Element node of document
    /// </summary>
    internal class NanoXmlNode : NanoXmlBase {
        private string _value;
        private string _name;

        private List<NanoXmlNode> _subNodes = new List<NanoXmlNode>();
        private List<NanoXmlAttribute> _attributes = new List<NanoXmlAttribute>();

        internal NanoXmlNode(string str, ref int i) {
            _name = ParseAttributes(str, ref i, _attributes, '>', '/');

            if (str[i] == '/') {
                // if this node has nothing inside
                i++; // skip /
                i++; // skip >
                return;
            }

            i++; // skip >

            // temporary. to include all whitespace into value, if any
            int tempI = i;

            SkipSpaces(str, ref tempI);

            if (str[tempI] == '<') {
                i = tempI;

                while (str[i + 1] != '/') {
                    // parse subnodes
                    i++; // skip <
                    _subNodes.Add(new NanoXmlNode(str, ref i));

                    SkipSpaces(str, ref i);

                    if (i >= str.Length)
                        return; // EOF

                    if (str[i] != '<')
                        throw new XmlParsingException("Unexpected token");
                }

                i++; // skip <
            } else {
                // parse value
                _value = GetValue(str, ref i, '<', '\0', false);
                i++; // skip <

                if (str[i] != '/')
                    throw new XmlParsingException("Invalid ending on tag " + _name);
            }

            i++; // skip /
            SkipSpaces(str, ref i);

            string endName = GetValue(str, ref i, '>', '\0', true);
            if (endName != _name)
                throw new XmlParsingException("Start/end tag name mismatch: " + _name + " and " + endName);
            SkipSpaces(str, ref i);

            if (str[i] != '>')
                throw new XmlParsingException("Invalid ending on tag " + _name);

            i++; // skip >
        }

        /// <summary>
        /// Element value
        /// </summary>
        public string Value {
            get { return _value; }
        }

        /// <summary>
        /// Element name
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// List of subelements
        /// </summary>
        public List<NanoXmlNode> SubNodes {
            get { return _subNodes; }
        }

        /// <summary>
        /// List of attributes
        /// </summary>
        public List<NanoXmlAttribute> Attributes {
            get { return _attributes; }
        }

        /// <summary>
        /// Returns subelement by given name
        /// </summary>
        /// <param name="nodeName">Name of subelement to get</param>
        /// <returns>First subelement with given name or NULL if no such element</returns>
        public NanoXmlNode this[string nodeName] {
            get {
                foreach (NanoXmlNode nanoXmlNode in _subNodes)
                    if (nanoXmlNode._name == nodeName)
                        return nanoXmlNode;

                return null;
            }
        }

        /// <summary>
        /// Returns attribute by given name
        /// </summary>
        /// <param name="attributeName">Attribute name to get</param>
        /// <returns><see cref="NanoXmlAttribute"/> with given name or null if no such attribute</returns>
        public NanoXmlAttribute GetAttribute(string attributeName) {
            foreach (NanoXmlAttribute nanoXmlAttribute in _attributes)
                if (nanoXmlAttribute.Name == attributeName)
                    return nanoXmlAttribute;

            return null;
        }

        /// <summary>
        /// Returns a list of all the descendant nodes with the given name (this method is kind of slow)
        /// </summary>
        /// <returns></returns>
        public List<NanoXmlNode> Descendants(string name) {
            var outNodes = new List<NanoXmlNode>();
            foreach (var node in SubNodes) {
                if (node.Name.EqualsCi(name))
                    outNodes.Add(node);
                outNodes.AddRange(node.Descendants(name));
            }
            return outNodes;
        }
    }

    /// <summary>
    /// XML element attribute
    /// </summary>
    internal class NanoXmlAttribute {
        private string _name;
        private string _value;

        /// <summary>
        /// Attribute name
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// Attribtue value
        /// </summary>
        public string Value {
            get { return _value; }
        }

        internal NanoXmlAttribute(string name, string value) {
            _name = name;
            _value = value;
        }
    }

    internal class XmlParsingException : Exception {
        public XmlParsingException(string message)
            : base(message) {}
    }
}