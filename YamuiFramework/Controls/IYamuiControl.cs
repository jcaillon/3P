using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YamuiFramework.Controls {
    interface IYamuiControl {

        /// <summary>
        /// Should call the protected method UpdateBounds which... updates bounds
        /// </summary>
        void UpdateBoundsPublic();

    }
}