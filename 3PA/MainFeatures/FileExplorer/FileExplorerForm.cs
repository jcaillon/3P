#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileExplorerForm.cs) is part of 3P.
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using YamuiFramework.Animations.Transitions;
using YamuiFramework.Fonts;
using _3PA.Images;
using _3PA.Lib;
using _3PA.MainFeatures.AutoCompletion;
using _3PA.MainFeatures.FilesInfoNs;
using _3PA.MainFeatures.FilteredLists;
using _3PA.MainFeatures.NppInterfaceForm;
using _3PA.MainFeatures.ProgressExecutionNs;

namespace _3PA.MainFeatures.FileExplorer {

    internal partial class FileExplorerForm : NppDockableDialog {

        #region Fields
        private const string StrEmptyList = "No files found!";
        private const string StrItems = " items";

        private string[] _explorerDirStr;

        /// <summary>
        /// The filter to apply to the autocompletion form
        /// </summary>
        public string FilterByText {
            get { return _filterByText; }
            set { _filterByText = value.ToLower(); ApplyFilter(); }
        }

        /// <summary>
        /// Lowered case filter string
        /// </summary>
        private static string _filterByText = "";

        /// <summary>
        ///  gets or sets the total items currently displayed in the form
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// The value ranging from 0 to 3 and indicating which folder we are exploring
        /// </summary>
        public int DirectoryToExplorer { get; private set; }

        // List of displayed type of file
        private static Dictionary<FileType, SelectorButton<FileType>> _displayedTypes;

        // remember the list that was passed to the autocomplete form when we set the items, we need this
        // because we reorder the list each time the user filters stuff, but we need the original order
        private List<FileListItem> _initialObjectsList;

        private int _currentType;

        private static bool _isListing;
        #endregion

        #region constructor

        public FileExplorerForm(EmptyForm formToCover)
            : base(formToCover) {
            InitializeComponent();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);


            StartPosition = FormStartPosition.Manual;

            #region Current file

            // register to Updated Operation events
            FilesInfo.OnUpdatedOperation += FilesInfoOnUpdatedOperation;
            FilesInfo.OnUpdatedErrors += FilesInfoOnUpdatedErrors;

            btGetHelp.BackGrndImage = (Config.Instance.GlobalShowDetailedHelpForErrors) ? ImageResources.GetHelp : Utils.MakeGrayscale3(ImageResources.GetHelp);
            UpdateErrorButtons(false);

            btGetHelp.ButtonPressed += BtGetHelpOnButtonPressed;
            btPrevError.ButtonPressed += BtPrevErrorOnButtonPressed;
            btNextError.ButtonPressed += BtNextErrorOnButtonPressed;
            btClearAllErrors.ButtonPressed += BtClearAllErrorsOnButtonPressed;

            toolTipHtml.SetToolTip(btGetHelp, "Toggle on/off the <b>detailed help</b> for compilation errors and warnings");
            toolTipHtml.SetToolTip(btPrevError, "<b>Move the caret</b> to the previous error");
            toolTipHtml.SetToolTip(btNextError, "<b>Move the caret</b> to the next error");
            toolTipHtml.SetToolTip(btClearAllErrors, "<b>Clear</b> all the displayed errors");
            toolTipHtml.SetToolTip(lbStatus, "Provides information on the current status of the file");

            lbStatus.BackColor = ThemeManager.Current.FormBack;

            #endregion

            #region File list

            // Image getter
            FileName.ImageGetter += ImageGetter;

            // Style the control
            StyleOvlTree();

            // Register to events
            fastOLV.DoubleClick += FastOlvOnDoubleClick;
            fastOLV.KeyDown += FastOlvOnKeyDown;
            fastOLV.Click += FastOlvOnClick;
            fastOLV.CellRightClick += FastOlvOnCellRightClick;

            // decorate rows
            fastOLV.UseCellFormatEvents = true;
            fastOLV.FormatCell += FastOlvOnFormatCell;

            // problems with the width of the column, set here
            FileName.Width = fastOLV.Width - 17;
            fastOLV.ClientSizeChanged += (sender, args) => FileName.Width = fastOLV.Width - 17;

            // button images
            btErase.BackGrndImage = ImageResources.eraser;
            btRefresh.BackGrndImage = ImageResources.refresh;

