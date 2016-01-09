#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (Object2Xml.cs) is part of 3P.
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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace _3PA.Lib {
    /// <summary>
    /// A class to read and write an object instance
    /// FYI : no fields of the T class can be null when saving an instance
    /// TODO: save more than a simple string, string dico
    /// <remarks>/!\ THIS CLASS IS DUPLICATED IN YAMUIFRAMEWORK!!!!!!!!</remarks>
    /// </summary>
    public static class Object2Xml<T> {

        #region Fields

        private const string RootString = "Root";
        private const string KeyString = "Key";
        private const string ValueString = "Value";
        private const string PrefixToParentOfDico = "Dico";

        #endregion


        #region Save methods

        /// <summary>
        /// Saves an object of type T into an xml,
        /// It mirrors LoadFromFile
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void SaveToFile(T instance, string filename, bool valueInAttribute = false) {
            var x = new List<T> {instance};
            SaveToFile(x, filename, valueInAttribute);
        }

        /// <summary>
        /// Method to save a list of object of type T into an xml,
        /// It mirrors LoadFromFile
        /// </summary>
        /// <param name="instance">your list of item</param>
        /// <param name="filename">destination</param>
        /// <param name="valueInAttribute">set to true will store all values in attributes</param>
        public static void SaveToFile(List<T> instance, string filename, bool valueInAttribute = false) {
            SaveToXDocument(instance, valueInAttribute).Save(filename);
        }

        /// <summary>
        /// Saves a list of object into an xml document
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="valueInAttribute"></param>
        /// <returns></returns>
        public static XDocument SaveToXDocument(List<T> instance, bool valueInAttribute = false) {
            XDocument document = new XDocument();
            XElement root = new XElement(RootString);

            /* loop through list */
            foreach (T item in instance) {
                root.Add(SaveToXElement(item, valueInAttribute));
            }
            document.Add(root);
            return document;
        }

        /// <summary>
        /// Saves an object to an XElement,
        /// it mirrors LoadFromXElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="valueInAttribute"></param>
        /// <returns></returns>
        public static XElement SaveToXElement(T item, bool valueInAttribute = false) {
            XElement itemElement = new XElement(typeof (T).Name);
            var properties = typeof (T).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) continue;

                XElement fieldElement = new XElement(property.Name);
                if (property.FieldType == typeof (Dictionary<string, string>)) {
                    itemElement.Add(DictToXml((Dictionary<string, string>) property.GetValue(item), PrefixToParentOfDico + property.Name, property.Name, valueInAttribute));
                } else {
                    if (property.FieldType == typeof (Color)) {
                        if (valueInAttribute)
                            fieldElement.Add(new XAttribute(ValueString, ColorTranslator.ToHtml((Color) property.GetValue(item))));
                        else
                            fieldElement.Add(ColorTranslator.ToHtml((Color) property.GetValue(item)));
                    } else if (property.FieldType.IsPrimitive || property.FieldType == typeof (string)) {
                        // case of a simple value
                        if (valueInAttribute)
                            fieldElement.Add(new XAttribute(ValueString, property.GetValue(item)));
                        else
                            fieldElement.Add(property.GetValue(item));
                        //elm.Add(Convert.ChangeType(property.GetValue(instance), typeof(string)));
                    }
                    itemElement.Add(fieldElement);
                }
            }
            return itemElement;
        }

        #endregion


        #region Load methods

        /// <summary>
        /// This reads xmlContent and load a list of item from it,
        /// it has no mirrored save function
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="xmlContent"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromString(List<T> instance, string xmlContent, bool valueInAttribute = false) {
            LoadFromXDocument(instance, XDocument.Parse(xmlContent), valueInAttribute);
        }

        /// <summary>
        /// Load an instance of the object T from an xml file,
        /// it mirrors SaveToFile
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromFile(T instance, string filename, bool valueInAttribute = false) {
            var root = XDocument.Load(filename).Root;
            if (root != null)
                LoadFromXElement(instance, root.Descendants(typeof (T).Name).First(), valueInAttribute);
        }

        /// <summary>
        /// Load a list of object T from an xml file,
        /// it mirrors SaveToFile
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromFile(List<T> instance, string filename, bool valueInAttribute = false) {
            LoadFromXDocument(instance, XDocument.Load(filename), valueInAttribute);
        }

        /// <summary>
        /// Load an instance of object T from a XDocument,
        /// it mirros SaveToXDocument
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="document"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromXDocument(List<T> instance, XDocument document, bool valueInAttribute = false) {
            XElement rootElement = document.Root;
            if (rootElement == null) return;

            /* loop through InstanceListItemString elements */
            foreach (XElement itemElement in rootElement.Descendants(typeof (T).Name)) {
                T item = (T) Activator.CreateInstance(typeof (T), new object[] {});
                LoadFromXElement(item, itemElement, valueInAttribute);
                instance.Add(item);
            }
        }

        /// <summary>
        /// Load an instance of object T from an XElement,
        /// it mirrors SaveToXElement
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemElement"></param>
        /// <param name="valueInAttribute"></param>
        private static void LoadFromXElement(T item, XElement itemElement, bool valueInAttribute = false) {
            var properties = typeof (T).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) continue;

                /* dico */
                if (property.FieldType == typeof (Dictionary<string, string>)) {
                    XElement elm = itemElement.Element(PrefixToParentOfDico + property.Name);
                    if (elm == null) continue;

                    property.SetValue(item, XmlToDictionary(elm, valueInAttribute));
                } else {
                    XElement elm = itemElement.Element(property.Name);
                    if (elm == null) continue;

                    /* color > to hex */
                    if (property.FieldType == typeof (Color)) {
                        if (valueInAttribute) property.SetValue(item, ColorTranslator.FromHtml(elm.Attribute(ValueString).Value));
                        else property.SetValue(item, ColorTranslator.FromHtml(elm.Value));

                        /* other type */
                    } else if (property.FieldType.IsPrimitive || property.FieldType == typeof (string)) {
                        var converter = TypeDescriptor.GetConverter(property.FieldType);
                        if (valueInAttribute) property.SetValue(item, converter.ConvertFromInvariantString(elm.Attribute(ValueString).Value));
                        else property.SetValue(item, converter.ConvertFromInvariantString(elm.Value));
                    }
                }
            }
        }

        #endregion


        #region Dictionnary

        private static Dictionary<string, string> XmlToDictionary(XElement baseElm, bool valueInAttribute = false) {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (XElement elm in baseElm.Elements()) {
                string dictKey = "";
                string dictVal = "";
                if (valueInAttribute) {
                    dictKey = elm.Attribute(KeyString).Value;
                    dictVal = elm.Attribute(ValueString).Value;
                } else {
                    XElement subElement = elm.Element(KeyString);
                    if (subElement != null)
                        dictKey = subElement.Value;
                    subElement = elm.Element(ValueString);
                    if (subElement != null)
                        dictVal = subElement.Value;
                }
                // if a key is not unique we will only take the first found!
                if (!string.IsNullOrEmpty(dictKey) && !dict.ContainsKey(dictKey))
                    dict.Add(dictKey, dictVal);
            }
            return dict;
        }

        private static XElement DictToXml(Dictionary<string, string> inputDict, string elmName, string subElmName, bool valueInAttribute = false) {
            XElement outElm = new XElement(elmName);
            Dictionary<string, string>.KeyCollection keys = inputDict.Keys;
            foreach (string key in keys) {
                XElement inner = new XElement(subElmName);
                if (valueInAttribute) {
                    inner.Add(new XAttribute(KeyString, key));
                    inner.Add(new XAttribute(ValueString, inputDict[key]));
                } else {
                    inner.Add(new XElement(KeyString, key));
                    inner.Add(new XElement(ValueString, inputDict[key]));
                }
                outElm.Add(inner);
            }
            return outElm;
        }

        #endregion

    }
    
}
