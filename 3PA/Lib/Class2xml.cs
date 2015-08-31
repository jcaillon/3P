using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace _3PA.Lib {

    /// <summary>
    /// A class to read and write a ConfigObject instance
    /// </summary>
    class Class2Xml {
        //TODO: Rendre la classe generique!

        public static void Save(ConfigObject instance, string filename) {
            XDocument document = new XDocument();
            XElement root = new XElement("Root");

            var properties = typeof(ConfigObject).GetFields();
            foreach (var property in properties) {
                XElement elm = new XElement(property.Name);
                if (property.FieldType.IsPrimitive || property.FieldType == typeof(string)) {
                    // case of a simple value
                    elm.Add(property.GetValue(instance));
                    root.Add(elm);
                }
            }

            // for the sortcuts dico
            root.Add(DictToXml(instance.ShortCuts, "Shortcuts", "Shortcut", "Function", "ShortKey"));

            document.Add(root);
            document.Save(filename);
        }

        public static void Load(ConfigObject instance, string filename) {
            XDocument document = XDocument.Load(filename);
            XElement baseElm = document.Root;
            if (baseElm == null) return;

            var properties = typeof(ConfigObject).GetFields();
            foreach (var property in properties) {
                XElement elm = baseElm.Element(property.Name);
                if (elm != null) {
                    property.SetValue(instance, TypeDescriptor.GetConverter(property.FieldType).ConvertFrom(elm.Value));
                }
            }
            XElement shElement = baseElm.Element("Shortcuts");
            if (shElement != null)
                instance.ShortCuts = XmlToDictionary(shElement, "Function", "ShortKey");
        }

        public static Dictionary<string, string> XmlToDictionary (XElement baseElm, string keyName, string valueName) {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (XElement elm in baseElm.Elements()) {
                string dictKey = elm.Attribute(keyName).Value;
                string dictVal = elm.Attribute(valueName).Value;
                dict.Add(dictKey, dictVal);
            }
            return dict;
        }

        public static XElement DictToXml (Dictionary<string, string> inputDict, string elmName, string subElmName, string keyName, string valueName) {
            XElement outElm = new XElement(elmName);
            Dictionary<string, string>.KeyCollection keys = inputDict.Keys;
            foreach (string key in keys) {
                XElement inner = new XElement(subElmName);
                inner.Add(new XAttribute(keyName, key));
                inner.Add(new XAttribute(valueName, inputDict[key]));
                outElm.Add(inner);
            }
            return outElm;
        }
    }
}
