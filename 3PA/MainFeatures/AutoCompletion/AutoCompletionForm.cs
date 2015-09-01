using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Controls;
using _3PA.Images;
using _3PA.Lib;

namespace YamuiDemoApp {
    public partial class AutoCompletionForm : Form {
        public AutoCompletionForm(List<CompletionData> objectsList) {
            InitializeComponent();
            Keyword.ImageGetter += rowObject => {
                var x = (CompletionData)rowObject;
                return (int)x.Type;
            };
            /*Keyword.ImageGetter += rowObject => {
                var x = (CompletionData) rowObject;
                switch (x.Type) {
                    case CompletionType.Keyword:
                        return Resources.autocompletion_keyword;
                    case CompletionType.Table:
                        return Resources.autocompletion_all;
                }
                return Resources.autocompletion_all;
            };*/
            fastOLV.SetObjects(objectsList);
            
        }

        private void yamuiButton1_ButtonPressed(object sender, YamuiFramework.Controls.ButtonPressedEventArgs e) {
            fastOLV.ListFilter = new TailFilter(500);
            //fastOLV.ClearObjects();
            
        }

        private void yamuiTextBox1_Click(object sender, EventArgs e) {

        }

        private void yamuiTextBox1_TextChanged(object sender, EventArgs e) {
            var x = (YamuiTextBox) sender;
            TextMatchFilter filter = TextMatchFilter.Contains(this.fastOLV, x.Text);
            fastOLV.ModelFilter = filter;
            fastOLV.DefaultRenderer = new HighlightTextRenderer(filter);
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            var x = (TextBox)sender;
            TextMatchFilter filter = TextMatchFilter.Contains(this.fastOLV, x.Text);
            fastOLV.DefaultRenderer = new HighlightTextRenderer(filter);
            fastOLV.AdditionalFilter = filter;
        }
    }
}
