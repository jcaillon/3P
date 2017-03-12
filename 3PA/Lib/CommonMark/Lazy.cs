#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Lazy.cs) is part of 3P.
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
namespace _3PA.Lib.CommonMark {
#if v2_0 || v3_5
    enum LazyThreadSafetyMode
    {
        None,
        PublicationOnly,
        ExecutionAndPublication
    }

    class Lazy<T>
    {
        private readonly Func<T> valueFactory;
        private readonly bool isThreadSafe;
        private readonly object _lock = new object();
        private T value;

        public Lazy(Func<T> valueFactory)
            : this(valueFactory, true)
        {
        }

        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
            : this(valueFactory, mode != LazyThreadSafetyMode.None)
        {
        }

        public Lazy(Func<T> valueFactory, bool isThreadSafe)
        {
            this.valueFactory = valueFactory;
            this.isThreadSafe = isThreadSafe;
        }

        public T Value
        {
            get
            {
                if (value == null)
                {
                    if (!isThreadSafe)
                    {
                        return value = valueFactory();
                    }
                    lock (_lock)
                    {
                        if (value == null)
                        {
                            value = valueFactory();
                        }
                    }
                }
                return value;
            }
        }
    }
#endif
}