using System;

namespace YamuiFramework.HtmlRenderer.Core.Core.Entities {
    public sealed class BoxClickedEventArgs : EventArgs {

        /// <summary>
        /// The value of the attribute "clickable" of the box
        /// </summary>
        public readonly string Attribute;

        /// <summary>
        /// Number of clicks that triggers this shit
        /// </summary>
        public readonly int NumberOfClicks;

        public bool Handled;

        public BoxClickedEventArgs(string attribute, int numberOfClicks) {
            Attribute = attribute;
            NumberOfClicks = numberOfClicks;
        }
    }
}
