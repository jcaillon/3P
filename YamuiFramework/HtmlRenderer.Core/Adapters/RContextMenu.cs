#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (RContextMenu.cs) is part of YamuiFramework.
// 
// YamuiFramework is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// YamuiFramework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with YamuiFramework. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using YamuiFramework.HtmlRenderer.Core.Adapters.Entities;

namespace YamuiFramework.HtmlRenderer.Core.Adapters
{
    /// <summary>
    /// Adapter for platform specific context menu - used to create and show context menu at specific location.<br/>
    /// Not relevant for platforms that don't render HTML on UI element.
    /// </summary>
    public abstract class RContextMenu : IDisposable
    {
        /// <summary>
        /// The total number of items in the context menu
        /// </summary>
        public abstract int ItemsCount { get; }

        /// <summary>
        /// Add divider item to the context menu.<br />
        /// The divider is a non clickable place holder used to separate items.
        /// </summary>
        public abstract void AddDivider();

        /// <summary>
        /// Add item to the context menu with the given text that will raise the given event when clicked.
        /// </summary><param name="text">the text to set on the new context menu item</param><param name="enabled">if to set the item as enabled or disabled</param><param name="onClick">the event to raise when the item is clicked</param>
        public abstract void AddItem(string text, bool enabled, EventHandler onClick);

        /// <summary>
        /// Remove the last item from the context menu iff it is a divider
        /// </summary>
        public abstract void RemoveLastDivider();

        /// <summary>
        /// Show the context menu in the given parent control at the given location.
        /// </summary><param name="parent">the parent control to show in</param><param name="location">the location to show at relative to the parent control</param>
        public abstract void Show(RControl parent, RPoint location);

        public abstract void Dispose();
    }
}