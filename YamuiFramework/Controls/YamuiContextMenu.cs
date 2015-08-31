using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace YamuiFramework.Controls {
    public class YamuiContextMenu : ContextMenuStrip {
        public YamuiContextMenu(IContainer container) {
            if (container != null) {
                container.Add(this);
            }

            UpdateTheme();     
        }

        public void UpdateTheme() {
            Renderer = new CtxRenderer(ThemeManager.FormColor.BackColor(), ThemeManager.AccentColor);  
            Invalidate();
        }

        private class CtxRenderer : ToolStripProfessionalRenderer {
            public CtxRenderer(Color backColor, Color accentColor) : base(new Contextcolors(backColor, accentColor)) { }
        }

        private class Contextcolors : ProfessionalColorTable {

            Color _backColor;
            Color _accentColor;

            public Contextcolors(Color backColor, Color accentColor) {
                _backColor = backColor;
                _accentColor = accentColor;
            }

            public override Color MenuItemSelected {
                get { return _accentColor; }
            }

            public override Color MenuBorder {
                get { return _accentColor; }
            }

            public override Color MenuItemBorder {
                get { return _accentColor; }
            }

            public override Color ImageMarginGradientBegin {
                get { return _backColor; }
            }

            public override Color ImageMarginGradientMiddle {
                get { return _backColor; }
            }

            public override Color ImageMarginGradientEnd {
                get { return _backColor; }
            }
        }
    }
}
