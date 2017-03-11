#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileListItem.cs) is part of 3P.
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
using System.Collections.Generic;
using System.Drawing;
using YamuiFramework.Controls.YamuiList;
using _3PA.Images;
using _3PA.Lib;

namespace _3PA.MainFeatures.FileExplorer {
    /// <summary>
    /// Object describing a file
    /// </summary>
    internal class FileListItem : FilteredTypeTreeListItem {
        public string BasePath { get; set; }
        public string FullPath { get; set; }
        public DateTime ModifieDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
        public long Size { get; set; }
        public FileType Type { get; set; }
        public FileFlag Flag { get; set; }
        public string SubString { get; set; }

        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public override string DisplayText { get; set; }

        /// <summary>
        /// return the image to display for this item
        /// If null, the image corresponding to ItemTypeImage will be used instead
        /// </summary>
        public override Image ItemImage {
            get { return null; }
        }

        /// <summary>
        /// return this item type (a unique int for each item type)
        /// if the value is strictly inferior to 0, the button for this type will not appear
        /// on the bottom of list
        /// </summary>
        public override int ItemType {
            get { return (int) Type; }
        }

        /// <summary>
        /// return the image that will be used to identify this item
        /// type, it will be used for the bottom buttons of the list
        /// All items of a given type should return the same image! The image used for the 
        /// bottom buttons will be that of the first item found for the given type
        /// </summary>
        public override Image ItemTypeImage {
            get { return Utils.GetImageFromStr(Utils.GetExtensionImage(Type.ToString(), true)); }
        }

        /// <summary>
        /// The text that describes this item type
        /// </summary>
        public override string ItemTypeText {
            get { return "Category : <span class='SubTextColor'><b>." + ((FileType) ItemType).ToString().ToLower() + " file</b></span><br><br>"; }
        }

        /// <summary>
        /// return true if the item is to be highlighted
        /// </summary>
        public override bool IsRowHighlighted {
            get { return false; }
        }

        /// <summary>
        /// return a string containing the subtext to display
        /// </summary>
        public override string SubText {
            get { return SubString; }
        }

        /// <summary>
        /// return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public override List<Image> TagImages {
            get {
                var outList = new List<Image>();
                foreach (var name in Enum.GetNames(typeof(FileFlag))) {
                    FileFlag flag = (FileFlag) Enum.Parse(typeof(FileFlag), name);
                    if (flag == 0 || !Flag.HasFlag(flag)) continue;

                    Image tryImg = (Image) ImageResources.ResourceManager.GetObject(name);
                    if (tryImg != null)
                        outList.Add(tryImg);
                }
                return outList;
            }
        }

        /// <summary>
        /// to override, that should return the list of the children for this item (if any) or null
        /// </summary>
        public override List<FilteredTypeTreeListItem> Children { get; set; }

        // make all items expanded by default
        public FileListItem() {
            IsExpanded = true;
        }
    }

    /// <summary>
    /// Type of an object file (depends on the file's extension),
    /// corresponds to an icon that appends "Type" to the enum name,
    /// for example the icon for R files is named RType.png
    /// </summary>
    internal enum FileType {
        Unknow,
        Cls,
        Df,
        D,
        Folder,
        I,
        Lst,
        P,
        Pl,
        R,
        T,
        W,
        Xml,
        Xref,
        Zip
    }

    /// <summary>
    /// File's flags,
    /// Same as other flag, corresponds to an icon with the same name as in the enumeration
    /// </summary>
    [Flags]
    internal enum FileFlag {
        /// <summary>
        /// Is the file starred by the user
        /// </summary>
        Favourite = 1,
        ReadOnly = 2
    }
}