            // events
            textFilter.TextChanged += TextFilterOnTextChanged;
            textFilter.KeyDown += TextFilterOnKeyDown;
            btRefresh.ButtonPressed += BtRefreshOnButtonPressed;
            btErase.ButtonPressed += BtEraseOnButtonPressed;

            // button tooltips
            toolTipHtml.SetToolTip(btErase, "<b>Erase</b> the content of the text filter");
            toolTipHtml.SetToolTip(btRefresh, "Click this button to <b>refresh</b> the list of files for the current directory<br>No automatic refreshing is done so you have to use this button when you add/delete a file in said directory");
            toolTipHtml.SetToolTip(textFilter, "Start writing a file name to <b>filter</b> the list below");
            toolTipHtml.SetToolTip(btGotoDir, "<b>Open</b> the current path in the windows explorer");
            toolTipHtml.SetToolTip(btDirectory, "Click to <b>change</b> the directory to explore");
            toolTipHtml.SetToolTip(lbDirectory, "Current directory being explored");

            // default to "everywhere"
            DirectoryToExplorer = 3;

            btGotoDir.BackGrndImage = ImageResources.OpenInExplorer;
            btDirectory.BackGrndImage = ImageResources.ExplorerDir3;
            _explorerDirStr = new[] { "Local path ", "Compilation path", "Propath", "Everywhere" };
            lbDirectory.Text = _explorerDirStr[DirectoryToExplorer];
            btDirectory.ButtonPressed += BtDirectoryOnButtonPressed;
            btGotoDir.ButtonPressed += BtGotoDirOnButtonPressed;

            #endregion

        }

        #endregion

