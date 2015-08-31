using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace YamuiFramework.Helper {
    public class ControlHelper {

        /// <summary>
        /// List all the controls children of "control" of type "type"
        /// this is recursive, so it find them all
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Control> GetAll(Control control, Type type) {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }

        /// <summary>
        /// Get the first control of the type type it can find
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Control GetFirst(Control control, Type type) {
            return control.Controls.Cast<object>().Where(control1 => control1.GetType() == type).Cast<Control>().FirstOrDefault();
        }
    }
}
