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
    public class Object2Xml<T> {

        private const string KeyString = "Key";
        private const string ValueString = "Value";
        private const string RootString = "Root";
        private const string InstanceListItemString = "Item";
        private const string PrefixToParentOfDico = "Dico";

        /// <summary>
        /// Saves an object of type T into an xml,
        /// It mirrors LoadFromFile
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void SaveToFile(T instance, string filename, bool valueInAttribute) {
            var x = new List<T> { instance };
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
            XElement itemElement = new XElement(InstanceListItemString);
            var properties = typeof(T).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) continue;

                XElement fieldElement = new XElement(property.Name);
                if (property.FieldType == typeof(Dictionary<string, string>)) {
                    itemElement.Add(DictToXml((Dictionary<string, string>)property.GetValue(item), PrefixToParentOfDico + property.Name, property.Name, valueInAttribute));
                } else {
                    if (property.FieldType == typeof(Color)) {
                        if (valueInAttribute)
                            fieldElement.Add(new XAttribute(ValueString, ColorTranslator.ToHtml((Color)property.GetValue(item))));
                        else
                            fieldElement.Add(ColorTranslator.ToHtml((Color)property.GetValue(item)));
                    } else if (property.FieldType.IsPrimitive || property.FieldType == typeof(string)) {
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
        public static void LoadFromFile(T instance, string filename, bool valueInAttribute) {
            var root = XDocument.Load(filename).Root;
            if (root != null)
                LoadFromXElement(instance, root.Descendants(InstanceListItemString).First(), valueInAttribute);
        }

        /// <summary>
        /// Load a list of object T from an xml file,
        /// it mirrors SaveToFile
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromFile(List<T> instance, string filename, bool valueInAttribute) {
            LoadFromXDocument(instance, XDocument.Load(filename), valueInAttribute);
        }

        /// <summary>
        /// Load an instance of objetc T from a XDocument,
        /// it mirros SaveToXDocument
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="document"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromXDocument(List<T> instance, XDocument document, bool valueInAttribute) {
            XElement rootElement = document.Root;
            if (rootElement == null) return;

            /* loop through InstanceListItemString elements */
            foreach (XElement itemElement in rootElement.Descendants(InstanceListItemString)) {
                T item = (T)Activator.CreateInstance(typeof(T), new object[] { });

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
        private static void LoadFromXElement(T item, XElement itemElement, bool valueInAttribute) {
            var properties = typeof(T).GetFields();

            /* loop through fields */
            foreach (var property in properties) {
                if (property.IsPrivate) continue;
                XElement elm = itemElement.Element(property.Name);
                if (elm == null) continue;

                /* dico */
                if (property.FieldType == typeof(Dictionary<string, string>)) {
                    property.SetValue(item, XmlToDictionary(elm, valueInAttribute));
                } else {

                    /* color > to hex */
                    if (property.FieldType == typeof(Color)) {
                        if (valueInAttribute) property.SetValue(item, ColorTranslator.FromHtml(elm.Attribute(ValueString).Value));
                        else property.SetValue(item, ColorTranslator.FromHtml(elm.Value));

                        /* other type */
                    } else if (property.FieldType.IsPrimitive || property.FieldType == typeof(string)) {
                        if (valueInAttribute) property.SetValue(item, TypeDescriptor.GetConverter(property.FieldType).ConvertFrom(elm.Attribute(ValueString).Value));
                        else property.SetValue(item, TypeDescriptor.GetConverter(property.FieldType).ConvertFrom(elm.Value));
                    }
                }
            }
        }

        private static Dictionary<string, string> XmlToDictionary(XElement baseElm, bool valueInAttribute) {
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
                    subElement = elm.Element(KeyString);
                    if (subElement != null)
                        dictVal = elm.Attribute(ValueString).Value;
                }
                if (!string.IsNullOrEmpty(dictKey))
                    dict.Add(dictKey, dictVal);
            }
            return dict;
        }

        private static XElement DictToXml(Dictionary<string, string> inputDict, string elmName, string subElmName, bool valueInAttribute) {
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
    }
    
}