        #region core

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.Clear(ThemeManager.Current.FormBack);
        }

        #endregion

        #region events

        /// <summary>
        /// Check/uncheck the menu depending on this form visibility
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e) {
            FileExplorer.UpdateMenuItemChecked();
            base.OnVisibleChanged(e);
        }

        #endregion

        #endregion

        #region File list

        #region cell formatting and style ovl

        /// <summary>
        /// Return the image that needs to be display on the left of an item
        /// representing its type
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        private static Image GetImageFromStr(string typeStr) {
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject(typeStr);
            return tryImg ?? ImageResources.Error;
        }

        /// <summary>
        /// Image getter for object rows
        /// </summary>
        /// <param name="rowObject"></param>
        /// <returns></returns>
        private static object ImageGetter(object rowObject) {
            var obj = (FileListItem)rowObject;
            if (obj == null) return ImageResources.Error;
            return GetImageFromStr(obj.Type + "Type");
        }

        /// <summary>
        /// Event on format cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void FastOlvOnFormatCell(object sender, FormatCellEventArgs args) {
            FileListItem obj = (FileListItem)args.Model;

            // currently document
            if (obj.FullPath.Equals(Plug.CurrentFilePath)) {
                RowBorderDecoration rbd = new RowBorderDecoration {
                    FillBrush = new SolidBrush(Color.FromArgb(50, ThemeManager.Current.MenuFocusBack)),
                    BorderPen = new Pen(Color.FromArgb(128, ThemeManager.Current.MenuFocusFore), 1),
                    BoundsPadding = new Size(-2, 0),
                    CornerRounding = 6.0f
                };
                args.SubItem.Decoration = rbd;
            }

            // display the flags
            int offset = -5;
            foreach (var name in Enum.GetNames(typeof(FileFlag))) {
                FileFlag flag = (FileFlag)Enum.Parse(typeof(FileFlag), name);
                if (flag == 0) continue;
                if (!obj.Flags.HasFlag(flag)) continue;
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                if (tryImg == null) continue;
                ImageDecoration decoration = new ImageDecoration(tryImg, 100, ContentAlignment.MiddleRight) {
                    Offset = new Size(offset, 0)
                };
                if (args.SubItem.Decoration == null)
                    args.SubItem.Decoration = decoration;
                else
                    args.SubItem.Decorations.Add(decoration);
                offset -= 20;
            }

            // display the sub string
            if (offset < -5) offset -= 5;
            if (!string.IsNullOrEmpty(obj.SubString)) {
                TextDecoration decoration = new TextDecoration(obj.SubString, 100) {
                    Alignment = ContentAlignment.MiddleRight,
                    Offset = new Size(offset, 0),
                    Font = FontManager.GetFont(FontStyle.Bold, 11),
                    TextColor = ThemeManager.Current.SubTextFore,
                    CornerRounding = 1f,
                    Rotation = 0,
                    BorderWidth = 1,
                    BorderColor = ThemeManager.Current.SubTextFore
                };
                args.SubItem.Decorations.Add(decoration);
            }
        }

        /// <summary>
        /// Apply thememanager theme to the treeview
        /// </summary>
        public void StyleOvlTree() {
            // Style the control
            fastOLV.OwnerDraw = true;
            fastOLV.Font = FontManager.GetFont(FontFunction.AutoCompletion);
            fastOLV.BackColor = ThemeManager.Current.FormBack;
            fastOLV.AlternateRowBackColor = ThemeManager.Current.FormAltBack;
            fastOLV.ForeColor = ThemeManager.Current.FormFore;
            fastOLV.HighlightBackgroundColor = ThemeManager.Current.MenuFocusBack;
            fastOLV.HighlightForegroundColor = ThemeManager.Current.MenuFocusFore;
            fastOLV.UnfocusedHighlightBackgroundColor = fastOLV.HighlightBackgroundColor;
            fastOLV.UnfocusedHighlightForegroundColor = fastOLV.HighlightForegroundColor;

            // Decorate and configure hot item
            fastOLV.UseHotItem = true;
            fastOLV.HotItemStyle = new HotItemStyle {
                BackColor = ThemeManager.Current.MenuHoverBack,
                ForeColor = ThemeManager.Current.MenuHoverFore
            };

            // overlay of empty list :
            fastOLV.EmptyListMsg = StrEmptyList;
            TextOverlay textOverlay = fastOLV.EmptyListMsgOverlay as TextOverlay;
            if (textOverlay != null) {
                textOverlay.TextColor = ThemeManager.Current.FormFore;
                textOverlay.BackColor = ThemeManager.Current.FormAltBack;
                textOverlay.BorderColor = ThemeManager.Current.FormFore;
                textOverlay.BorderWidth = 4.0f;
                textOverlay.Font = FontManager.GetFont(FontStyle.Bold, 30f);
                textOverlay.Rotation = -5;
            }

            fastOLV.UseAlternatingBackColors = Config.Instance.GlobalUseAlternateBackColorOnGrid;

            fastOLV.DefaultRenderer = new FilteredItemTextRenderer();
        }

        #endregion

        #region Refresh file list and selector mechanic

        /// <summary>
        /// Call this method to completly refresh the object view list (recompute the items of the list)
        /// </summary>
        public void RefreshFileList() {
            if (_isListing) return;
            _isListing = true;
            Task.Factory.StartNew(() => {
                try {
                    RefreshFileListAction();
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while listing files");
                    _isListing = false;
                }
            });
        }

        public void RefreshFileListAction() {
            // get the list of FileObjects
            _initialObjectsList = new List<FileListItem>();
            switch (DirectoryToExplorer) {
                case 0:
                    _initialObjectsList = FileExplorer.ListFileOjectsInDirectory(ProEnvironment.Current.BaseLocalPath);
                    break;
                case 1:
                    _initialObjectsList = FileExplorer.ListFileOjectsInDirectory(ProEnvironment.Current.BaseCompilationPath);
                    break;
                case 2:
                    foreach (var dir in ProEnvironment.Current.GetProPathDirList) {
                        _initialObjectsList.AddRange(FileExplorer.ListFileOjectsInDirectory(dir, false, false));
                    }
                    break;
                default:
                    // get the list of FileObjects
                    Regex regex = new Regex(@"\\\.");
                    var fullList = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
                    fullList.Add(ProEnvironment.Current.BaseLocalPath);
                    if (!fullList.Contains(ProEnvironment.Current.BaseCompilationPath))
                        fullList.Add(ProEnvironment.Current.BaseCompilationPath);
                    // base local path
                    if (Directory.Exists(ProEnvironment.Current.BaseLocalPath)) {
                        foreach (var directory in Directory.GetDirectories(ProEnvironment.Current.BaseLocalPath, "*", SearchOption.AllDirectories)) {
                            if (!fullList.Contains(directory) && (!Config.Instance.FileExplorerIgnoreUnixHiddenFolders || !regex.IsMatch(directory)))
                                fullList.Add(directory);
                        }
                    }
                    // base compilation path
                    if (Directory.Exists(ProEnvironment.Current.BaseCompilationPath)) {
                        foreach (var directory in Directory.GetDirectories(ProEnvironment.Current.BaseCompilationPath, "*", SearchOption.AllDirectories)) {
                            if (!fullList.Contains(directory) && (!Config.Instance.FileExplorerIgnoreUnixHiddenFolders || !regex.IsMatch(directory)))
                                fullList.Add(directory);
                        }
                    }
                    // for each dir in propath
                    foreach (var directory in ProEnvironment.Current.GetProPathDirList) {
                        if (!fullList.Contains(directory) && (!Config.Instance.FileExplorerIgnoreUnixHiddenFolders || !regex.IsMatch(directory)))
                            fullList.Add(directory);
                    }
                    foreach (var path in fullList) {
                        _initialObjectsList.AddRange(FileExplorer.ListFileOjectsInDirectory(path, false));
                    }
                    break;
            }

            // apply custom sorting
            _initialObjectsList.Sort(new FilesSortingClass());

            // invoke on ui thread
            BeginInvoke((Action)delegate {
                try {
                    // delete any existing buttons
                    if (_displayedTypes != null) {
                        foreach (var selectorButton in _displayedTypes) {
                            selectorButton.Value.ButtonPressed -= HandleTypeClick;
                            if (Controls.Contains(selectorButton.Value))
                                Controls.Remove(selectorButton.Value);
                            selectorButton.Value.Dispose();
                        }
                    }

                    // get distinct types, create a button for each
                    int xPos = 59;
                    int yPox = Height - 28;
                    _displayedTypes = new Dictionary<FileType, SelectorButton<FileType>>();
                    foreach (var type in _initialObjectsList.Select(x => x.Type).Distinct()) {
                        var but = new SelectorButton<FileType> {
                            BackGrndImage = GetImageFromStr(type + "Type"),
                            Activated = true,
                            Size = new Size(24, 24),
                            TabStop = false,
                            Location = new Point(xPos, yPox),
                            Type = type,
                            AcceptsRightClick = true,
                            Anchor = AnchorStyles.Left | AnchorStyles.Bottom
                        };
                        but.ButtonPressed += HandleTypeClick;
                        toolTipHtml.SetToolTip(but, "Type of item : <b>" + type + "</b>:<br><br><b>Left click</b> to toggle on/off this filter<br><b>Right click</b> to filter for this type only");
                        _displayedTypes.Add(type, but);
                        Controls.Add(but);
                        xPos += but.Width;
                    }

                    // label for the number of items
                    TotalItems = _initialObjectsList.Count;
                    nbitems.Text = TotalItems + StrItems;
                    fastOLV.SetObjects(_initialObjectsList);
                } catch (Exception e) {
                    ErrorHandler.ShowErrors(e, "Error while showing the list of files");
                } finally {
                    _isListing = false;
                }
            });
            ApplyFilter();
        }

        /// <summary>
        /// use this to programmatically uncheck any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetActiveType(List<FileType> allowedType) {
            if (_displayedTypes == null) return;
            if (allowedType == null) allowedType = new List<FileType>();
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) >= 0;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// use this to programmatically check any type that is not in the given list
        /// </summary>
        /// <param name="allowedType"></param>
        public void SetUnActiveType(List<FileType> allowedType) {
            if (_displayedTypes == null) return;
            if (allowedType == null) allowedType = new List<FileType>();
            foreach (var selectorButton in _displayedTypes) {
                selectorButton.Value.Activated = allowedType.IndexOf(selectorButton.Value.Type) < 0;
                selectorButton.Value.Invalidate();
            }
        }

        /// <summary>
        /// allows to programmatically select the first item of the list
        /// </summary>
        public void SelectFirstItem() {
            if (TotalItems > 0) fastOLV.SelectedIndex = 0;
        }

        #endregion

        #region events

        /// <summary>
        /// Executed when the user double click an item or press enter
        /// </summary>
        public void OnActivateItem() {
            var curItem = GetCurrentFile();
            if (curItem == null)
                return;

            Utils.OpenAnyLink(curItem.FullPath);
        }

        /// <summary>
        /// handles click on a type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleTypeClick(object sender, EventArgs args) {
            var mouseEvent = args as MouseEventArgs;
            FileType clickedType = ((SelectorButton<FileType>)sender).Type;

            // on right click
            if (mouseEvent != null && mouseEvent.Button == MouseButtons.Right) {
                // everything is unactive but this one
                if (_displayedTypes.Count(b => b.Value.Activated) == 1 && _displayedTypes.First(b => b.Value.Activated).Key == clickedType) {
                    SetUnActiveType(null);
                } else {
                    SetActiveType(new List<FileType> { clickedType });
                }
            } else
                // left click is only a toggle
                _displayedTypes[clickedType].Activated = !_displayedTypes[clickedType].Activated;

            _displayedTypes[clickedType].Invalidate();
            ApplyFilter();
        }

        /// <summary>
        /// handles double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void FastOlvOnDoubleClick(object sender, EventArgs eventArgs) {
            OnActivateItem();
        }

        /// <summary>
        /// Handles keydown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyEventArgs"></param>
        private void FastOlvOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = OnKeyDown(keyEventArgs.KeyCode);
        }

        private void FastOlvOnClick(object sender, EventArgs eventArgs) {
            if (!KeyboardMonitor.GetModifiers.IsAlt)
                return;
            var curItem = GetCurrentFile();
            if (curItem != null) {
                // remove or add favourite flag
                if (curItem.Flags.HasFlag(FileFlag.Favourite))
                    curItem.Flags &= ~FileFlag.Favourite;
                else
                    curItem.Flags |= FileFlag.Favourite;
            }
        }

        private void FastOlvOnCellRightClick(object sender, CellRightClickEventArgs cellRightClickEventArgs) {
            var fileObj = (FileListItem)cellRightClickEventArgs.Model;
            if (fileObj != null) {
                Utils.OpenFileInFolder(fileObj.FullPath);
                cellRightClickEventArgs.Handled = true;
            }
        }

        #endregion

        #region on key events

        public bool OnKeyDown(Keys key) {
            bool handled = true;
            // down and up change the selection
            if (key == Keys.Up) {
                if (fastOLV.SelectedIndex > 0)
                    fastOLV.SelectedIndex--;
                else
                    fastOLV.SelectedIndex = (TotalItems - 1);
                if (fastOLV.SelectedIndex >= 0)
                    fastOLV.EnsureVisible(fastOLV.SelectedIndex);
            } else if (key == Keys.Down) {
                if (fastOLV.SelectedIndex < (TotalItems - 1))
                    fastOLV.SelectedIndex++;
                else
                    fastOLV.SelectedIndex = 0;
                if (fastOLV.SelectedIndex >= 0)
                    fastOLV.EnsureVisible(fastOLV.SelectedIndex);

                // escape close
            } else if (key == Keys.Escape) {
                Npp.GrabFocus();

                // left and right keys
            } else if (key == Keys.Left) {
                handled = LeftRight(true);

            } else if (key == Keys.Right) {
                handled = LeftRight(false);

                // enter and tab accept the current selection
            } else if (key == Keys.Enter) {
                OnActivateItem();

            } else if (key == Keys.Tab) {
                OnActivateItem();
                GiveFocustoTextBox();

                // else, any other key is unhandled
            } else {
                handled = false;
            }

            // down and up activate the display of tooltip
            if (key == Keys.Up || key == Keys.Down) {
                // TODO
                //InfoToolTip.InfoToolTip.ShowToolTipFromAutocomplete(GetCurrentSuggestion(), new Rectangle(new Point(Location.X, Location.Y), new Size(Width, Height)), _isReversed);
            }
            return handled;
        }

        private bool LeftRight(bool isLeft) {
            // Alt must be pressed
            if (!KeyboardMonitor.GetModifiers.IsAlt)
                return false;

            // only 1 type is active
            if (_displayedTypes.Count(b => b.Value.Activated) == 1)
                _currentType = _displayedTypes.FindIndex(pair => pair.Value.Activated) + (isLeft ? -1 : 1);
            if (_currentType > _displayedTypes.Count - 1) _currentType = 0;
            if (_currentType < 0) _currentType = _displayedTypes.Count - 1;
            SetActiveType(new List<FileType> { _displayedTypes.ElementAt(_currentType).Key });
            ApplyFilter();
            return true;
        }

        #endregion

        #region Filter

        /// <summary>
        /// this methods sorts the items to put the best match on top and then filter it with modelFilter
        /// </summary>
        private void ApplyFilter() {
            if (_initialObjectsList == null || _initialObjectsList.Count == 0)
                return;

            // apply filter to each item in the list then set the list
            _initialObjectsList.ForEach(data => data.FilterApply(_filterByText));
            if (string.IsNullOrEmpty(_filterByText)) {
                fastOLV.SetObjects(_initialObjectsList);
            } else {
                fastOLV.SetObjects(_initialObjectsList.OrderBy(data => data.FilterDispertionLevel).ToList());
            }

            // apply the filter, need to match the filter + need to be an active type (Selector button activated)
            fastOLV.ModelFilter = new ModelFilter(FilterPredicate);

            // update total items
            TotalItems = ((ArrayList)fastOLV.FilteredObjects).Count;
            nbitems.Text = TotalItems + StrItems;

            // if the selected row is > to number of items, then there will be a unselect
            if (fastOLV.SelectedIndex == -1) fastOLV.SelectedIndex = 0;
            if (fastOLV.SelectedIndex >= 0)
                fastOLV.EnsureVisible(fastOLV.SelectedIndex);
        }

        /// <summary>
        /// if true, the item isn't filtered
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static bool FilterPredicate(object o) {
            var compData = (FileListItem)o;
            bool output = false;
            // check for the filter match, the activated category,
            if (compData.FilterFullyMatch && _displayedTypes.ContainsKey(compData.Type))
                output = _displayedTypes[compData.Type].Activated;

            return output;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Get the current selected item
        /// </summary>
        /// <returns></returns>
        public FileListItem GetCurrentFile() {
            try {
                return (FileListItem)fastOLV.SelectedItem.RowObject;
            } catch (Exception x) {
                ErrorHandler.DirtyLog(x);
            }
            return null;
        }

        internal void Redraw() {
            fastOLV.Invalidate();
        }

        /// <summary>
        /// Explicit
        /// </summary>
        public void GiveFocustoTextBox() {
            textFilter.Focus();
        }

        /// <summary>
        /// Explicit
        /// </summary>
        public void ClearFilter() {
            textFilter.Text = "";
            FilterByText = "";
        }

        #endregion

        #endregion

        #region File list buttons events

        private void BtGotoDirOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            if (DirectoryToExplorer == 0)
                Utils.OpenFolder(ProEnvironment.Current.BaseLocalPath);
            else if (DirectoryToExplorer == 1)
                Utils.OpenFolder(ProEnvironment.Current.BaseCompilationPath);
        }

        private void BtDirectoryOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            if (_isListing) return;
            DirectoryToExplorer++;
            if (DirectoryToExplorer > 3) DirectoryToExplorer = 0;
            Image tryImg = (Image)ImageResources.ResourceManager.GetObject("ExplorerDir" + DirectoryToExplorer);
            btDirectory.BackGrndImage = tryImg ?? ImageResources.Error;
            btDirectory.Invalidate();
            lbDirectory.Text = _explorerDirStr[DirectoryToExplorer];
            if (DirectoryToExplorer > 1)
                btGotoDir.Hide();
            else
                btGotoDir.Show();
            RefreshFileList();

            GiveFocustoTextBox();
        }

        private void TextFilterOnTextChanged(object sender, EventArgs eventArgs) {
            FilterByText = textFilter.Text;
        }

        private void BtEraseOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            textFilter.Text = "";
            FilterByText = "";
            textFilter.Invalidate();

            GiveFocustoTextBox();
        }

        private void BtRefreshOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            RefreshFileList();

            GiveFocustoTextBox();
        }

        private void TextFilterOnKeyDown(object sender, KeyEventArgs keyEventArgs) {
            keyEventArgs.Handled = OnKeyDown(keyEventArgs.KeyCode);
        }

        #endregion

        #region Current file

        private int _currentOperation = - 1;

        private void FilesInfoOnUpdatedOperation(UpdatedOperationEventArgs updatedOperationEventArgs) {

            if (_currentOperation == (int) updatedOperationEventArgs.CurrentOperation)
                return;

            // status text, take the last flag found
            foreach (var name in Enum.GetNames(typeof(CurrentOperation))) {
                CurrentOperation flag = (CurrentOperation)Enum.Parse(typeof(CurrentOperation), name);
                if (updatedOperationEventArgs.CurrentOperation.HasFlag(flag)) {
                    lbStatus.Text = ((DisplayAttr) flag.GetAttributes()).Name;
                }
            }

            // blink back color
            lbStatus.UseCustomBackColor = true;
            Transition.run(lbStatus, "BackColor", ThemeManager.Current.FormBack, ThemeManager.Current.AccentColor, new TransitionType_Flash(3, 400), (o, args) => { lbStatus.UseCustomBackColor = false; });

            _currentOperation = (int)updatedOperationEventArgs.CurrentOperation;
        }

        private void FilesInfoOnUpdatedErrors(UpdatedErrorsEventArgs updatedErrorsEventArgs) {

            lbNbErrors.UseCustomBackColor = true;
            lbNbErrors.UseCustomForeColor = true;
            var t = new Transition(new TransitionType_Linear(500));

            // disable/enable buttons
            UpdateErrorButtons(updatedErrorsEventArgs.NbErrors > 0);

            // colors
            if (Style.BgErrorLevelColors != null && Style.BgErrorLevelColors.Count > 0) {
                t.add(lbNbErrors, "BackColor", Style.BgErrorLevelColors[(int)updatedErrorsEventArgs.ErrorLevel]);
                t.add(lbNbErrors, "ForeColor", Style.FgErrorLevelColors[(int)updatedErrorsEventArgs.ErrorLevel]);
            }

            // text
            t.add(lbNbErrors, "Text", updatedErrorsEventArgs.NbErrors.ToString());
            t.add(lbErrorText, "Text", ((DisplayAttr)updatedErrorsEventArgs.ErrorLevel.GetAttributes()).Name);

            t.run();
        }

        private void UpdateErrorButtons(bool activate) {
            btPrevError.Enabled = activate;
            btNextError.Enabled = activate;
            btClearAllErrors.Enabled = activate;
            btPrevError.BackGrndImage = activate ? ImageResources.Previous : Utils.MakeGrayscale3(ImageResources.Previous);
            btNextError.BackGrndImage = activate ? ImageResources.Next : Utils.MakeGrayscale3(ImageResources.Next);
            btClearAllErrors.BackGrndImage = activate ? ImageResources.ClearAll : Utils.MakeGrayscale3(ImageResources.ClearAll);
            btPrevError.Invalidate();
            btNextError.Invalidate();
            btClearAllErrors.Invalidate();
        }

        private void BtClearAllErrorsOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            FilesInfo.ClearAllErrors();
            Npp.GrabFocus();
        }

        private void BtNextErrorOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            FilesInfo.GoToNextError(Npp.Line.CurrentLine + 1);
        }

        private void BtPrevErrorOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            FilesInfo.GoToPrevError(Npp.Line.CurrentLine - 1);
        }

        private void BtGetHelpOnButtonPressed(object sender, EventArgs buttonPressedEventArgs) {
            Config.Instance.GlobalShowDetailedHelpForErrors = !Config.Instance.GlobalShowDetailedHelpForErrors;
            btGetHelp.BackGrndImage = (Config.Instance.GlobalShowDetailedHelpForErrors) ? ImageResources.GetHelp : Utils.MakeGrayscale3(ImageResources.GetHelp);
            FilesInfo.ClearAnnotationsAndMarkers();
            FilesInfo.UpdateErrorsInScintilla();
            Npp.GrabFocus();
        }

        #endregion

    }

    #region sorting
    /// <summary>
    /// Class used in objectlist.Sort method
    /// </summary>
    internal class FilesSortingClass : IComparer<FileListItem> {
        public int Compare(FileListItem x, FileListItem y) {
            // first, the favourite
            int compare = x.Flags.HasFlag(FileFlag.Favourite).CompareTo(y.Flags.HasFlag(FileFlag.Favourite));
            if (compare != 0) return compare;

            // then the folders
            compare = y.Type.Equals(FileType.Folder).CompareTo(x.Type.Equals(FileType.Folder));
            if (compare != 0) return compare;

            // then the non read only
            compare = y.Flags.HasFlag(FileFlag.ReadOnly).CompareTo(x.Flags.HasFlag(FileFlag.ReadOnly));
            if (compare != 0) return compare;

            // sort by FileName
            return string.Compare(x.DisplayText, y.DisplayText, StringComparison.CurrentCultureIgnoreCase);
        }
    }
    #endregion
}