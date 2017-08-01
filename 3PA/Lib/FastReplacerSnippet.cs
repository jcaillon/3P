using System;
using System.Collections.Generic;
using System.Text;

namespace _3PA.Lib {
    internal class FastReplacerSnippet {
        private class InnerSnippet {
            public FastReplacerSnippet Snippet;
            public int Start; // Position of the snippet in parent snippet's Text.
            public int End; // Position of the snippet in parent snippet's Text.
            public int Order1; // Order of snippets with a same Start position in their parent.
            public int Order2; // Order of snippets with a same Start position and Order1 in their parent.

            public override string ToString() {
                return "InnerSnippet: " + Snippet.Text;
            }
        }

        public readonly string Text;
        private readonly List<InnerSnippet> _innerSnippets;

        public FastReplacerSnippet(string text) {
            Text = text;
            _innerSnippets = new List<InnerSnippet>();
        }

        public override string ToString() {
            return "Snippet: " + Text;
        }

        public void Append(FastReplacerSnippet snippet) {
            _innerSnippets.Add(new InnerSnippet {
                Snippet = snippet,
                Start = Text.Length,
                End = Text.Length,
                Order1 = 1,
                Order2 = _innerSnippets.Count
            });
        }

        public void Replace(int start, int end, FastReplacerSnippet snippet) {
            _innerSnippets.Add(new InnerSnippet {
                Snippet = snippet,
                Start = start,
                End = end,
                Order1 = 0,
                Order2 = 0
            });
        }

        public void InsertBefore(int start, FastReplacerSnippet snippet) {
            _innerSnippets.Add(new InnerSnippet {
                Snippet = snippet,
                Start = start,
                End = start,
                Order1 = 2,
                Order2 = _innerSnippets.Count
            });
        }

        public void InsertAfter(int end, FastReplacerSnippet snippet) {
            _innerSnippets.Add(new InnerSnippet {
                Snippet = snippet,
                Start = end,
                End = end,
                Order1 = 1,
                Order2 = _innerSnippets.Count
            });
        }

        public void ToString(StringBuilder sb) {
            _innerSnippets.Sort(delegate (InnerSnippet a, InnerSnippet b) {
                if (a == b) return 0;
                if (a.Start != b.Start) return a.Start - b.Start;
                if (a.End != b.End) return a.End - b.End; // Disambiguation if there are inner snippets inserted before a token (they have End==Start) go before inner snippets inserted instead of a token (End>Start).
                if (a.Order1 != b.Order1) return a.Order1 - b.Order1;
                if (a.Order2 != b.Order2) return a.Order2 - b.Order2;
                throw new InvalidOperationException(string.Format(
                    "Internal error: Two snippets have ambigous order. At position from {0} to {1}, order1 is {2}, order2 is {3}. First snippet is \"{4}\", second snippet is \"{5}\".",
                    a.Start, a.End, a.Order1, a.Order2, a.Snippet.Text, b.Snippet.Text));
            });
            int lastPosition = 0;
            foreach (InnerSnippet innerSnippet in _innerSnippets) {
                if (innerSnippet.Start < lastPosition)
                    throw new InvalidOperationException(string.Format(
                        "Internal error: Token is overlapping with a previous token. Overlapping token is from position {0} to {1}, previous token ends at position {2} in snippet \"{3}\".",
                        innerSnippet.Start, innerSnippet.End, lastPosition, Text));
                sb.Append(Text, lastPosition, innerSnippet.Start - lastPosition);
                innerSnippet.Snippet.ToString(sb);
                lastPosition = innerSnippet.End;
            }
            sb.Append(Text, lastPosition, Text.Length - lastPosition);
        }

        public int GetLength() {
            int len = Text.Length;
            foreach (var innerSnippet in _innerSnippets) {
                len -= innerSnippet.End - innerSnippet.Start;
                len += innerSnippet.Snippet.GetLength();
            }
            return len;
        }
    }
}
