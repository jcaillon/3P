using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Linq;

namespace YamuiFramework.Themes {
    /// <summary>
    /// A class to read and write an object instance
    /// </summary>
    internal class Class2Xml<T> {

        private const string KeyString = "Key";
        private const string ValueString = "Value";
        private const string RootString = "Root";
        private const string InstanceListItemString = "Item";
        private const string PrefixToParentOfDico = "Dico";

        /// <summary>
        /// Saves an object of type <T> into an xml
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void SaveToFile(T instance, string filename, bool valueInAttribute = false) {
            var x = new List<T> { instance };
            SaveToFile(x, filename, valueInAttribute);
        }

        /// <summary>
        /// Method to save a list of object of type <T> into an xml
        /// </summary>
        /// <param name="instance">your list of item</param>
        /// <param name="filename">destination</param>
        /// <param name="valueInAttribute">set to true will store all values in attributes</param>
        public static void SaveToFile(List<T> instance, string filename, bool valueInAttribute = false) {
            XDocument document = new XDocument();
            XElement root = new XElement(RootString);

            /* loop through list */
            foreach (T item in instance) {
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
                root.Add(itemElement);
            }
            document.Add(root);
            document.Save(filename);
        }

        /// <summary>
        /// This reads xmlContent and load a list of item from it
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="xmlContent"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromRaw(List<T> instance, string xmlContent, bool valueInAttribute = false) {
            Load(instance, XDocument.Parse(xmlContent), valueInAttribute);
        }

        /// <summary>
        /// Reads an xml to instanciate an object
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="filename"></param>
        /// <param name="valueInAttribute"></param>
        public static void LoadFromFile(T instance, string filename, bool valueInAttribute = false) {
            var x = new List<T> {instance};
            LoadFromFile(x, filename, valueInAttribute);
            instance = x[0];
        }

        public static void LoadFromFile(List<T> instance, string filename, bool valueInAttribute = false) {
            Load(instance, XDocument.Load(filename), valueInAttribute);
        }

        private static void Load(List<T> instance, XDocument document, bool valueInAttribute = false) {
            XElement rootElement = document.Root;
            if (rootElement == null) return;

            var properties = typeof(T).GetFields();

            /* loop through InstanceListItemString elements */
            foreach (XElement itemElement in rootElement.Descendants(InstanceListItemString)) {
                T item = (T)Activator.CreateInstance(typeof(T), new object[] { });

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

                instance.Add(item);
            }
        }

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
                    subElement = elm.Element(KeyString);
                    if (subElement != null)
                        dictVal = elm.Attribute(ValueString).Value;
                }
                if (!string.IsNullOrEmpty(dictKey))
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
    }
    
}
