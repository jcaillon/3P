#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (Enumerable.cs) is part of 3P.
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

namespace _3PA.Lib.CommonMark.Syntax {
    internal class Enumerable : IEnumerable<EnumeratorEntry> {
        private Block _root;

        public Enumerable(Block root) {
            if (root == null)
                throw new ArgumentNullException("root");

            _root = root;
        }

        public IEnumerator<EnumeratorEntry> GetEnumerator() {
            return new Enumerator(_root);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private sealed class Enumerator : IEnumerator<EnumeratorEntry> {
            private Block _root;
            private EnumeratorEntry _current;
            private Stack<BlockStackEntry> _blockStack = new Stack<BlockStackEntry>();
            private Stack<InlineStackEntry> _inlineStack = new Stack<InlineStackEntry>();

            public Enumerator(Block root) {
                _root = root;
                _blockStack.Push(new BlockStackEntry(root, null));
            }

            public EnumeratorEntry Current {
                get { return _current; }
            }

            object System.Collections.IEnumerator.Current {
                get { return _current; }
            }

#if OptimizeFor45
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif

            private bool ShouldSkip(Inline inline) {
                if (inline.Tag == InlineTag.String
                    && inline.FirstChild == null
                    && inline.LiteralContentValue.Length == 0)
                    return true;

                return false;
            }

            public bool MoveNext() {
                repeatMoveNext:

                Inline inline;
                if (_inlineStack.Count > 0) {
                    var entry = _inlineStack.Pop();
                    if (entry.NeedsClose != null) {
                        inline = entry.NeedsClose;
                        _current = new EnumeratorEntry(false, true, inline);

                        if (entry.Target != null) {
                            entry.NeedsClose = null;
                            _inlineStack.Push(entry);
                        }

                        return true;
                    }

                    if (entry.Target != null) {
                        inline = entry.Target;
                        _current = new EnumeratorEntry(true, inline.FirstChild == null, inline);

                        if (inline.FirstChild != null) {
                            _inlineStack.Push(new InlineStackEntry(inline.NextSibling, inline));
                            _inlineStack.Push(new InlineStackEntry(inline.FirstChild, null));
                        } else if (inline.NextSibling != null) {
                            _inlineStack.Push(new InlineStackEntry(inline.NextSibling, null));
                        }

                        if (ShouldSkip(_current.Inline)) {
                            goto repeatMoveNext;
                        }
                    }

                    return true;
                }

                Block block;
                if (_blockStack.Count > 0) {
                    var entry = _blockStack.Pop();
                    if (entry.NeedsClose != null) {
                        block = entry.NeedsClose;
                        _current = new EnumeratorEntry(false, true, block);

                        if (entry.Target != null) {
                            entry.NeedsClose = null;
                            _blockStack.Push(entry);
                        }

                        return true;
                    }

                    if (entry.Target != null) {
                        block = entry.Target;
                        _current = new EnumeratorEntry(true, block.FirstChild == null && block.InlineContent == null, block);

                        if (block.FirstChild != null) {
                            _blockStack.Push(new BlockStackEntry(block.NextSibling, block));
                            _blockStack.Push(new BlockStackEntry(block.FirstChild, null));
                        } else if (block.NextSibling != null && block != _root) {
                            _blockStack.Push(new BlockStackEntry(block.NextSibling, block.InlineContent == null ? null : block));
                        } else if (block.InlineContent != null) {
                            _blockStack.Push(new BlockStackEntry(null, block));
                        }

                        if (block.InlineContent != null) {
                            _inlineStack.Push(new InlineStackEntry(block.InlineContent, null));
                        }
                    }

                    return true;
                }

                return false;
            }

            public void Reset() {
                _current = null;
                _blockStack.Clear();
                _inlineStack.Clear();
            }

            void IDisposable.Dispose() {}

            private struct BlockStackEntry {
                public readonly Block Target;
                public Block NeedsClose;

                public BlockStackEntry(Block target, Block needsClose) {
                    Target = target;
                    NeedsClose = needsClose;
                }
            }

            private struct InlineStackEntry {
                public readonly Inline Target;
                public Inline NeedsClose;

                public InlineStackEntry(Inline target, Inline needsClose) {
                    Target = target;
                    NeedsClose = needsClose;
                }
            }
        }
    }
}