using System;
using System.IO;
using System.Reflection;

namespace _3PA.Lib {

    class AssemblyInfo {

        private static Assembly _assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets the title property
        /// </summary>
        public static string ProductTitle {
            get {
                return GetAttributeValue<AssemblyTitleAttribute>(a => a.Title,
                       Path.GetFileNameWithoutExtension(_assembly.CodeBase));
            }
        }

        /// <summary>
        /// Gets the application's version
        /// </summary>
        public static string Version {
            get {
                string result = string.Empty;
                Version version = _assembly.GetName().Version;
                if (version != null)
                    return version.ToString();
                else
                    return "1.0.0.0";
            }
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        public static string Description {
            get { return GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description); }
        }


        /// <summary>
        ///  Gets the product's full name.
        /// </summary>
        public static string Product {
            get { return GetAttributeValue<AssemblyProductAttribute>(a => a.Product); }
        }

        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        public static string Copyright {
            get { return GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright); }
        }

        /// <summary>
        /// Gets the company information for the product.
        /// </summary>
        public static string Company {
            get { return GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company); }
        }

        protected static string GetAttributeValue<TAttr>(Func<TAttr,
          string> resolveFunc, string defaultResult = null) where TAttr : Attribute {
            object[] attributes = _assembly.GetCustomAttributes(typeof(TAttr), false);
            if (attributes.Length > 0)
                return resolveFunc((TAttr)attributes[0]);
            else
                return defaultResult;
        }
    }
}
