#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HandleManager.cs) is part of 3P.
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
using System.Collections.Generic;

namespace WixToolset.Dtf.Compression.Cab {
    /// <summary>
    /// Generic class for managing allocations of integer handles
    /// for objects of a certain type.
    /// </summary>
    /// <typeparam name="T">The type of objects the handles refer to.</typeparam>
    internal sealed class HandleManager<T> where T : class {
        /// <summary>
        /// Auto-resizing list of objects for which handles have been allocated.
        /// Each handle is just an index into this list. When a handle is freed,
        /// the list item at that index is set to null.
        /// </summary>
        private List<T> handles;

        /// <summary>
        /// Creates a new HandleManager instance.
        /// </summary>
        public HandleManager() {
            handles = new List<T>();
        }

        /// <summary>
        /// Gets the object of a handle, or null if the handle is invalid.
        /// </summary>
        /// <param name="handle">The integer handle previously allocated
        /// for the desired object.</param>
        /// <returns>The object for which the handle was allocated.</returns>
        public T this[int handle] {
            get {
                if (handle > 0 && handle <= handles.Count) {
                    return handles[handle - 1];
                }
                return null;
            }
        }

        /// <summary>
        /// Allocates a new handle for an object.
        /// </summary>
        /// <param name="obj">Object that the handle will refer to.</param>
        /// <returns>New handle that can be later used to retrieve the object.</returns>
        public int AllocHandle(T obj) {
            handles.Add(obj);
            int handle = handles.Count;
            return handle;
        }

        /// <summary>
        /// Frees a handle that was previously allocated. Afterward the handle
        /// will be invalid and the object it referred to can no longer retrieved.
        /// </summary>
        /// <param name="handle">Handle to be freed.</param>
        public void FreeHandle(int handle) {
            if (handle > 0 && handle <= handles.Count) {
                handles[handle - 1] = null;
            }
        }
    }
}